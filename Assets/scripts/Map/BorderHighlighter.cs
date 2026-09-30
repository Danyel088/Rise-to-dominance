using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BorderHighlighter : MonoBehaviour
{
    [Header("Настройки контура")]
    public Color borderColor = Color.yellow; // Ярко-желтый отлично контрастирует с водой и травой
    public float lineWidth = 0.08f;          // Толщина границы

    private List<LineRenderer> linePool = new List<LineRenderer>();
    private Material defaultMaterial;

    private void Awake()
    {
        // Используем стандартный спрайтовый шейдер для чистых и ярких линий
        defaultMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    /// <summary>
    /// Рисует внешнюю границу вокруг списка доступных клеток
    /// </summary>
    public void DrawBorder(HashSet<Vector3Int> reachableTiles, Tilemap tilemap)
    {
        ClearBorder();

        if (reachableTiles == null || reachableTiles.Count == 0 || tilemap == null) return;

        float halfX = tilemap.cellSize.x / 2f;
        float halfY = tilemap.cellSize.y / 2f;

        foreach (var cell in reachableTiles)
        {
            Vector3 center = tilemap.GetCellCenterWorld(cell);

            // Вычисляем углы клетки (небольшое смещение по Z = -0.1f, чтобы линия была над тайлами)
            Vector3 topLeft = center + new Vector3(-halfX, halfY, -0.1f);
            Vector3 topRight = center + new Vector3(halfX, halfY, -0.1f);
            Vector3 bottomLeft = center + new Vector3(-halfX, -halfY, -0.1f);
            Vector3 bottomRight = center + new Vector3(halfX, -halfY, -0.1f);

            // Если соседа сверху нет в зоне хода — рисуем верхнюю грань
            if (!reachableTiles.Contains(cell + Vector3Int.up))
                DrawSegment(topLeft, topRight);

            // Если соседа снизу нет — рисуем нижнюю грань
            if (!reachableTiles.Contains(cell + Vector3Int.down))
                DrawSegment(bottomLeft, bottomRight);

            // Если соседа слева нет — рисуем левую грань
            if (!reachableTiles.Contains(cell + Vector3Int.left))
                DrawSegment(bottomLeft, topLeft);

            // Если соседа справа нет — рисуем правую грань
            if (!reachableTiles.Contains(cell + Vector3Int.right))
                DrawSegment(bottomRight, topRight);
        }
    }

    /// <summary>
    /// Скрывает границу
    /// </summary>
    public void ClearBorder()
    {
        foreach (var line in linePool)
        {
            line.gameObject.SetActive(false);
        }
    }

    private void DrawSegment(Vector3 start, Vector3 end)
    {
        LineRenderer line = GetOrCreateLine();
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.gameObject.SetActive(true);
    }

    private LineRenderer GetOrCreateLine()
    {
        foreach (var line in linePool)
        {
            if (!line.gameObject.activeSelf)
                return line;
        }

        GameObject obj = new GameObject("BorderSegment");
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.material = defaultMaterial;
        lr.startColor = borderColor;
        lr.endColor = borderColor;
        lr.sortingOrder = 20; // Отображаем поверх всех тайлов

        linePool.Add(lr);
        return lr;
    }
}