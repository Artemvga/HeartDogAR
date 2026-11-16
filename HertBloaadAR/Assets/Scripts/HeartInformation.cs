using UnityEngine;

public class HeartInformation : MonoBehaviour
{
    [Header("Назначьте AR-камеру Vuforia")]
    public Camera arCamera;

    void LateUpdate()
    {
        // Проверяем, что камера назначена
        if (arCamera == null)
            return;

        // Получаем направление от объекта к камере
        Vector3 direction = arCamera.transform.position - transform.position;

        // Игнорируем разницу по высоте (оси Y), оставляя только горизонтальное направление
        direction.y = 0;

        // Если направление не нулевое, применяем поворот только по оси Y
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}