using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CyclicLoadingSystem : MonoBehaviour
{
    [System.Serializable]
    public class LoadingStep
    {
        public string message;          // Текст для отображения
        public float duration;         // Длительность шага в секундах
        public GameObject objectToActivate; // Объект для активации
    }

    [Header("Шаги загрузки")]
    [SerializeField] private LoadingStep[] loadingSteps;

    [Header("UI элементы")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Настройки")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private float timeScale = 1f;

    private float currentStepTime;
    private int currentStepIndex;
    private bool isRunning = false;

    void Start()
    {
        InitializeUI();

        if (startOnAwake)
        {
            StartLoading();
        }
    }

    void Update()
    {
        if (isRunning)
        {
            UpdateLoadingProgress();
        }
    }

    void InitializeUI()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        if (loadingText != null)
            loadingText.text = "Ожидание запуска...";
    }

    void UpdateLoadingProgress()
    {
        if (loadingSteps == null || loadingSteps.Length == 0) return;

        var currentStep = loadingSteps[currentStepIndex];

        // Обновляем время текущего шага
        currentStepTime += Time.deltaTime * timeScale;

        // Вычисляем прогресс текущего шага (0-1)
        float stepProgress = currentStepTime / currentStep.duration;

        // Обновляем UI
        UpdateUI(stepProgress);

        // Проверяем, закончился ли текущий шаг
        if (currentStepTime >= currentStep.duration)
        {
            MoveToNextStep();
        }
    }

    void UpdateUI(float stepProgress)
    {
        var currentStep = loadingSteps[currentStepIndex];

        // Обновляем текст
        if (loadingText != null)
            loadingText.text = currentStep.message;

        // Обновляем слайдер (прогресс внутри текущего шага)
        if (progressSlider != null)
            progressSlider.value = stepProgress;
    }

    void MoveToNextStep()
    {
        // Деактивируем текущий объект
        if (loadingSteps[currentStepIndex].objectToActivate != null)
        {
            loadingSteps[currentStepIndex].objectToActivate.SetActive(false);
        }

        // Переходим к следующему шагу
        currentStepIndex++;
        currentStepTime = 0f;

        // Если дошли до конца массива - начинаем новый цикл
        if (currentStepIndex >= loadingSteps.Length)
        {
            currentStepIndex = 0;
        }

        // Активируем новый объект
        if (loadingSteps[currentStepIndex].objectToActivate != null)
        {
            loadingSteps[currentStepIndex].objectToActivate.SetActive(true);
        }
    }

    // Публичный метод для старта загрузки
    public void StartLoading()
    {
        if (isRunning || loadingSteps == null || loadingSteps.Length == 0) return;

        isRunning = true;
        currentStepIndex = 0;
        currentStepTime = 0f;

        // Активируем первый объект
        if (loadingSteps[0].objectToActivate != null)
        {
            loadingSteps[0].objectToActivate.SetActive(true);
        }

        Debug.Log("Циклическая загрузка запущена!");
    }

    public void StopLoading()
    {
        if (!isRunning) return;

        isRunning = false;

        // Деактивируем текущий объект
        if (loadingSteps[currentStepIndex].objectToActivate != null)
        {
            loadingSteps[currentStepIndex].objectToActivate.SetActive(false);
        }

        Debug.Log("Циклическая загрузка остановлена!");
    }

    public void PauseLoading()
    {
        isRunning = false;
    }

    public void ResumeLoading()
    {
        isRunning = true;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale);
    }

    // Для отладки в инспекторе
    [ContextMenu("Запустить загрузку")]
    private void DebugStartLoading()
    {
        StartLoading();
    }

    [ContextMenu("Остановить загрузку")]
    private void DebugStopLoading()
    {
        StopLoading();
    }

    [ContextMenu("Перейти к следующему шагу")]
    private void DebugNextStep()
    {
        if (isRunning)
        {
            MoveToNextStep();
        }
    }

    // Публичные свойства для доступа к состоянию
    public bool IsRunning => isRunning;
    public int CurrentStepIndex => currentStepIndex;
    public float CurrentStepProgress => loadingSteps != null && loadingSteps.Length > 0 ?
        currentStepTime / loadingSteps[currentStepIndex].duration : 0f;
}