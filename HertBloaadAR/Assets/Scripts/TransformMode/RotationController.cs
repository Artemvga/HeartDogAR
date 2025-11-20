using UnityEngine;
using System.Collections;

public class RotationController : MonoBehaviour
{
    [Header("Настройки вращения")]
    public float rotationSpeed = 2f;
    public bool invertRotation = false;
    public bool smoothRotation = true;
    public float smoothFactor = 8f;

    [Header("Настройки сброса")]
    public float resetDuration = 0.5f;

    [Header("Цвета кнопок")]
    public Color normalInvertColor = Color.white;
    public Color activeInvertColor = Color.green;

    private Transform targetObject;
    private Vector2 previousTouchPosition;
    private bool isRotating = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;
    private bool isActive = true;

    public void SetTargetObject(Transform newTarget)
    {
        targetObject = newTarget;
    }

    public void Initialize()
    {
        if (targetObject != null)
        {
            initialRotation = targetObject.rotation;
            targetRotation = initialRotation;
        }
    }

    void Update()
    {
        if (!isActive || targetObject == null) return;

        if (smoothRotation)
        {
            targetObject.rotation = Quaternion.Lerp(targetObject.rotation, targetRotation, smoothFactor * Time.deltaTime);
        }
    }

    public void StartRotation(Vector2 touchPosition)
    {
        if (!isActive || targetObject == null) return;
        isRotating = true;
        previousTouchPosition = touchPosition;
    }

    public void ProcessRotation(Vector2 currentPosition)
    {
        if (!isActive || !isRotating || targetObject == null) return;

        Vector2 delta = currentPosition - previousTouchPosition;
        float sensitivity = rotationSpeed * 0.1f;

        float direction = invertRotation ? -1f : 1f;

        // Свободное вращение по всем осям
        float rotationX = -delta.y * sensitivity * direction; // Вверх/вниз
        float rotationY = delta.x * sensitivity * direction;  // Влево/вправо

        Vector3 newRotation = new Vector3(rotationX, rotationY, 0);

        if (smoothRotation)
        {
            targetRotation *= Quaternion.Euler(newRotation * Time.deltaTime * 60f);
        }
        else
        {
            targetObject.Rotate(newRotation * Time.deltaTime * 60f, Space.Self);
        }

        previousTouchPosition = currentPosition;
    }

    public void EndRotation()
    {
        isRotating = false;
    }

    public void ResetRotation()
    {
        if (targetObject != null)
        {
            targetObject.rotation = initialRotation;
            targetRotation = initialRotation;
        }
    }

    public IEnumerator SmoothResetRotation()
    {
        if (targetObject == null) yield break;

        float timer = 0f;
        Quaternion startRotation = targetObject.rotation;

        while (timer < resetDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / resetDuration;
            targetObject.rotation = Quaternion.Lerp(startRotation, initialRotation, progress);
            targetRotation = initialRotation;
            yield return null;
        }

        targetObject.rotation = initialRotation;
        targetRotation = initialRotation;
    }

    public void ToggleInversion()
    {
        invertRotation = !invertRotation;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active) EndRotation();
    }

    public bool GetInvertState() => invertRotation;
    public Color GetInvertButtonColor() => invertRotation ? activeInvertColor : normalInvertColor;

    public void ForceUpdateRotation()
    {
        if (targetObject != null)
        {
            targetRotation = targetObject.rotation;
        }
    }
}