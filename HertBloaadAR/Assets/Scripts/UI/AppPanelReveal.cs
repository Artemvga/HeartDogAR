using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AppPanelReveal : MonoBehaviour
{
    CanvasGroup group;
    float elapsed;
    void OnEnable()
    {
        group = GetComponent<CanvasGroup>();
        elapsed = 0; group.alpha = 0;
    }
    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        group.alpha = Mathf.SmoothStep(0, 1, elapsed / .16f);
    }
}
