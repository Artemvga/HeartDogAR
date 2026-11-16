using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    [Header("Настройки масштабирования")]
    public float scaleSpeed = 0.01f;
    public float minScale = 0.5f;
    public float maxScale = 4f;
    public bool smoothScaling = true;
    public float smoothFactor = 5f;

    [Header("Настройки сброса")]
    public float resetDuration = 0.5f;

    private Transform targetObject;
    private Vector3 initialScale;
    private Vector3 targetScale;
    private Vector2 initialTouchPosition;
    private bool isScaling = false;
    private bool isActive = false;
    private readonly System.Collections.Generic.Dictionary<Transform, Vector3> initialScales = new System.Collections.Generic.Dictionary<Transform, Vector3>();

    public void SetTargetObject(Transform newTarget)
    {
        targetObject = newTarget;
    }

    public void Initialize()
    {
        if (targetObject != null)
        {
            if (!initialScales.TryGetValue(targetObject, out initialScale))
            {
                initialScale = targetObject.localScale;
                initialScales[targetObject] = initialScale;
            }
            targetScale = targetObject.localScale;
        }
    }

    void Update()
    {
        if (!isActive || targetObject == null) return;

        if (smoothScaling)
        {
            targetObject.localScale = Vector3.Lerp(targetObject.localScale, targetScale, smoothFactor * Time.deltaTime);
        }
    }

    public void StartScaling(Vector2 touchPosition)
    {
        if (!isActive || targetObject == null) return;
        isScaling = true;
        initialTouchPosition = touchPosition;
    }

    public void ProcessScaling(Vector2 currentPosition)
    {
        if (!isActive || !isScaling || targetObject == null) return;

        // Upward drag zooms in consistently, regardless of where the gesture starts.
        float delta = (currentPosition.y - initialTouchPosition.y) / Mathf.Max(1f, Screen.height);
        ZoomByFactor(Mathf.Exp(delta * 3f));

        initialTouchPosition = currentPosition;
    }

    // Pinch and wheel are available in both control modes.
    public void ZoomByFactor(float factor)
    {
        if (targetObject == null || initialScale.sqrMagnitude < 1e-10f ||
            factor <= 0 || float.IsNaN(factor) || float.IsInfinity(factor)) return;
        float current = Vector3.Dot(targetScale, initialScale) / initialScale.sqrMagnitude;
        float multiplier = Mathf.Clamp(current * factor, minScale, maxScale);
        targetScale = initialScale * multiplier;
        // Apply immediately: zoom must also work while rotation mode is active.
        targetObject.localScale = targetScale;
    }

    public void EndScaling()
    {
        isScaling = false;
    }

    public void ResetScale()
    {
        if (targetObject != null)
        {
            targetObject.localScale = initialScale;
            targetScale = initialScale;
        }
    }

    public IEnumerator SmoothResetScale()
    {
        if (targetObject == null) yield break;

        float timer = 0f;
        Vector3 startScale = targetObject.localScale;

        while (timer < resetDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / resetDuration;
            targetObject.localScale = Vector3.Lerp(startScale, initialScale, progress);
            targetScale = initialScale;
            yield return null;
        }

        targetObject.localScale = initialScale;
        targetScale = initialScale;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active) EndScaling();
    }

    public float GetCurrentScalePercent()
    {
        if (targetObject == null) return 0f;
        return (targetScale.x / initialScale.x - minScale) / (maxScale - minScale) * 100f;
    }

    public Vector3 GetInitialScale() => initialScale;
    public Vector3 GetCurrentScale() => targetScale;

    public void ForceUpdateScale()
    {
        if (targetObject != null)
        {
            targetScale = targetObject.localScale;
        }
    }
}