#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CowKingBehaviorTests
{
    private const string CowKingPath = "Assets/Gameplay/Cowking/CowKing.cs";
    private const string CowKingBreathPath = "Assets/Gameplay/Cowking/Breath/CowKingBreath.cs";

    [Test]
    public void TakeDamageDoesNotPlayDamagedAnimationWhileAttacking()
    {
        string source = ReadProjectFile(CowKingPath);

        int attackGuardIndex = IndexOfRequired(source, "if (currentState != CowKingState.Attack)");
        int damagedAnimationIndex = IndexOfRequired(source, "PlayDamagedAnimation();");
        int damagedStateIndex = IndexOfRequired(source, "ChangeState(CowKingState.Damaged);");

        Assert.That(damagedAnimationIndex, Is.GreaterThan(attackGuardIndex));
        Assert.That(damagedAnimationIndex, Is.LessThan(damagedStateIndex));
    }

    [Test]
    public void CowKingKeepsAttackAnimationFlagDuringActiveBreath()
    {
        string source = ReadProjectFile(CowKingPath);

        Assert.That(source, Does.Contain("SetBreathAnimation(true);"));
        Assert.That(source, Does.Contain("SetBreathAnimation(false);"));

        int spawnBreathIndex = IndexOfRequired(source, "private void SpawnBreath()");
        int startBreathAnimationIndex = IndexOfRequired(source, "SetBreathAnimation(true);");
        int breathingFlagIndex = IndexOfRequired(source, "isBreathing = true;");

        Assert.That(startBreathAnimationIndex, Is.GreaterThan(spawnBreathIndex));
        Assert.That(startBreathAnimationIndex, Is.LessThan(breathingFlagIndex));

        int endBreathIndex = IndexOfRequired(source, "private void EndBreath()");
        int stopBreathAnimationIndex = IndexOfRequired(source, "SetBreathAnimation(false);", endBreathIndex);

        Assert.That(stopBreathAnimationIndex, Is.GreaterThan(endBreathIndex));
    }

    [Test]
    public void CowKingEvaluatesAttackTransitionBeforeSpawningBreath()
    {
        string source = ReadProjectFile(CowKingPath);

        int spawnBreathIndex = IndexOfRequired(source, "private void SpawnBreath()");
        int startBreathAnimationIndex = IndexOfRequired(source, "SetBreathAnimation(true);", spawnBreathIndex);
        int playAttackIndex = IndexOfRequired(source, "PlayAttackAnimation();", spawnBreathIndex);
        int instantiateIndex = IndexOfRequired(source, "Instantiate(breathPrefab, breathSpawnPoint.position, Quaternion.identity)", spawnBreathIndex);

        Assert.That(startBreathAnimationIndex, Is.LessThan(playAttackIndex));
        Assert.That(playAttackIndex, Is.LessThan(instantiateIndex));

        int playAttackMethodIndex = IndexOfRequired(source, "private void PlayAttackAnimation()");
        int updateAnimatorIndex = IndexOfRequired(source, "animator.Update(0f);", playAttackMethodIndex);

        Assert.That(updateAnimatorIndex, Is.GreaterThan(playAttackMethodIndex));
        Assert.That(
            source,
            Does.Not.Contain("animator.Play("),
            "CowKing should let the immediate IsBreathing transition evaluate instead of hard-swapping to another sprite rect.");
    }

    [Test]
    public void CowKingBreathPollsPaddleOverlapWhileActive()
    {
        string source = ReadProjectFile(CowKingBreathPath);

        int updateIndex = IndexOfRequired(source, "private void Update()");
        int overlapCheckIndex = IndexOfRequired(source, "IsPaddleOverlappingBreath()", updateIndex);
        int timerIndex = IndexOfRequired(source, "damageTimer += Time.deltaTime;", updateIndex);

        Assert.That(overlapCheckIndex, Is.GreaterThan(updateIndex));
        Assert.That(overlapCheckIndex, Is.LessThan(timerIndex));
        Assert.That(source, Does.Contain("GetPaddle()"));
        Assert.That(source, Does.Contain("Physics2D.SyncTransforms();"));
        Assert.That(source, Does.Contain("breathCollider.Distance(paddleCollider).isOverlapped"));
        Assert.That(source, Does.Contain("breathCollider.bounds.Intersects(paddleCollider.bounds)"));
        Assert.That(source, Does.Contain("IsPaddleBoundsOverlappingBreath(paddleCollider)"));
        Assert.That(source, Does.Contain("IsPaddleTransformInsideBreathColumn(paddle)"));
        Assert.That(source, Does.Contain("IsWorldPointInsideBreathPolygon"));
        Assert.That(source, Does.Contain("ContainsPointInPolygon"));
        Assert.That(source, Does.Contain("FindFirstObjectByType<Paddle>()"));
        Assert.That(source, Does.Contain("FindFirstObjectByType<User>()"));
        Assert.That(source, Does.Contain("FindFirstObjectByType<UserHealth>()"));
        Assert.That(source, Does.Not.Contain("private bool isTouchingPaddle"));
    }

    [Test]
    public void CowKingBreathFollowsSpawnPointWhileActive()
    {
        string cowKingSource = ReadProjectFile(CowKingPath);
        string breathSource = ReadProjectFile(CowKingBreathPath);

        int spawnBreathIndex = IndexOfRequired(cowKingSource, "private void SpawnBreath()");
        int instantiateIndex = IndexOfRequired(cowKingSource, "Instantiate(breathPrefab, breathSpawnPoint.position, Quaternion.identity)", spawnBreathIndex);
        int attachIndex = IndexOfRequired(cowKingSource, "AttachToSpawnPoint(breathSpawnPoint)", instantiateIndex);

        Assert.That(attachIndex, Is.GreaterThan(instantiateIndex));
        Assert.That(breathSource, Does.Contain("public void AttachToSpawnPoint(Transform spawnPoint)"));
        Assert.That(breathSource, Does.Contain("private void LateUpdate()"));
        Assert.That(breathSource, Does.Contain("AlignToSpawnPoint();"));
        Assert.That(breathSource, Does.Contain("transform.position = followTarget.position;"));
        Assert.That(cowKingSource, Does.Not.Contain("UpdateBreathSpawnPoint"));
        Assert.That(cowKingSource, Does.Not.Contain("GetSpriteLocalBreathAnchorX"));
        Assert.That(cowKingSource, Does.Not.Contain("breathAnchorPixelX"));
        Assert.That(cowKingSource, Does.Not.Contain("AttachToSpawnPoint(breathSpawnPoint, bodyRenderer)"));
        Assert.That(breathSource, Does.Not.Contain("horizontalCenterTargetRenderer"));
        Assert.That(breathSource, Does.Not.Contain("AlignHorizontalCenterToTarget"));
        Assert.That(breathSource, Does.Not.Contain("horizontalCenterTargetRenderer.bounds.center"));
        Assert.That(breathSource, Does.Not.Contain("breathRenderer.bounds.center"));
    }

    [Test]
    public void CowKingBreathKeepsAnchorPositionWhileFollowingSpawnPoint()
    {
        GameObject breathObject = new GameObject("CowKingBreath");
        GameObject spawnPoint = new GameObject("BreathSpawnPoint");

        try
        {
            spawnPoint.transform.position = new Vector3(1.25f, -3f, 0f);

            CowKingBreath breath = breathObject.AddComponent<CowKingBreath>();
            MethodInfo attachToSpawnPoint = typeof(CowKingBreath).GetMethod(
                "AttachToSpawnPoint",
                new[] { typeof(Transform) });
            Assert.That(attachToSpawnPoint, Is.Not.Null);

            attachToSpawnPoint.Invoke(breath, new object[] { spawnPoint.transform });
            Assert.That(breathObject.transform.position, Is.EqualTo(spawnPoint.transform.position));

            spawnPoint.transform.position = new Vector3(2.25f, -4f, 0f);
            MethodInfo lateUpdate = typeof(CowKingBreath).GetMethod(
                "LateUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(lateUpdate, Is.Not.Null);
            lateUpdate.Invoke(breath, null);

            Assert.That(breathObject.transform.position, Is.EqualTo(spawnPoint.transform.position));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(spawnPoint);
            UnityEngine.Object.DestroyImmediate(breathObject);
        }
    }

    [Test]
    public void CowKingHealthBarSitsBelowSpriteFeet()
    {
        GameObject boss = new GameObject("CowKing");
        Texture2D texture = null;
        Sprite sprite = null;

        try
        {
            SpriteRenderer renderer = boss.AddComponent<SpriteRenderer>();
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f, 1f);
            renderer.sprite = sprite;

            boss.AddComponent<CowKing>();
            MonsterHealthBar healthBar = boss.GetComponent<MonsterHealthBar>();
            Assert.That(healthBar, Is.Not.Null);

            healthBar.SetHealth(10, 10);
            GameObject barRoot = GameObject.Find("CowKing_HealthBar");

            Assert.That(barRoot, Is.Not.Null);
            SpriteRenderer bodyRenderer = barRoot.transform.Find("Body").GetComponent<SpriteRenderer>();
            Assert.That(bodyRenderer.bounds.max.y, Is.LessThanOrEqualTo(renderer.bounds.min.y - 0.04f));
        }
        finally
        {
            GameObject barRoot = GameObject.Find("CowKing_HealthBar");
            if (barRoot != null)
                UnityEngine.Object.DestroyImmediate(barRoot);

            if (sprite != null)
                UnityEngine.Object.DestroyImmediate(sprite);
            if (texture != null)
                UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(boss);
        }
    }

    private static string ReadProjectFile(string projectRelativePath)
    {
        return File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
    }

    private static int IndexOfRequired(string source, string requiredText)
    {
        return IndexOfRequired(source, requiredText, 0);
    }

    private static int IndexOfRequired(string source, string requiredText, int startIndex)
    {
        int index = source.IndexOf(requiredText, startIndex, StringComparison.Ordinal);
        Assert.That(index, Is.GreaterThanOrEqualTo(0), $"{requiredText} is missing.");
        return index;
    }
}
#endif
