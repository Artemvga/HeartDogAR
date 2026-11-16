using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    [Header("Настройки масштабирования")]
    public float scaleSpeed = 0.01f;
    public float minScale = 1f;
    public float maxScale = 2f;
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

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float currentDistance = Vector2.Distance(currentPosition, screenCenter);
        float initialDistance = Vector2.Distance(initialTouchPosition, screenCenter);

        // Pointer delta already measures movement; multiplying by frame time makes gestures FPS dependent.
        float scaleChange = (currentDistance - initialDistance) * scaleSpeed * 0.1f;
        Vector3 newScale = targetScale * (1f + scaleChange);

        float clampedScale = Mathf.Clamp(newScale.x, initialScale.x * minScale, initialScale.x * maxScale);
        newScale = initialScale * (clampedScale / initialScale.x);

        if (smoothScaling)
        {
            targetScale = newScale;
        }
        else
        {
            targetObject.localScale = newScale;
            targetScale = newScale;
        }

        initialTouchPosition = currentPosition;
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