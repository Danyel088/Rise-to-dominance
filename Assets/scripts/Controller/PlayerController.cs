using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Ссылки")]
    public Tilemap groundTilemap;
    public BorderHighlighter borderHighlighter; // Ссылка на наш скрипт подсвечивания контура

    [Header("Выделение")]
    public Unit selectedUnit;

    // Словарь подсвеченных клеток: Клетка -> Стоимость в AP
    private Dictionary<Vector3Int, int> reachableCells = new Dictionary<Vector3Int, int>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // 1. Проверяем клик по юниту
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<Unit>() != null)
            {
                Unit clickedUnit = hit.collider.GetComponent<Unit>();
                SelectUnit(clickedUnit);
                return;
            }

            // 2. Если юнит уже выбран и кликаем по клетке для хода
            if (selectedUnit != null && selectedUnit.faction == TurnManager.Instance.CurrentFaction)
            {
                Vector3Int clickedCell = groundTilemap.WorldToCell(mouseWorldPos);

                // Если кликнули в подсвеченную (доступную) клетку
                if (reachableCells.ContainsKey(clickedCell))
                {
                    int cost = reachableCells[clickedCell];
                    Vector3 targetPos = groundTilemap.GetCellCenterWorld(clickedCell);
                    targetPos.z = -1f; // Дикарь поверх тайлов

                    // Перемещаем юнита
                    selectedUnit.MoveTo(targetPos, cost);

                    // Пересчитываем доступную зону для оставшихся AP
                    CalculateReachableCells();
                }
                else
                {
                    // Кликнули в недоступную клетку — снимаем выделение
                    DeselectUnit();
                }
            }
        }
    }

    public void SelectUnit(Unit unit)
    {
        DeselectUnit();
        selectedUnit = unit;
        Debug.Log($"Выбран юнит фракции {selectedUnit.faction}");

        if (selectedUnit.faction == TurnManager.Instance.CurrentFaction)
        {
            CalculateReachableCells();
        }
    }

    public void DeselectUnit()
    {
        ClearHighlights();
        selectedUnit = null;
        reachableCells.Clear();
    }

    // Алгоритм поиска доступных клеток (BFS)
    void CalculateReachableCells()
    {
        ClearHighlights();
        reachableCells.Clear();

        if (selectedUnit == null || selectedUnit.currentAP <= 0) return;

        Vector3Int startCell = groundTilemap.WorldToCell(selectedUnit.transform.position);
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        queue.Enqueue(startCell);
        reachableCells[startCell] = 0;

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentDistance = reachableCells[current];

            if (currentDistance >= selectedUnit.currentAP) continue;

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;

                // Проверяем, есть ли на этой клетке суша
                if (!groundTilemap.HasTile(neighbor)) continue;

                int newDistance = currentDistance + 1;

                if (!reachableCells.ContainsKey(neighbor) || newDistance < reachableCells[neighbor])
                {
                    reachableCells[neighbor] = newDistance;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // РИСУЕМ ВНЕШНЮЮ РАМКУ ВОКРУГ ВСЕЙ ДОСТУПНОЙ ЗОНЫ
        if (borderHighlighter != null)
        {
            HashSet<Vector3Int> tilesSet = new HashSet<Vector3Int>(reachableCells.Keys);
            borderHighlighter.DrawBorder(tilesSet, groundTilemap);
        }
    }

    // Сброс внешней рамки
    void ClearHighlights()
    {
        if (borderHighlighter != null)
        {
            borderHighlighter.ClearBorder();
        }
    }
}