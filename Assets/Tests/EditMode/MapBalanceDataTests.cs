#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class MapBalanceDataTests
{
    private static readonly int[] MapNumbers =
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
        40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50
    };

    private struct EnemyBalance
    {
        public EnemyBalance(int hp, int exp)
        {
            Hp = hp;
            Exp = exp;
        }

        public int Hp { get; }
        public int Exp { get; }
    }

    private static readonly int[] OriginalEnemyHp =
    {
        2, 2, 5, 5, 10, 15, 20, 25, 30, 35,
        40, 45, 50, 55, 60, 65, 70, 75, 80, 85,
        90, 100, 110, 121, 133, 146, 161, 177, 195, 214,
        236, 259, 285, 314, 345, 380, 418, 459, 505, 556,
        612, 673, 740, 814, 895, 985, 1083, 1192, 1311, 1442, 1586
    };

    private static readonly int[] OriginalEnemyCount =
    {
        3, 4, 4, 7, 4, 4, 4, 4, 4, 4,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4
    };

    private static readonly int[] OriginalExpReward =
    {
        2, 2, 5, 5, 10, 15, 20, 25, 30, 35,
        40, 45, 50, 55, 60, 65, 70, 75, 80, 85,
        90, 95, 100, 105, 110, 115, 120, 125, 130, 135,
        140, 145, 150, 155, 160, 165, 170, 175, 180, 185,
        190, 195, 200, 205, 210, 215, 220, 225, 230, 235, 240
    };

    private static readonly int[] RequiredExpByLevel =
    {
        1, 7, 13, 20, 27, 33,
        60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180,
        271, 286, 300, 314, 329, 343, 357, 371, 386, 400, 414, 429, 443, 457, 471,
        680, 700, 720, 740, 760, 780, 800, 820, 840, 860, 880, 900, 920, 940, 960
    };

    private static Dictionary<int, EnemyBalance[]> expectedEnemiesByMap;

    [Test]
    public void GeneratedMapsMatchBalanceTable()
    {
        for (int i = 0; i < MapNumbers.Length; ++i)
        {
            int mapNumber = MapNumbers[i];
            EnemyBalance[] expectedEnemies = GetExpectedEnemiesForMap(mapNumber);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Data/Maps/Map{mapNumber}.prefab");
            Assert.That(prefab, Is.Not.Null, $"Map{mapNumber}.prefab is missing.");

            Transform spawnRoot = prefab.transform.Find("EnemySpawnPoints");
            Assert.That(spawnRoot, Is.Not.Null, $"Map{mapNumber} is missing EnemySpawnPoints.");
            Assert.That(spawnRoot.childCount, Is.EqualTo(expectedEnemies.Length), $"Map{mapNumber} spawn point count.");

            WorldEnemySpawner spawner = prefab.GetComponent<WorldEnemySpawner>();
            Assert.That(spawner, Is.Not.Null, $"Map{mapNumber} is missing WorldEnemySpawner.");

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            Assert.That(serializedSpawner.FindProperty("minSpawnCount").intValue, Is.EqualTo(expectedEnemies.Length));
            Assert.That(serializedSpawner.FindProperty("maxSpawnCount").intValue, Is.EqualTo(expectedEnemies.Length));
            Assert.That(serializedSpawner.FindProperty("spawnPoints").arraySize, Is.EqualTo(expectedEnemies.Length));
            Assert.That(serializedSpawner.FindProperty("enemySpawnDataList").arraySize, Is.EqualTo(expectedEnemies.Length));

            for (int spawnIndex = 0; spawnIndex < expectedEnemies.Length; ++spawnIndex)
            {
                EnemySpawnData spawnData = AssetDatabase.LoadAssetAtPath<EnemySpawnData>(
                    $"Assets/Data/Maps/Map{mapNumber}/EnemySpawnData_{spawnIndex + 1}.asset");
                Assert.That(spawnData, Is.Not.Null, $"Map{mapNumber} EnemySpawnData_{spawnIndex + 1} is missing.");
                Assert.That(spawnData.MaxHp, Is.EqualTo(expectedEnemies[spawnIndex].Hp));
                Assert.That(spawnData.ExpReward, Is.EqualTo(expectedEnemies[spawnIndex].Exp));
            }
        }
    }

    [Test]
    public void RequiredExpMatchesBalanceTable()
    {
        GameObject gameObject = new GameObject("UserLevel");
        UserLevel userLevel = gameObject.AddComponent<UserLevel>();

        FieldInfo field = typeof(UserLevel).GetField("requiredExpByLevel", BindingFlags.Instance | BindingFlags.NonPublic);
        int[] requiredExpByLevel = (int[])field.GetValue(userLevel);

        Assert.That(requiredExpByLevel, Is.EqualTo(RequiredExpByLevel));

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void GameSceneWorldUsesGeneratedMapsAndExcludesLegacyMaps()
    {
        string sceneText = File.ReadAllText("Assets/Scenes/GameScene.unity");

        foreach (int mapNumber in MapNumbers)
        {
            string guid = AssetDatabase.AssetPathToGUID($"Assets/Data/Maps/Map{mapNumber}.prefab");
            Assert.That(sceneText, Does.Contain($"guid: {guid}"), $"Map{mapNumber} is not registered in GameScene World.");
        }

        for (int legacyMapNumber = 0; legacyMapNumber <= 2; ++legacyMapNumber)
        {
            string guid = AssetDatabase.AssetPathToGUID($"Assets/Data/Maps/LegacyRegistered/LegacyMap{legacyMapNumber}.prefab");
            Assert.That(sceneText, Does.Not.Contain($"guid: {guid}"), $"LegacyMap{legacyMapNumber} should not be registered in GameScene World.");
        }
    }

    private static EnemyBalance[] GetExpectedEnemiesForMap(int mapNumber)
    {
        if (expectedEnemiesByMap == null)
            expectedEnemiesByMap = BuildExpectedEnemiesByMap();

        return expectedEnemiesByMap[mapNumber];
    }

    private static Dictionary<int, EnemyBalance[]> BuildExpectedEnemiesByMap()
    {
        Dictionary<int, EnemyBalance[]> expectedByMap = new Dictionary<int, EnemyBalance[]>();
        List<EnemyBalance> originalFourEnemySequence = new List<EnemyBalance>();
        List<int> fourEnemyMaps = new List<int>();

        for (int i = 0; i < MapNumbers.Length; ++i)
        {
            int mapNumber = MapNumbers[i];
            int originalCount = OriginalEnemyCount[i];
            EnemyBalance originalBalance = new EnemyBalance(OriginalEnemyHp[i], OriginalExpReward[i]);

            if (originalCount != 4)
            {
                expectedByMap[mapNumber] = Repeat(originalBalance, originalCount);
                continue;
            }

            fourEnemyMaps.Add(mapNumber);
            for (int spawnIndex = 0; spawnIndex < originalCount; ++spawnIndex)
                originalFourEnemySequence.Add(originalBalance);
        }

        EnemyBalance finalEnemyBalance = originalFourEnemySequence[originalFourEnemySequence.Count - 1];
        int sourceIndex = 0;
        foreach (int mapNumber in fourEnemyMaps)
        {
            if (sourceIndex >= originalFourEnemySequence.Count)
            {
                expectedByMap[mapNumber] = new[] { finalEnemyBalance };
                continue;
            }

            int enemyCount = Mathf.Min(8, originalFourEnemySequence.Count - sourceIndex);
            EnemyBalance[] mapEnemies = new EnemyBalance[enemyCount];
            for (int i = 0; i < enemyCount; ++i)
                mapEnemies[i] = originalFourEnemySequence[sourceIndex + i];

            expectedByMap[mapNumber] = mapEnemies;
            sourceIndex += enemyCount;
        }

        return expectedByMap;
    }

    private static EnemyBalance[] Repeat(EnemyBalance balance, int count)
    {
        EnemyBalance[] repeated = new EnemyBalance[count];
        for (int i = 0; i < count; ++i)
            repeated[i] = balance;
        return repeated;
    }
}
#endif
