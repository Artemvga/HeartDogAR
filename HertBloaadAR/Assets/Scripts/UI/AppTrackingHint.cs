using TMPro;
using UnityEngine;
using Vuforia;

public class AppTrackingHint : MonoBehaviour
{
    ObserverBehaviour observer;
    TMP_Text label;
    public void Bind(ObserverBehaviour target, TMP_Text text)
    {
        observer = target; label = text;
        if (observer != null)
        {
            observer.OnTargetStatusChanged += OnStatus;
            OnStatus(observer, observer.TargetStatus);
        }
    }
    void OnStatus(ObserverBehaviour target, TargetStatus status)
    {
        bool tracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
        label.text = tracked ? "Модель найдена · можно исследовать" : "Наведите камеру на учебный маркер";
        label.color = tracked ? AppPresentation.Accent : AppPresentation.Ink;
    }
    void OnDestroy()
    {
        if (observer != null) observer.OnTargetStatusChanged -= OnStatus;
    }
}
