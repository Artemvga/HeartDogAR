using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AppSafeArea : MonoBehaviour
{
    Rect lastArea;
    Vector2Int lastScreen;
    void OnEnable() => Apply();
    void Update()
    {
        if (lastArea != Screen.safeArea || lastScreen != new Vector2Int(Screen.width, Screen.height)) Apply();
    }
    void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        var scaler=GetComponentInParent<CanvasScaler>();
        if(scaler!=null)scaler.referenceResolution=Screen.width>Screen.height?new Vector2(1920,1080):new Vector2(1080,1920);
        lastArea = Screen.safeArea; lastScreen = new Vector2Int(Screen.width, Screen.height);
        var rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(lastArea.xMin / Screen.width, lastArea.yMin / Screen.height);
        rect.anchorMax = new Vector2(lastArea.xMax / Screen.width, lastArea.yMax / Screen.height);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
