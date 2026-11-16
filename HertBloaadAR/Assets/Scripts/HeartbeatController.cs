using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartbeatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator dogHeartAnimator;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI bpmText;
    [SerializeField] private TextMeshProUGUI minSpeedText;
    [SerializeField] private TextMeshProUGUI maxSpeedText;

    [Header("Settings")]
    [SerializeField] private float minSpeedMultiplier = 0.5f;
    [SerializeField] private float maxSpeedMultiplier = 2f;
    [SerializeField] private float baseBPM = 60f;

    private void Start()
    {
        if (speedSlider == null)
        {
            Debug.LogError("HeartbeatController: назначьте ползунок скорости.", this);
            enabled = false;
            return;
        }

        // Настройка слайдера
        speedSlider.minValue = minSpeedMultiplier;
        speedSlider.maxValue = maxSpeedMultiplier;
        speedSlider.value = 1f;

        // Настройка текстовых меток
        if (minSpeedText != null)
            minSpeedText.text = $"{minSpeedMultiplier:0.##}×";
        if (maxSpeedText != null)
            maxSpeedText.text = $"{maxSpeedMultiplier:0.##}×";

        // Подписываемся на событие изменения значения слайдера
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);

        // Инициализируем начальные значения
        UpdateAnimationSpeed(1f);
        UpdateUI(1f);
    }

    private void OnSpeedChanged(float newMultiplier)
    {
        UpdateAnimationSpeed(newMultiplier);
        UpdateUI(newMultiplier);
    }

    private void UpdateAnimationSpeed(float multiplier)
    {
        if (dogHeartAnimator != null)
        {
            dogHeartAnimator.speed = multiplier;
        }
    }

    private void UpdateUI(float multiplier)
    {
        // Обновляем текст состояния в зависимости от скорости
        if (stateText != null)
        {
            stateText.text = GetStateByMultiplier(multiplier);
        }

        // Обновляем BPM
        if (bpmText != null)
        {
            float currentBPM = baseBPM * multiplier;
            bpmText.text = $"Ударов в минуту: {currentBPM:F0}";
        }
    }

    private string GetStateByMultiplier(float multiplier)
    {
        // Определяем состояние немецкой овчарки по скорости сердцебиения
        if (multiplier <= 0.6f)
            return "Состояние: Глубокий сон";
        else if (multiplier <= 0.8f)
            return "Состояние: Покой";
        else if (multiplier <= 1.2f)
            return "Состояние: Нормальное";
        else if (multiplier <= 1.5f)
            return "Состояние: Активность";
        else if (multiplier <= 1.8f)
            return "Состояние: Бег";
        else
            return "Состояние: Интенсивная нагрузка";
    }

    public void ResetToNormalSpeed()
    {
        if (speedSlider != null) speedSlider.value = 1f;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, minSpeedMultiplier, maxSpeedMultiplier);
        if (speedSlider != null) speedSlider.value = multiplier;
    }

    private void OnDestroy()
    {
        if (speedSlider != null)
            speedSlider.onValueChanged.RemoveListener(OnSpeedChanged);
    }
}