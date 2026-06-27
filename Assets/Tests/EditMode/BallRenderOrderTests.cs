#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class BallRenderOrderTests
{
    private const int HealthBarSortingSlots = 2;
    private const string BallPrefabPath = "Assets/Gameplay/Ball/Ball.prefab";
    private const string EnemyPrefabPath = "Assets/Gameplay/Enemy/Enemy.prefab";
    private const string CowKingPrefabPath = "Assets/Gameplay/Cowking/CowKing.prefab";
    private const string PaddlePrefabPath = "Assets/Gameplay/Paddle/Paddle.prefab";

    [Test]
    public void GameplaySpritesUseExpectedRenderOrder()
    {
        int ballOrder = ReadFirstSpriteRendererSortingOrder(BallPrefabPath);
        int enemyOrder = ReadFirstSpriteRendererSortingOrder(EnemyPrefabPath);
        int cowKingOrder = ReadFirstSpriteRendererSortingOrder(CowKingPrefabPath);
        int paddleOrder = ReadHighestSpriteRendererSortingOrder(PaddlePrefabPath);

        Assert.That(ballOrder, Is.GreaterThan(cowKingOrder));
        Assert.That(cowKingOrder, Is.GreaterThan(paddleOrder));
        Assert.That(paddleOrder, Is.GreaterThan(enemyOrder));
        Assert.That(ballOrder - cowKingOrder, Is.GreaterThan(HealthBarSortingSlots));
        Assert.That(cowKingOrder - paddleOrder, Is.GreaterThan(HealthBarSortingSlots));
        Assert.That(paddleOrder - enemyOrder, Is.GreaterThan(HealthBarSortingSlots));
    }

    private static int ReadFirstSpriteRendererSortingOrder(string projectRelativePath)
    {
        string prefab = File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
        Match spriteRenderer = Regex.Match(
            prefab,
            @"SpriteRenderer:[\s\S]*?m_SortingOrder: (-?\d+)");

        Assert.That(spriteRenderer.Success, Is.True, $"{projectRelativePath} has no SpriteRenderer sorting order.");
        return int.Parse(spriteRenderer.Groups[1].Value);
    }

    private static int ReadHighestSpriteRendererSortingOrder(string projectRelativePath)
    {
        string prefab = File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
        MatchCollection spriteRenderers = Regex.Matches(
            prefab,
            @"SpriteRenderer:[\s\S]*?m_SortingOrder: (-?\d+)");

        Assert.That(spriteRenderers.Count, Is.GreaterThan(0), $"{projectRelativePath} has no SpriteRenderer sorting order.");

        int highestOrder = int.MinValue;
        foreach (Match spriteRenderer in spriteRenderers)
            highestOrder = Mathf.Max(highestOrder, int.Parse(spriteRenderer.Groups[1].Value));

        return highestOrder;
    }
}
#endif
