using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    [Header("Ссылка на ModeController")]
    [SerializeField] private ModeController modeController;

    private GameObject currentActiveObject;
    private readonly Dictionary<Button, UnityAction> listeners = new Dictionary<Button, UnityAction>();
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
            position = transform.localPosition;
            rotation = transform.localEulerAngles;
            scale = transform.localScale;
        }

        public void ApplyTo(Transform transform)
        {
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
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
                UnityAction action = () => OnButtonClick(index);
                listeners[buttonObjectPairs[i].button] = action;
                buttonObjectPairs[i].button.onClick.AddListener(action);
            }
        }

        if (defaultActiveIndex >= 0 && defaultActiveIndex < buttonObjectPairs.Length)
        {
            ActivateObjectsForButton(defaultActiveIndex);
            UpdateModeControllerTarget();

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
            originalMaterials[obj] = renderer.sharedMaterials;
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        ResetPreviousObject();
        ActivateObjectsForButton(buttonIndex);
        UpdateModeControllerTarget();

    }

    private void ResetPreviousObject()
    {
        if (currentActiveObject != null && objectStates.ContainsKey(currentActiveObject))
        {
            var state = objectStates[currentActiveObject];
            currentActiveObject.transform.localPosition = state.position;
            currentActiveObject.transform.localScale = state.scale;
            currentActiveObject.transform.localEulerAngles = state.rotation;

            Debug.Log($"Объект {currentActiveObject.name} сброшен (позиция и масштаб)");
        }
    }

    private void ActivateObjectsForButton(int activeIndex)
    {
        if (activeIndex < 0 || activeIndex >= buttonObjectPairs.Length) return;

        for (int i = 0; i < buttonObjectPairs.Length; i++)
        {
            var button = buttonObjectPairs[i].button;
            if (button == null) continue;
            var colors = button.colors;
            colors.normalColor = i == activeIndex ? AppPresentation.SelectedSurface : AppPresentation.Surface;
            button.colors = colors;
        }
        currentActiveObject = null;
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
                renderer.sharedMaterials = originalMaterials[obj];
            }
        }
    }

    private void SetObjectCustomMaterial(GameObject obj, Material customMaterial)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && customMaterial != null)
        {
            Material[] customMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < customMaterials.Length; i++)
            {
                customMaterials[i] = customMaterial;
            }
            renderer.sharedMaterials = customMaterials;
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

            modeController.SetTargetObject(targetObject.transform);

            Debug.Log($"Целевой объект успешно обновлен: {targetObject.name}");
        }
        else
        {
            Debug.LogWarning("Не найден активный объект для ModeController");
            modeController.SetTargetObject(null);
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
        foreach (var entry in listeners)
            if (entry.Key != null) entry.Key.onClick.RemoveListener(entry.Value);
    }

    public void ForceUpdateTarget()
    {
        UpdateModeControllerTarget();
    }
}