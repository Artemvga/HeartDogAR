using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonGroupControllerUIInformation : MonoBehaviour
{
    [System.Serializable]
    public class TransparentObject
    {
        public GameObject gameObject;
        public Material transparentMaterial;
    }

    [System.Serializable]
    public class ButtonObjectsPair
    {
        public Button button;
        public GameObject[] objectsToToggle;
        public TransparentObject[] transparentObjects;
        public GameObject[] forceDeactivateObjects;
    }

    [Header("Настройки кнопок и объектов")]
    [SerializeField] private ButtonObjectsPair[] buttonObjectPairs;
    [SerializeField] private int defaultActiveIndex = -1;

    [Header("Настройки анимации")]
    [SerializeField] private float rotationDuration = 1.0f;
    [SerializeField] private Ease rotationEase = Ease.OutBack;

    [Header("Ссылка на ModeController")]
    [SerializeField] private ModeController modeController;

    private GameObject currentActiveObject;
    private Dictionary<GameObject, ObjectState> objectStates = new Dictionary<GameObject, ObjectState>();
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();

    [System.Serializable]
    public class ObjectState
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public ObjectState(Transform transform)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;
        }

        public void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.localScale = scale;
        }
    }

    private void Start()
    {
        if (modeController == null)
        {
            modeController = FindObjectOfType<ModeController>();
            if (modeController == null)
            {
                Debug.LogError("ButtonGroupController: ModeController не найден!");
                return;
            }
        }

        SaveInitialStates();
        SaveOriginalMaterials();

        for (int i = 0; i < buttonObjectPairs.Length; i++)
        {
            int index = i;
            if (buttonObjectPairs[i].button != null)
            {
                buttonObjectPairs[i].button.onClick.AddListener(() => OnButtonClick(index));
            }
        }

        if (defaultActiveIndex >= 0 && defaultActiveIndex < buttonObjectPairs.Length)
        {
            ActivateObjectsForButton(defaultActiveIndex);
            UpdateModeControllerTarget();

            // ПОВОРОТ ПЕРВОГО ОБЪЕКТА ПРИ ЗАГРУЗКЕ
            RotateFirstObject();
        }
    }

    private void RotateFirstObject()
    {
        if (currentActiveObject != null)
        {
            Transform targetTransform = currentActiveObject.transform;

            // ПОВОРОТ НА 360 ГРАДУСОВ по оси Y
            Vector3 currentRotation = targetTransform.eulerAngles;
            Vector3 targetRotation = new Vector3(currentRotation.x, currentRotation.y + 360f, currentRotation.z);

            // Выполняем анимацию поворота с помощью DOTween
            targetTransform.DORotate(targetRotation, rotationDuration, RotateMode.FastBeyond360)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    Debug.Log($"Поворот объекта {currentActiveObject.name} на 360° завершен");
                });

            Debug.Log($"Запущен поворот объекта {currentActiveObject.name} на 360 градусов");
        }
    }

    private void SaveInitialStates()
    {
        foreach (var pair in buttonObjectPairs)
        {
            foreach (GameObject obj in pair.objectsToToggle)
            {
                if (obj != null && !objectStates.ContainsKey(obj))
                {
                    objectStates[obj] = new ObjectState(obj.transform);
                }
            }

            foreach (TransparentObject transparentObj in pair.transparentObjects)
            {
                if (transparentObj.gameObject != null && !objectStates.ContainsKey(transparentObj.gameObject))
                {
                    objectStates[transparentObj.gameObject] = new ObjectState(transparentObj.gameObject.transform);
                }
            }

            foreach (GameObject obj in pair.forceDeactivateObjects)
            {
                if (obj != null && !objectStates.ContainsKey(obj))
                {
                    objectStates[obj] = new ObjectState(obj.transform);
                }
            }
        }
    }

    private void SaveOriginalMaterials()
    {
        foreach (var pair in buttonObjectPairs)
        {
            foreach (GameObject obj in pair.objectsToToggle)
            {
                if (obj != null)
                {
                    SaveObjectMaterials(obj);
                }
            }

            foreach (TransparentObject transparentObj in pair.transparentObjects)
            {
                if (transparentObj.gameObject != null)
                {
                    SaveObjectMaterials(transparentObj.gameObject);
                }
            }

            foreach (GameObject obj in pair.forceDeactivateObjects)
            {
                if (obj != null)
                {
                    SaveObjectMaterials(obj);
                }
            }
        }
    }

    private void SaveObjectMaterials(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && !originalMaterials.ContainsKey(obj))
        {
            originalMaterials[obj] = renderer.materials;
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        ResetPreviousObject();
        ActivateObjectsForButton(buttonIndex);
        UpdateModeControllerTarget();

        // ПОВОРОТ НА 360° ПРИ КАЖДОМ НАЖАТИИ НА КНОПКУ
        RotateFirstObject();
    }

    private void ResetPreviousObject()
    {
        if (currentActiveObject != null && objectStates.ContainsKey(currentActiveObject))
        {
            var state = objectStates[currentActiveObject];
            currentActiveObject.transform.position = state.position;
            currentActiveObject.transform.localScale = state.scale;

            Debug.Log($"Объект {currentActiveObject.name} сброшен (позиция и масштаб)");
        }
    }

    private void ActivateObjectsForButton(int activeIndex)
    {
        if (activeIndex < 0 || activeIndex >= buttonObjectPairs.Length) return;

        bool firstObjectSet = false;
        foreach (GameObject obj in buttonObjectPairs[activeIndex].objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                SetObjectTransparency(obj, false);

                if (!firstObjectSet)
                {
                    currentActiveObject = obj;
                    firstObjectSet = true;
                    Debug.Log($"Установлен целевой объект для ModeController: {obj.name}");
                }
                Debug.Log($"Активирован основной объект: {obj.name}");
            }
        }

        foreach (TransparentObject transparentObj in buttonObjectPairs[activeIndex].transparentObjects)
        {
            if (transparentObj.gameObject != null)
            {
                transparentObj.gameObject.SetActive(true);
                SetObjectCustomMaterial(transparentObj.gameObject, transparentObj.transparentMaterial);
                Debug.Log($"Активирован прозрачный объект: {transparentObj.gameObject.name}");
            }
        }

        foreach (GameObject obj in buttonObjectPairs[activeIndex].forceDeactivateObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                SetObjectTransparency(obj, false);
                Debug.Log($"Деактивирован объект: {obj.name}");
            }
        }
    }

    private void SetObjectTransparency(GameObject obj, bool transparent)
    {
        if (originalMaterials.ContainsKey(obj))
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.materials = originalMaterials[obj];
            }
        }
    }

    private void SetObjectCustomMaterial(GameObject obj, Material customMaterial)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && customMaterial != null)
        {
            Material[] customMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < customMaterials.Length; i++)
            {
                customMaterials[i] = customMaterial;
            }
            renderer.materials = customMaterials;
        }
    }

    private void UpdateModeControllerTarget()
    {
        if (modeController == null)
        {
            Debug.LogError("ModeController не назначен!");
            return;
        }

        GameObject targetObject = currentActiveObject;
        if (targetObject == null)
        {
            targetObject = FindFirstActiveObject();
        }

        if (targetObject != null)
        {
            Debug.Log($"Обновление целевого объекта в ModeController: {targetObject.name}");

            modeController.targetObject = targetObject.transform;
            modeController.UpdateTargetTransform();

            Debug.Log($"Целевой объект успешно обновлен: {targetObject.name}");
        }
        else
        {
            Debug.LogWarning("Не найден активный объект для ModeController");
            modeController.targetObject = null;
        }
    }

    private GameObject FindFirstActiveObject()
    {
        foreach (var pair in buttonObjectPairs)
        {
            foreach (GameObject obj in pair.objectsToToggle)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    return obj;
                }
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        foreach (var pair in buttonObjectPairs)
        {
            if (pair.button != null)
            {
                pair.button.onClick.RemoveAllListeners();
            }
        }
    }

    public void ForceUpdateTarget()
    {
        UpdateModeControllerTarget();
    }
}