using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public enum ControlMode { Rotation, Scale }

public class ModeController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IScrollHandler
{
    [Header("Целевой объект")]
    public Transform targetObject;

    [Header("Компоненты контроллеров")]
    public RotationController rotationController;
    public ScaleController scaleController;

    [Header("UI Элементы")]
    public Button resetButton;
    public Button invertButton;
    public Button mainModeSwitchButton;
    public TextMeshProUGUI mainModeText;

    [Header("Кнопки точного вращения")]
    public Button rotateUpButton;
    public Button rotateDownButton;
    public Button rotateLeftButton;
    public Button rotateRightButton;

    [Header("Кнопки масштабирования")]
    public Button scaleUpButton;
    public Button scaleDownButton;

    [Header("Настройки режимов")]
    public string rotationModeText = "Вращение";
    public string scaleModeText = "Приближение";
    public Color rotationModeColor = Color.blue;
    public Color scaleModeColor = Color.red;

    [Header("Настройки точного управления")]
    public float preciseRotationSpeed = 2.5f;
    public float preciseScaleSpeed = 0.025f;
    public float buttonHoldInterval = 0.1f;

    private ControlMode currentMode = ControlMode.Rotation;
    private bool isButtonHeld = false;
    private System.Action currentHoldAction;
    private Coroutine holdRoutine;
    private Coroutine resetRoutine;

    bool pinching;
    float previousPinchDistance;
    readonly System.Collections.Generic.List<RaycastResult> gestureHits = new System.Collections.Generic.List<RaycastResult>();

    void Update()
    {
        if (Input.touchCount != 2) { pinching = false; return; }
        var a = Input.GetTouch(0); var b = Input.GetTouch(1);
        if (!pinching)
        {
            if (!IsGestureSurface(a.position) || !IsGestureSurface(b.position)) return;
            StopReset();
            rotationController?.EndRotation(); scaleController?.EndScaling();
            previousPinchDistance = Vector2.Distance(a.position, b.position);
            pinching = true;
            return;
        }
        float distance = Vector2.Distance(a.position, b.position);
        if (previousPinchDistance > 1f && distance > 1f)
            scaleController?.ZoomByFactor(distance / previousPinchDistance);
        previousPinchDistance = distance;
    }

    bool IsGestureSurface(Vector2 position)
    {
        if (EventSystem.current == null) return false;
        gestureHits.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, gestureHits);
        return gestureHits.Count > 0 && gestureHits[0].gameObject == gameObject;
    }

    public void OnScroll(PointerEventData eventData)
    {
        StopReset();
        scaleController?.ZoomByFactor(Mathf.Exp(eventData.scrollDelta.y * .15f));
    }

    void Start()
    {
        InitializeSystem();
    }

    void InitializeSystem()
    {
        SetupUI();
        UpdateMode();

        if (targetObject != null)
        {
            ForceInitializeControllers();
        }
    }

    void ForceInitializeControllers()
    {
        if (rotationController != null)
        {
            rotationController.SetTargetObject(targetObject);
            rotationController.Initialize();
        }

        if (scaleController != null)
        {
            scaleController.SetTargetObject(targetObject);
            scaleController.Initialize();
        }
    }

    void SetupUI()
    {
        // Основные кнопки
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetButtonClick);
        }

        if (invertButton != null)
        {
            invertButton.onClick.RemoveAllListeners();
            invertButton.onClick.AddListener(OnInvertButtonClick);
        }

        if (mainModeSwitchButton != null)
        {
            mainModeSwitchButton.onClick.RemoveAllListeners();
            mainModeSwitchButton.onClick.AddListener(OnMainModeSwitchButtonClick);
        }

        // Кнопки точного вращения (всегда активны в режиме вращения)
        SetupPreciseRotationButtons();

        // Кнопки масштабирования
        SetupScaleButtons();

        SetupTouchPanel();
    }

    void SetupPreciseRotationButtons()
    {
        if (rotateUpButton != null)
        {
            rotateUpButton.onClick.RemoveAllListeners();
            rotateUpButton.onClick.AddListener(() => RotatePrecise(Vector3.right));
            SetupButtonHold(rotateUpButton, () => RotatePreciseContinuous(Vector3.right));
        }

        if (rotateDownButton != null)
        {
            rotateDownButton.onClick.RemoveAllListeners();
            rotateDownButton.onClick.AddListener(() => RotatePrecise(Vector3.left));
            SetupButtonHold(rotateDownButton, () => RotatePreciseContinuous(Vector3.left));
        }

        if (rotateLeftButton != null)
        {
            rotateLeftButton.onClick.RemoveAllListeners();
            rotateLeftButton.onClick.AddListener(() => RotatePrecise(Vector3.up));
            SetupButtonHold(rotateLeftButton, () => RotatePreciseContinuous(Vector3.up));
        }

        if (rotateRightButton != null)
        {
            rotateRightButton.onClick.RemoveAllListeners();
            rotateRightButton.onClick.AddListener(() => RotatePrecise(Vector3.down));
            SetupButtonHold(rotateRightButton, () => RotatePreciseContinuous(Vector3.down));
        }
    }

    void SetupScaleButtons()
    {
        if (scaleUpButton != null)
        {
            scaleUpButton.onClick.RemoveAllListeners();
            scaleUpButton.onClick.AddListener(() => ScalePrecise(1f));
            SetupButtonHold(scaleUpButton, () => ScalePreciseContinuous(1f));
        }

        if (scaleDownButton != null)
        {
            scaleDownButton.onClick.RemoveAllListeners();
            scaleDownButton.onClick.AddListener(() => ScalePrecise(-1f));
            SetupButtonHold(scaleDownButton, () => ScalePreciseContinuous(-1f));
        }
    }

    void SetupButtonHold(Button button, System.Action holdAction)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();


        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {
            StartButtonHold(holdAction);
        });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            StopButtonHold();
        });
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            StopButtonHold();
        });
        trigger.triggers.Add(pointerExit);
    }

    void StartButtonHold(System.Action action)
    {
        if (isButtonHeld) return;

        isButtonHeld = true;
        currentHoldAction = action;

        holdRoutine = StartCoroutine(ButtonHoldCoroutine());
    }

    void StopButtonHold()
    {
        isButtonHeld = false;
        currentHoldAction = null;
        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = null;
    }

    IEnumerator ButtonHoldCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        while (isButtonHeld)
        {
            currentHoldAction?.Invoke();
            yield return new WaitForSecondsRealtime(Mathf.Max(0.03f, buttonHoldInterval));
        }
    }

    void RotatePrecise(Vector3 direction)
    {
        if (targetObject == null || rotationController == null) return;

        float directionMultiplier = rotationController.GetInvertState() ? -1f : 1f;
        Vector3 rotation = direction * preciseRotationSpeed * directionMultiplier;

        StopReset();
        targetObject.Rotate(rotation, Space.Self);
        rotationController.ForceUpdateRotation();
    }

    void RotatePreciseContinuous(Vector3 direction)
    {
        if (targetObject == null || rotationController == null) return;

        float directionMultiplier = rotationController.GetInvertState() ? -1f : 1f;
        Vector3 rotation = direction * preciseRotationSpeed * 0.5f * directionMultiplier;

        StopReset();
        targetObject.Rotate(rotation, Space.Self);
        rotationController.ForceUpdateRotation();
    }

    void ScalePrecise(float direction)
    {
        if (targetObject == null || scaleController == null) return;

        float scaleChange = direction * preciseScaleSpeed;
        ApplyScaleChange(scaleChange);
    }

    void ScalePreciseContinuous(float direction)
    {
        if (targetObject == null || scaleController == null) return; // ИСПРАВЛЕНО: было scaleController != null

        float scaleChange = direction * preciseScaleSpeed * 0.5f;
        ApplyScaleChange(scaleChange);
    }

    void ApplyScaleChange(float scaleChange)
    {
        StopReset();
        Vector3 newScale = targetObject.localScale * (1f + scaleChange);

        if (scaleController != null)
        {
            float minScale = scaleController.GetInitialScale().x * scaleController.minScale;
            float maxScale = scaleController.GetInitialScale().x * scaleController.maxScale;
            float clampedScale = Mathf.Clamp(newScale.x, minScale, maxScale);

            Vector3 initial = scaleController.GetInitialScale();
            if (Mathf.Approximately(initial.x, 0)) return;
            targetObject.localScale = initial * (clampedScale / initial.x);
            scaleController.ForceUpdateScale();
        }
    }

    void OnDisable()
    {
        pinching = false;
        StopButtonHold();
        StopReset();
        rotationController?.EndRotation();
        scaleController?.EndScaling();
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) StopButtonHold();
    }

    void StopReset()
    {
        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = null;
    }

    void UpdateMode()
    {
        if (rotationController != null)
            rotationController.SetActive(currentMode == ControlMode.Rotation);

        if (scaleController != null)
            scaleController.SetActive(currentMode == ControlMode.Scale);

        UpdateUI();
    }

    void UpdateUI()
    {
        UpdateMainModeText();
        UpdateInvertButton();
        UpdatePreciseControls();
    }

    void UpdateMainModeText()
    {
        if (mainModeText != null)
        {
            mainModeText.text = currentMode == ControlMode.Rotation ? rotationModeText : scaleModeText;
            mainModeText.color = currentMode == ControlMode.Rotation ? rotationModeColor : scaleModeColor;
        }

        if (mainModeSwitchButton != null)
        {
            TextMeshProUGUI buttonText = mainModeSwitchButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = currentMode == ControlMode.Rotation ? scaleModeText : rotationModeText;
            }
        }
    }

    void UpdateInvertButton()
    {
        if (invertButton != null && rotationController != null)
        {
            invertButton.gameObject.SetActive(currentMode == ControlMode.Rotation);

            if (currentMode == ControlMode.Rotation)
            {
                Image invertImage = invertButton.GetComponent<Image>();
                if (invertImage != null)
                {
                    invertImage.color = Color.white;
                    var colors = invertButton.colors;
                    colors.normalColor = rotationController.GetInvertButtonColor();
                    invertButton.colors = colors;
                }

                TextMeshProUGUI invertText = invertButton.GetComponentInChildren<TextMeshProUGUI>();
                if (invertText != null)
                {
                    invertText.text = rotationController.GetInvertState() ? "Инверсия: Вкл" : "Инверсия: Выкл";
                }
            }
        }
    }

    void UpdatePreciseControls()
    {
        bool isRotationMode = currentMode == ControlMode.Rotation;

        // Rotation uses a direct drag; the old arrow buttons stay hidden.
        if (rotateUpButton != null) rotateUpButton.gameObject.SetActive(false);
        if (rotateDownButton != null) rotateDownButton.gameObject.SetActive(false);
        if (rotateLeftButton != null) rotateLeftButton.gameObject.SetActive(false);
        if (rotateRightButton != null) rotateRightButton.gameObject.SetActive(false);

        if (scaleUpButton != null) scaleUpButton.gameObject.SetActive(!isRotationMode);
        if (scaleDownButton != null) scaleDownButton.gameObject.SetActive(!isRotationMode);
    }

    void SetupTouchPanel()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0, 0, 0, 0.01f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetObject == null || Input.touchCount > 1 || pinching) return;
        StopReset();

        if (currentMode == ControlMode.Rotation && rotationController != null)
        {
            rotationController.StartRotation(eventData.position);
        }
        else if (currentMode == ControlMode.Scale && scaleController != null)
        {
            scaleController.StartScaling(eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetObject == null || Input.touchCount > 1 || pinching) return;

        if (currentMode == ControlMode.Rotation && rotationController != null)
        {
            rotationController.ProcessRotation(eventData.position);
        }
        else if (currentMode == ControlMode.Scale && scaleController != null)
        {
            scaleController.ProcessScaling(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetObject == null) return;

        if (currentMode == ControlMode.Rotation && rotationController != null)
        {
            rotationController.EndRotation();
        }
        else if (currentMode == ControlMode.Scale && scaleController != null)
        {
            scaleController.EndScaling();
        }
    }

    void OnResetButtonClick()
    {
        if (targetObject == null) return;

        StopButtonHold();
        StopReset();
        resetRoutine = StartCoroutine(SmoothReset());
    }

    IEnumerator SmoothReset()
    {
        if (currentMode == ControlMode.Rotation && rotationController != null)
        {
            yield return rotationController.SmoothResetRotation();
        }
        else if (currentMode == ControlMode.Scale && scaleController != null)
        {
            yield return scaleController.SmoothResetScale();
        }
    }

    void OnInvertButtonClick()
    {
        if (targetObject == null) return;

        if (currentMode == ControlMode.Rotation && rotationController != null)
        {
            rotationController.ToggleInversion();
            UpdateInvertButton();
        }
    }

    void OnMainModeSwitchButtonClick()
    {
        SwitchControlMode();
    }

    public void SwitchControlMode()
    {
        StopButtonHold();
        StopReset();
        currentMode = currentMode == ControlMode.Rotation ? ControlMode.Scale : ControlMode.Rotation;
        UpdateMode();
    }

    public void SetTargetObject(Transform newTarget)
    {
        pinching = false;
        StopButtonHold();
        StopReset();
        targetObject = newTarget;
        rotationController?.EndRotation();
        scaleController?.EndScaling();
        ForceInitializeControllers();
    }

    public void UpdateTargetTransform() => SetTargetObject(targetObject);

    public ControlMode GetCurrentControlMode() => currentMode;
    public Transform GetTargetObject() => targetObject;
}