#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyGroundingTests
{
    private const float GroundVisualInset = 0.06f;

    [Test]
    public void MoveRangeUsesFootprintInsteadOfFullBodyWidth()
    {
        Tilemap tilemap = CreateTilemapWithRow(0, 2);
        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.4f, 1f);
        enemyObject.transform.position = new Vector3(0.75f, 1f, 0f);

        InvokePrivate(enemy, "MeasureMoveRange", tilemap);

        Assert.That(enemy.MoveRange.min, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(enemy.MoveRange.max, Is.EqualTo(0.5f).Within(0.001f));

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(tilemap.transform.parent.gameObject);
    }

    [Test]
    public void MoveRangeClampsEdgeSpawnAwayFromSurfaceSide()
    {
        Tilemap tilemap = CreateTilemapWithRow(12, 3, 3);
        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.4f, 1f);
        enemyObject.transform.position = new Vector3(1.75f, 7.1f, 0f);

        InvokePrivate(enemy, "MeasureMoveRange", tilemap);

        Assert.That(enemy.MoveRange.min, Is.EqualTo(2.2f).Within(0.001f));
        Assert.That(enemy.MoveRange.max, Is.EqualTo(2.3f).Within(0.001f));
        Assert.That(enemyObject.transform.position.x, Is.EqualTo(2.2f).Within(0.001f));

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(tilemap.transform.parent.gameObject);
    }

    [Test]
    public void SnapToGroundPlacesColliderBottomSlightlyBelowTileTop()
    {
        Tilemap tilemap = CreateTilemapWithRow(0, 3);
        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1.4f);
        enemyObject.transform.position = new Vector3(0.75f, 2f, 0f);

        InvokePrivate(enemy, "SnapToGround", tilemap);

        Assert.That(collider.bounds.min.y, Is.EqualTo(0.5f - GroundVisualInset).Within(0.001f));

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(tilemap.transform.parent.gameObject);
    }

    [Test]
    public void GroundingDoesNotUseFarSidePlatformWhenSpawnIsOffset()
    {
        Tilemap tilemap = CreateTilemapWithRow(2, 2, 3);
        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 1f);
        enemyObject.transform.position = new Vector3(0.6f, 0.1f, 0f);

        InvokePrivate(enemy, "SnapToGround", tilemap);
        InvokePrivate(enemy, "MeasureMoveRange", tilemap);

        Assert.That(collider.bounds.min.y, Is.EqualTo(-0.4f).Within(0.001f));
        Assert.That(enemyObject.transform.position.x, Is.EqualTo(0.6f).Within(0.001f));
        Assert.That(enemy.MoveRange.min, Is.EqualTo(0.6f).Within(0.001f));
        Assert.That(enemy.MoveRange.max, Is.EqualTo(0.6f).Within(0.001f));

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(tilemap.transform.parent.gameObject);
    }

    [Test]
    public void SnapToGroundKeepsNearbyPlatformWhenFootIsSlightlyBelowHalfCell()
    {
        Tilemap tilemap = CreateTilemapWithCells(new Vector3Int(0, -1, 0), new Vector3Int(0, 2, 0));
        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 1f);
        enemyObject.transform.position = new Vector3(0.25f, 1.745f, 0f);

        InvokePrivate(enemy, "SnapToGround", tilemap);

        Assert.That(collider.bounds.min.y, Is.EqualTo(1.5f - GroundVisualInset).Within(0.001f));

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(tilemap.transform.parent.gameObject);
    }

    private static Tilemap CreateTilemapWithRow(int y, int width)
    {
        return CreateTilemapWithRow(y, 0, width);
    }

    private static Tilemap CreateTilemapWithRow(int y, int startX, int width)
    {
        GameObject gridObject = new GameObject("Grid");
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f, 0.5f, 0f);

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        for (int x = startX; x < startX + width; ++x)
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);

        return tilemap;
    }

    private static Tilemap CreateTilemapWithCells(params Vector3Int[] cells)
    {
        GameObject gridObject = new GameObject("Grid");
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f, 0.5f, 0f);

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        Tile tile = ScriptableObject.CreateInstance<Tile>();

        foreach (Vector3Int cell in cells)
            tilemap.SetTile(cell, tile);

        return tilemap;
    }

    private static void InvokePrivate(object target, string methodName, Tilemap tilemap)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"{methodName} is missing.");
        method.Invoke(target, new object[] { tilemap });
    }
}
#endif
