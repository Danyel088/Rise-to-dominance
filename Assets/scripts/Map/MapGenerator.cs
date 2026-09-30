using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Ссылки на Tilemap")]
    public Tilemap groundTilemap;

    [Header("Тайлы биомов")]
    public TileBase waterTile;  // Тайл воды
    public TileBase grassTile;  // Тайл травы
    public TileBase forestTile; // Тайл леса / деревьев
    public TileBase stoneTile;  // Тайл камня / гор

    [Header("Размер под экран камеры")]
    public int width = 18;
    public int height = 10;

    [Header("Настройки генерации")]
    public float scale = 0.2f;
    public bool useRandomSeed = true;
    public int seed = 0;

    [Header("Пороги высоты биомов")]
    [Range(0f, 1f)] public float waterThreshold = 0.30f;  // Все, что ниже — вода
    [Range(0f, 1f)] public float grassThreshold = 0.55f;  // Ниже этого порога — трава
    [Range(0f, 1f)] public float forestThreshold = 0.72f; // Ниже этого — лес, а выше — камень

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("MapGenerator: Назначьте Ground Tilemap в Инспекторе!");
            return;
        }

        groundTilemap.ClearAllTiles();

        if (useRandomSeed)
        {
            seed = Random.Range(0, 100000);
        }

        int offsetX = width / 2;
        int offsetY = height / 2;
        Vector2 center = new Vector2(offsetX, offsetY);
        float maxDistance = Mathf.Max(width, height) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x * scale + seed;
                float yCoord = (float)y * scale + seed;
                float noise = Mathf.PerlinNoise(xCoord, yCoord);

                float distFromCenter = Vector2.Distance(new Vector2(x, y), center);
                float islandGradient = Mathf.Clamp01(1f - (distFromCenter / maxDistance));

                // Итоговая "высота" ячейки карты
                float heightValue = (noise * 0.65f) + (islandGradient * 0.45f);

                Vector3Int tilePosition = new Vector3Int(x - offsetX, y - offsetY, 0);

                // Выбираем тайл в зависимости от высоты
                if (heightValue < waterThreshold)
                {
                    if (waterTile != null) groundTilemap.SetTile(tilePosition, waterTile);
                }
                else if (heightValue < grassThreshold)
                {
                    if (grassTile != null) groundTilemap.SetTile(tilePosition, grassTile);
                }
                else if (heightValue < forestThreshold)
                {
                    if (forestTile != null) groundTilemap.SetTile(tilePosition, forestTile);
                }
                else
                {
                    if (stoneTile != null) groundTilemap.SetTile(tilePosition, stoneTile);
                }
            }
        }

        SnapUnitToLandCenter();
    }

    private void SnapUnitToLandCenter()
    {
        Unit unit = Object.FindFirstObjectByType<Unit>();
        if (unit == null) return;

        Vector3Int closestCell = Vector3Int.zero;
        float minDistance = float.MaxValue;
        bool foundLand = false;

        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
        {
            TileBase currentTile = groundTilemap.GetTile(pos);

            // Ищем сушу (любой тайл, кроме воды)
            if (currentTile != null && currentTile != waterTile)
            {
                float dist = Vector3.Distance(pos, Vector3.zero);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestCell = pos;
                    foundLand = true;
                }
            }
        }

        if (foundLand)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(closestCell);
            unit.transform.position = new Vector3(worldPos.x, worldPos.y, -1f);
        }
    }
}