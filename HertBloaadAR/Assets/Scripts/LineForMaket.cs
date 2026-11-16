using UnityEngine;

public class LineForMaket : MonoBehaviour
{
    [Header("Массивы точек")]
    public Transform[] innerPoints;    // Точки внутри родительского объекта
    public Transform[] outerPoints;    // Точки снаружи

    [Header("Настройки линий")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    [Range(0.01f, 0.5f)]
    public float lineWidth = 0.05f;
    public bool useWorldSpace = true;

    private LineRenderer[] lineRenderers;
    private bool isInitialized = false;

    void Start()
    {
        InitializeLines();
    }

    void InitializeLines()
    {
        // Проверяем массивы
        if (innerPoints == null || outerPoints == null)
        {
            Debug.LogError("Массивы точек не назначены!");
            return;
        }

        if (innerPoints.Length != outerPoints.Length)
        {
            Debug.LogError("Количество внутренних и внешних точек должно совпадать!");
            return;
        }

        // Создаем массив LineRenderer'ов
        lineRenderers = new LineRenderer[innerPoints.Length];

        for (int i = 0; i < innerPoints.Length; i++)
        {
            if (innerPoints[i] == null || outerPoints[i] == null)
            {
                Debug.LogWarning($"Пропущена пара точек {i} - отсутствуют трансформы");
                continue;
            }

            CreateLine(i);
        }

        isInitialized = true;
        Debug.Log($"Создано {innerPoints.Length} постоянных линий");
    }

    void CreateLine(int index)
    {
        // Создаем GameObject для линии
        GameObject lineObject = new GameObject($"Line_{index}");
        lineObject.transform.SetParent(this.transform);
        lineObject.transform.localPosition = Vector3.zero;

        // Добавляем и настраиваем LineRenderer
        LineRenderer lr = lineObject.AddComponent<LineRenderer>();
        lineRenderers[index] = lr;

        // Базовая настройка LineRenderer
        lr.positionCount = 2;
        lr.useWorldSpace = useWorldSpace;

        // Устанавливаем позиции
        lr.SetPosition(0, innerPoints[index].position);
        lr.SetPosition(1, outerPoints[index].position);

        // Настраиваем внешний вид
        lr.material = lineMaterial;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // Дополнительные настройки для лучшего вида
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Постоянно обновляем позиции линий
        UpdateLinePositions();
    }

    void UpdateLinePositions()
    {
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (lineRenderers[i] != null &&
                innerPoints[i] != null &&
                outerPoints[i] != null)
            {
                lineRenderers[i].SetPosition(0, innerPoints[i].position);
                lineRenderers[i].SetPosition(1, outerPoints[i].position);
            }
        }
    }

    // Метод для ручного обновления
    public void RefreshLines()
    {
        UpdateLinePositions();
    }

    // Метод для очистки всех линий
    public void ClearAllLines()
    {
        if (lineRenderers == null) return;

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            if (lineRenderers[i] != null && lineRenderers[i].gameObject != null)
            {
                Destroy(lineRenderers[i].gameObject);
            }
        }

        lineRenderers = null;
        isInitialized = false;
    }

    // Метод для пересоздания линий
    public void RecreateLines()
    {
        ClearAllLines();
        InitializeLines();
    }

    void OnDestroy()
    {
        ClearAllLines();
    }
}