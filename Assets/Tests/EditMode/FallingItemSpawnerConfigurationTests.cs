#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class FallingItemSpawnerConfigurationTests
{
    [Test]
    public void DefaultMapDropSettingsCoverBalanceTableAndItemPools()
    {
        GameObject gameObject = new GameObject("FallingItemSpawner");
        FallingItemSpawner spawner = gameObject.AddComponent<FallingItemSpawner>();

        InvokePrivate(spawner, "EnsureDefaultDropSettings");
        Array settings = GetPrivate<Array>(spawner, "mapDropSettings");

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.Length, Is.EqualTo(51));

        AssertMapPool(settings.GetValue(0), new[] { 1005, 3010, 2010 }, new[] { 1f, 1f, 1f });
        AssertMapPool(
            settings.GetValue(1),
            new[] { 1005, 3010, 2010, 4204 },
            new[] { 0.5f, 0.5f, 0.5f, 0.5f });
        AssertMapPool(
            settings.GetValue(2),
            new[] { 1005, 3010, 2010, 4204, 4101 },
            new[] { 1f, 1f, 1f, 0.5f, 0.5f });
        AssertMapPool(
            settings.GetValue(3),
            new[] { 1005, 3015, 2010, 4204, 4101, 4103, 4104, 4203, 4105 },
            new[] { 4.5f, 4.5f, 4.5f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f });
        AssertMapPool(
            settings.GetValue(5),
            new[] { 1005, 3020, 2010, 4204, 4101, 4103, 4104, 4203, 4105, 4201, 4102 },
            new[] { 5.5f, 5.5f, 5.5f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f });
        AssertMapPool(
            settings.GetValue(10),
            new[] { 1005, 3035, 2009, 4204, 4101, 4103, 4104, 4203, 4105, 4201, 4102, 4202 },
            new[] { 6f, 6f, 6f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f });
        AssertMapPool(
            settings.GetValue(21),
            new[] { 1005, 3070, 2006, 4204, 4101, 4103, 4104, 4203, 4105, 4201, 4102, 4202 },
            new[] { 6f, 6f, 6f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f });
        AssertMapPool(
            settings.GetValue(50),
            new[] { 1005, 3150, 2001, 4204, 4101, 4103, 4104, 4203, 4105, 4201, 4102, 4202 },
            new[] { 6f, 6f, 6f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.5f });

        UnityEngine.Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ScheduleMapDropsQueuesOneEnhancementItemPerMap()
    {
        GameObject spawnerObject = new GameObject("FallingItemSpawner");
        GameObject enhancementPrefab = new GameObject("EnhancementTicket");
        GameObject attackPrefab = new GameObject("AttackUp");
        GameObject chunk = new GameObject("MapChunk");
        FallingItemSpawner spawner = spawnerObject.AddComponent<FallingItemSpawner>();

        SetPrivate(spawner, "enhancementTicketPrefab", enhancementPrefab);
        SetPrivate(spawner, "attackUpPrefab", attackPrefab);

        spawner.ScheduleMapDrops(10, chunk.transform);

        IList pendingDrops = GetPrivate<IList>(spawner, "pendingDrops");
        Assert.That(pendingDrops, Has.Count.EqualTo(1));

        UnityEngine.Object.DestroyImmediate(chunk);
        UnityEngine.Object.DestroyImmediate(attackPrefab);
        UnityEngine.Object.DestroyImmediate(enhancementPrefab);
        UnityEngine.Object.DestroyImmediate(spawnerObject);
    }

    private static void AssertMapPool(object setting, int[] expectedIds, float[] expectedWeights)
    {
        object pool = GetPrivate<object>(setting, "enhancementCardPool");
        int[] ids = GetPrivate<int[]>(pool, "cardIds");
        float[] weights = GetPrivate<float[]>(pool, "cardWeights");

        Assert.That(ids, Is.EqualTo(expectedIds));
        Assert.That(weights, Is.EqualTo(expectedWeights));
    }

    private static T GetPrivate<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        return (T)field.GetValue(target);
    }

    private static void SetPrivate(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        field.SetValue(target, value);
    }

    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
        method.Invoke(target, null);
    }
}
#endif
