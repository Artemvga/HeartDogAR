using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StagesCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Slider progressSlider;

    [Header("Stages Configuration")]
    [SerializeField] private StageInfo[] stages;

    [System.Serializable]
    public class StageInfo
    {
        public string animatorStateName;
        public string displayName;
        [HideInInspector] public float animationLength;
    }

    private string currentStateName;
    private float currentStateStartTime;

    private void Start()
    {
        AutoDetectAllAnimationLengths();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string newStateName = GetCurrentStateName(stateInfo);

        // ≈сли состо€ние изменилось
        if (newStateName != currentStateName)
        {
            currentStateName = newStateName;
            currentStateStartTime = Time.time;
        }

        // ќбновл€ем прогресс дл€ текущего состо€ни€
        UpdateCurrentStateProgress(stateInfo);
    }

    private string GetCurrentStateName(AnimatorStateInfo stateInfo)
    {
        foreach (var stage in stages)
        {
            if (stateInfo.IsName(stage.animatorStateName))
            {
                return stage.animatorStateName;
            }
        }
        return "Unknown";
    }

    private void UpdateCurrentStateProgress(AnimatorStateInfo stateInfo)
    {
        foreach (var stage in stages)
        {
            if (stateInfo.IsName(stage.animatorStateName))
            {
                float progress = 0f;

                // »спользуем длину анимации дл€ расчета прогресса
                if (stage.animationLength > 0)
                {
                    float elapsedTime = Time.time - currentStateStartTime;
                    progress = (elapsedTime / stage.animationLength) * 100f;
                    progress = Mathf.Clamp(progress, 0f, 100f);
                }
                else
                {
                    // Fallback: используем normalizedTime если длина не определена
                    progress = (stateInfo.normalizedTime % 1) * 100f;
                }

                UpdateUI(stage.displayName, progress);
                break;
            }
        }
    }

    private void UpdateUI(string stageName, float progress)
    {
        stateText.text = $"{stageName} - {progress:F1}%";
        progressSlider.value = progress;
    }

    private void AutoDetectAllAnimationLengths()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;

        foreach (var stage in stages)
        {
            bool found = false;

            foreach (var clip in animationClips)
            {
                if (clip.name == stage.animatorStateName)
                {
                    stage.animationLength = clip.length;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                stage.animationLength = 0f;
            }
        }
    }

    // ƒл€ отладки в Inspector
    [ContextMenu("Refresh Animation Lengths")]
    private void RefreshLengths()
    {
        AutoDetectAllAnimationLengths();
        Debug.Log("ƒлины анимаций обновлены!");
    }
}