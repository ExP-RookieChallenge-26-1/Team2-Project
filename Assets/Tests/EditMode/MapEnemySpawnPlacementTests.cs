#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEnemySpawnPlacementTests
{
    private const float MinimumEnemySurfaceWidth = 1.425f;

    [Test]
    public void EnemySpawnPointsUseTopSurfaceTiles()
    {
        foreach (string prefabPath in GetMapPrefabPaths())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, $"{prefabPath} is missing.");

            Tilemap tilemap = prefab.GetComponentInChildren<Tilemap>();
            Assert.That(tilemap, Is.Not.Null, $"{prefabPath} is missing a Tilemap.");

            Transform spawnRoot = prefab.transform.Find("EnemySpawnPoints");
            Assert.That(spawnRoot, Is.Not.Null, $"{prefabPath} is missing EnemySpawnPoints.");

            for (int i = 0; i < spawnRoot.childCount; ++i)
            {
                Transform spawnPoint = spawnRoot.GetChild(i);
                Assert.That(
                    TryFindSpawnGroundCell(tilemap, spawnPoint.position, out Vector3Int groundCell),
                    Is.True,
                    $"{prefabPath}/{spawnPoint.name} has no top surface below it.");

                Assert.That(
                    IsTopSurfaceTile(tilemap, groundCell),
                    Is.True,
                    $"{prefabPath}/{spawnPoint.name} is not on a tile's top-facing surface.");
                Assert.That(
                    GetTopSurfaceRunWidth(tilemap, groundCell),
                    Is.GreaterThanOrEqualTo(MinimumEnemySurfaceWidth),
                    $"{prefabPath}/{spawnPoint.name} is on a top surface too narrow for the enemy body.");
            }
        }
    }

    [Test]
    public void GeneratedEnemySpawnPointsUseEvenTopSurfaceHeightQuantiles()
    {
        for (int mapNumber = 0; mapNumber <= 50; ++mapNumber)
        {
            string prefabPath = $"Assets/Data/Maps/Map{mapNumber}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, $"{prefabPath} is missing.");

            Tilemap tilemap = prefab.GetComponentInChildren<Tilemap>();
            Assert.That(tilemap, Is.Not.Null, $"{prefabPath} is missing a Tilemap.");

            Transform spawnRoot = prefab.transform.Find("EnemySpawnPoints");
            Assert.That(spawnRoot, Is.Not.Null, $"{prefabPath} is missing EnemySpawnPoints.");

            List<int> topRows = GetUniqueSupportedTopSurfaceRows(tilemap);
            List<int> actualRows = new List<int>();
            for (int i = 0; i < spawnRoot.childCount; ++i)
            {
                Transform spawnPoint = spawnRoot.GetChild(i);
                Assert.That(
                    TryFindSpawnGroundCell(tilemap, spawnPoint.position, out Vector3Int groundCell),
                    Is.True,
                    $"{prefabPath}/{spawnPoint.name} has no top surface below it.");
                actualRows.Add(groundCell.y + 1);
            }

            actualRows.Sort();

            for (int i = 0; i < actualRows.Count; ++i)
            {
                int expectedRow = GetEvenlySampledRow(topRows, i, actualRows.Count);
                Assert.That(
                    actualRows[i],
                    Is.EqualTo(expectedRow),
                    $"{prefabPath} spawn height #{i + 1} should use evenly sampled top-surface row {expectedRow}.");
            }
        }
    }

    private static IEnumerable<string> GetMapPrefabPaths()
    {
        yield return "Assets/Data/Maps/LegacyRegistered/LegacyMap0.prefab";
        yield return "Assets/Data/Maps/LegacyRegistered/LegacyMap1.prefab";
        yield return "Assets/Data/Maps/LegacyRegistered/LegacyMap2.prefab";

        for (int i = 0; i <= 50; ++i)
            yield return $"Assets/Data/Maps/Map{i}.prefab";
    }

    private static List<int> GetUniqueSupportedTopSurfaceRows(Tilemap tilemap)
    {
        List<int> rows = new List<int>();

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
        {
            if (!IsTopSurfaceTile(tilemap, cell) || GetTopSurfaceRunWidth(tilemap, cell) < MinimumEnemySurfaceWidth)
                continue;

            int topRow = cell.y + 1;
            if (!rows.Contains(topRow))
                rows.Add(topRow);
        }

        rows.Sort();
        return rows;
    }

    private static int GetEvenlySampledRow(List<int> rows, int spawnIndex, int spawnCount)
    {
        Assert.That(rows, Is.Not.Empty, "Map has no top-surface rows.");

        if (spawnCount <= 1)
            return rows[rows.Count / 2];

        int rowIndex = Mathf.RoundToInt(spawnIndex * (rows.Count - 1f) / (spawnCount - 1f));
        return rows[Mathf.Clamp(rowIndex, 0, rows.Count - 1)];
    }

    private static bool TryFindSpawnGroundCell(Tilemap tilemap, Vector3 position, out Vector3Int groundCell)
    {
        const int horizontalRadius = 1;
        const int verticalRadius = 24;
        const float aboveSpawnToleranceCells = 0.25f;

        Vector3Int startCell = tilemap.WorldToCell(position);
        float highestAllowedTopY = position.y + tilemap.cellSize.y * aboveSpawnToleranceCells;
        float bestScore = float.PositiveInfinity;
        groundCell = default;

        for (int xOffset = -horizontalRadius; xOffset <= horizontalRadius; ++xOffset)
        {
            for (int yOffset = -verticalRadius; yOffset <= verticalRadius; ++yOffset)
            {
                Vector3Int candidate = new Vector3Int(startCell.x + xOffset, startCell.y + yOffset, startCell.z);
                if (!IsTopSurfaceTile(tilemap, candidate))
                    continue;

                Vector3 topCenter = tilemap.GetCellCenterWorld(candidate);
                topCenter.y = tilemap.CellToWorld(candidate + Vector3Int.up).y;
                if (topCenter.y > highestAllowedTopY)
                    continue;

                float score = (topCenter - position).sqrMagnitude;
                if (score >= bestScore)
                    continue;

                bestScore = score;
                groundCell = candidate;
            }
        }

        return bestScore < float.PositiveInfinity;
    }

    private static bool IsTopSurfaceTile(Tilemap tilemap, Vector3Int cell)
    {
        return tilemap.HasTile(cell) && !tilemap.HasTile(cell + Vector3Int.up);
    }

    private static float GetTopSurfaceRunWidth(Tilemap tilemap, Vector3Int cell)
    {
        Vector3Int left = cell;
        while (IsTopSurfaceTile(tilemap, left + Vector3Int.left))
            left += Vector3Int.left;

        Vector3Int right = cell;
        while (IsTopSurfaceTile(tilemap, right + Vector3Int.right))
            right += Vector3Int.right;

        return (right.x - left.x + 1) * tilemap.cellSize.x;
    }
}
#endif
