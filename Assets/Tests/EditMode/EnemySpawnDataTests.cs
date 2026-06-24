#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemySpawnDataTests
{
    [Test]
    public void EnemyInitializeCopiesStatsFromSpawnData()
    {
        EnemySpawnData spawnData = ScriptableObject.CreateInstance<EnemySpawnData>();
        SetInt(spawnData, "<MaxHp>k__BackingField", 42);
        SetInt(spawnData, "<ExpReward>k__BackingField", 7);
        SetFloat(spawnData, "<MoveSpeed>k__BackingField", 1.5f);
        SetInt(spawnData, "<AttackDamage>k__BackingField", 9);

        GameObject enemyObject = new GameObject("Enemy");
        Enemy enemy = enemyObject.AddComponent<Enemy>();

        enemy.Initialize(spawnData);

        Assert.That(enemy.Stats, Is.Not.Null);
        Assert.That(enemy.Stats, Is.Not.SameAs(spawnData));
        Assert.That(enemy.Stats.MaxHp, Is.EqualTo(42));
        Assert.That(enemy.Stats.ExpReward, Is.EqualTo(7));
        Assert.That(enemy.Stats.MoveSpeed, Is.EqualTo(1.5f));
        Assert.That(enemy.Stats.AttackDamage, Is.EqualTo(9));

        Object.DestroyImmediate(enemy.Stats);
        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(spawnData);
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
