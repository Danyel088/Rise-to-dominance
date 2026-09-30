using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    [Header("Faction Settings")]
    public int faction = 1;

    [Header("Action Points")]
    public int maxAP = 3;
    public int currentAP;

    [Header("Grid References")]
    public Tilemap groundTilemap;

    private void Start()
    {
        currentAP = maxAP;
        if (groundTilemap == null) groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();

        // Выравнивание по центру клетки
        float snappedX = Mathf.Floor(transform.position.x) + 0.5f;
        float snappedY = Mathf.Floor(transform.position.y) + 0.5f;
        transform.position = new Vector3(snappedX, snappedY, 0);
    }

    private void OnEnable() => TurnManager.OnTurnChanged += HandleTurnChanged;
    private void OnDisable() => TurnManager.OnTurnChanged -= HandleTurnChanged;

    private void HandleTurnChanged(int newTurn, int activeFaction)
    {
        if (activeFaction == faction) currentAP = maxAP;
    }

    // Перемещение в целевую точку со списанием стоимости AP
    public void MoveTo(Vector3 targetPosition, int apCost)
    {
        if (currentAP < apCost) return;

        transform.position = targetPosition;
        currentAP -= apCost;
        Debug.Log($"Юнит перемещен! Потрачено AP: {apCost}. Осталось AP: {currentAP}");
    }
}