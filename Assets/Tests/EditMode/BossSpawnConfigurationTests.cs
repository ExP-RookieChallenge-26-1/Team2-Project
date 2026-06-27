#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BossSpawnConfigurationTests
{
    private const string BossMapPath = "Assets/Data/Maps/Map27.prefab";
    private const string CowKingPrefabPath = "Assets/Gameplay/Cowking/CowKing.prefab";
    private const string CowKingAnimatorControllerPath = "Assets/Gameplay/Cowking/CowKingAnimator.controller";

    [Test]
    public void FirstMapAfterCompressedEnemiesSpawnsCowKingAtConfiguredSpawnPoint()
    {
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossMapPath);
        Assert.That(mapPrefab, Is.Not.Null, $"{BossMapPath} is missing.");

        BossSpawnTrigger spawnTrigger = mapPrefab.GetComponent<BossSpawnTrigger>();
        Assert.That(spawnTrigger, Is.Not.Null, $"{BossMapPath} is missing BossSpawnTrigger.");

        SerializedObject serializedTrigger = new SerializedObject(spawnTrigger);
        GameObject bossPrefab = serializedTrigger.FindProperty("bossPrefab").objectReferenceValue as GameObject;
        Transform bossSpawnPoint = serializedTrigger.FindProperty("bossSpawnPoint").objectReferenceValue as Transform;

        GameObject expectedBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CowKingPrefabPath);
        Transform expectedSpawnPoint = mapPrefab.transform.Find("BossSpawnPoint");

        Assert.That(expectedBossPrefab, Is.Not.Null, $"{CowKingPrefabPath} is missing.");
        Assert.That(expectedSpawnPoint, Is.Not.Null, $"{BossMapPath} is missing BossSpawnPoint.");
        Assert.That(bossPrefab, Is.SameAs(expectedBossPrefab), "BossSpawnTrigger must spawn CowKing.");
        Assert.That(bossSpawnPoint, Is.SameAs(expectedSpawnPoint), "BossSpawnTrigger must reference BossSpawnPoint.");
    }

    [Test]
    public void NonFinalMapPrefabsDoNotContainBossSpawn()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Data/Maps" }))
        {
            string mapPath = AssetDatabase.GUIDToAssetPath(guid);
            if (mapPath == BossMapPath)
                continue;

            GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPath);
            Assert.That(mapPrefab, Is.Not.Null, $"{mapPath} is missing.");

            Assert.That(mapPrefab.GetComponent<BossSpawnTrigger>(), Is.Null, $"{mapPath} should not have BossSpawnTrigger.");
            Assert.That(mapPrefab.transform.Find("BossSpawnPoint"), Is.Null, $"{mapPath} should not have BossSpawnPoint.");
        }
    }

    [Test]
    public void BossSpawnTriggerPreSpawnsCowKingAsChunkChildBeforeActivation()
    {
        using BossSpawnFixture fixture = new BossSpawnFixture();

        InvokePrivate(fixture.Trigger, "Start");

        GameObject bossObject = GetPrivateField<GameObject>(fixture.Trigger, "spawnedBossObject");
        Assert.That(bossObject, Is.Not.Null, "Boss should be prepared before its activation zone is reached.");
        Assert.That(bossObject.transform.parent, Is.SameAs(fixture.TriggerObject.transform));
        Assert.That(bossObject.transform.position, Is.EqualTo(fixture.SpawnPoint.position));
        Assert.That(bossObject.GetComponent<Collider2D>().enabled, Is.False);
        Assert.That(bossObject.GetComponent<Animator>().enabled, Is.True);
    }

    [Test]
    public void BossSpawnTriggerPlaysMoveAnimationBeforeActivation()
    {
        using BossSpawnFixture fixture = new BossSpawnFixture();
        fixture.BossPrefabAnimator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CowKingAnimatorControllerPath);
        Assert.That(fixture.BossPrefabAnimator.runtimeAnimatorController, Is.Not.Null);

        InvokePrivate(fixture.Trigger, "Start");

        GameObject bossObject = GetPrivateField<GameObject>(fixture.Trigger, "spawnedBossObject");
        Animator bossAnimator = bossObject.GetComponent<Animator>();

        Assert.That(bossAnimator.enabled, Is.True);
        Assert.That(bossAnimator.GetBool("IsMoving"), Is.True);
        Assert.That(bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("CowKing_Move"), Is.True);
    }

    [Test]
    public void BossSpawnTriggerDetachesCowKingFromChunkWhenActivated()
    {
        using BossSpawnFixture fixture = new BossSpawnFixture();
        SetPrivateField(fixture.Trigger, "activateDelay", 0f);

        InvokePrivate(fixture.Trigger, "Start");
        GameObject bossObject = GetPrivateField<GameObject>(fixture.Trigger, "spawnedBossObject");

        InvokePrivate(fixture.Trigger, "TrySpawnBoss");

        Assert.That(bossObject.transform.parent, Is.Null);
    }

    [Test]
    public void BossSpawnTriggerActivatesAfterBossDropsLowerIntoViewport()
    {
        using BossSpawnFixture fixture = new BossSpawnFixture();
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();

        try
        {
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            fixture.SpawnPoint.position = ViewportToWorld(camera, 0.9f);
            Assert.That(
                InvokePrivate<bool>(fixture.Trigger, "IsBossSpawnPointInActivationZone"),
                Is.False,
                "Boss should keep approaching before reaching the lower activation line.");

            fixture.SpawnPoint.position = ViewportToWorld(camera, 0.75f);
            Assert.That(
                InvokePrivate<bool>(fixture.Trigger, "IsBossSpawnPointInActivationZone"),
                Is.True,
                "Boss should activate once it reaches the lower activation line.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    [Test]
    public void BossSpawnTriggerResetSessionStateClearsPreviousBossSpawn()
    {
        SetStaticPrivateField(typeof(BossSpawnTrigger), "hasSpawnedBossOnce", true);

        try
        {
            MethodInfo resetMethod = typeof(BossSpawnTrigger).GetMethod(
                "ResetSessionState",
                BindingFlags.Static | BindingFlags.Public);

            Assert.That(
                resetMethod,
                Is.Not.Null,
                "BossSpawnTrigger.ResetSessionState should clear static boss spawn state before a new game starts.");

            resetMethod.Invoke(null, null);

            Assert.That(
                GetStaticPrivateField<bool>(typeof(BossSpawnTrigger), "hasSpawnedBossOnce"),
                Is.False);
        }
        finally
        {
            SetStaticPrivateField(typeof(BossSpawnTrigger), "hasSpawnedBossOnce", false);
        }
    }

    private sealed class BossSpawnFixture : IDisposable
    {
        public BossSpawnFixture()
        {
            SetStaticPrivateField(typeof(BossSpawnTrigger), "hasSpawnedBossOnce", false);

            this.TriggerObject = new GameObject("BossChunk");
            this.Trigger = this.TriggerObject.AddComponent<BossSpawnTrigger>();

            GameObject spawnObject = new GameObject("BossSpawnPoint");
            spawnObject.transform.SetParent(this.TriggerObject.transform, false);
            spawnObject.transform.position = new Vector3(0.5f, 8f, 0f);
            this.SpawnPoint = spawnObject.transform;

            this.BossPrefab = new GameObject("CowKingPrefab");
            this.BossPrefab.AddComponent<SpriteRenderer>();
            this.BossPrefabAnimator = this.BossPrefab.AddComponent<Animator>();
            this.BossPrefab.AddComponent<BoxCollider2D>();
            this.BossPrefab.AddComponent<CowKing>();

            SetPrivateField(this.Trigger, "bossPrefab", this.BossPrefab);
            SetPrivateField(this.Trigger, "bossSpawnPoint", this.SpawnPoint);
        }

        public GameObject TriggerObject { get; }
        public BossSpawnTrigger Trigger { get; }
        public Transform SpawnPoint { get; }
        public GameObject BossPrefab { get; }
        public Animator BossPrefabAnimator { get; }

        public void Dispose()
        {
            SetStaticPrivateField(typeof(BossSpawnTrigger), "hasSpawnedBossOnce", false);

            GameObject spawnedBoss = GetPrivateField<GameObject>(this.Trigger, "spawnedBossObject");
            if (spawnedBoss != null)
                UnityEngine.Object.DestroyImmediate(spawnedBoss);

            UnityEngine.Object.DestroyImmediate(this.TriggerObject);
            UnityEngine.Object.DestroyImmediate(this.BossPrefab);
        }
    }

    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
        method.Invoke(target, null);
    }

    private static T InvokePrivate<T>(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
        return (T)method.Invoke(target, null);
    }

    private static Vector3 ViewportToWorld(Camera camera, float viewportY)
    {
        Vector3 worldPosition = camera.ViewportToWorldPoint(new Vector3(0.5f, viewportY, 10f));
        worldPosition.z = 0f;
        return worldPosition;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        field.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        return (T)field.GetValue(target);
    }

    private static void SetStaticPrivateField(Type targetType, string fieldName, object value)
    {
        FieldInfo field = targetType.GetField(
            fieldName,
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"{targetType.Name}.{fieldName} is missing.");
        field.SetValue(null, value);
    }

    private static T GetStaticPrivateField<T>(Type targetType, string fieldName)
    {
        FieldInfo field = targetType.GetField(
            fieldName,
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"{targetType.Name}.{fieldName} is missing.");
        return (T)field.GetValue(null);
    }
}
#endif
