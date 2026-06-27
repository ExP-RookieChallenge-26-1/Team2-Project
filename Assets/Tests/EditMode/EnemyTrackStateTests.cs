#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemyTrackStateTests
{
    private const string EnemyPath = "Assets/Gameplay/Enemy/Enemy.cs";
    private const string EnemyAnimatorControllerPath = "Assets/Gameplay/Enemy/EnemyAnimator.controller";
    private const string TrackStatePath = "Assets/Gameplay/Enemy/TrackState.cs";
    private const string AttackStatePath = "Assets/Gameplay/Enemy/AttackState.cs";

    [Test]
    public void SetHeadingFlipsRightBecauseEnemyArtFacesLeftByDefault()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);

        fixture.Enemy.SetHeading(Enemy.Heading.Left);
        Assert.That(fixture.EnemyRenderer.flipX, Is.False);

        fixture.Enemy.SetHeading(Enemy.Heading.Right);
        Assert.That(fixture.EnemyRenderer.flipX, Is.True);
    }

    [Test]
    public void CheckTrackTransitionFacesPaddleWhenBelowTrackThreshold()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);
        SetFloat(fixture.StatsAsset, "<TrackYThreshold>k__BackingField", 1f);
        fixture.Enemy.Initialize(fixture.StatsAsset);
        fixture.Enemy.ChangeState(fixture.Enemy.IdleState);
        fixture.Enemy.SetHeading(Enemy.Heading.Right);

        InvokePrivate(fixture.Enemy, "CheckTrackTransition");

        Assert.That(fixture.EnemyRenderer.flipX, Is.False);
    }

    [Test]
    public void DeadEnemyDoesNotEnterEncounterStateAfterDeath()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);
        SetFloat(fixture.StatsAsset, "<TrackYThreshold>k__BackingField", 1f);
        fixture.Enemy.Initialize(fixture.StatsAsset);
        fixture.Enemy.ChangeState(fixture.Enemy.IdleState);
        fixture.Enemy.SetHeading(Enemy.Heading.Right);
        SetPrivateField(fixture.Enemy, "currentState", null);
        SetPrivateField(fixture.Enemy, "isDead", true);

        InvokePrivate(fixture.Enemy, "CheckTrackTransition");

        Assert.That(GetPrivateField<IEnemyState>(fixture.Enemy, "currentState"), Is.Null);
        Assert.That(fixture.EnemyRenderer.flipX, Is.True);
    }

    [Test]
    public void TrackStateFacesPaddleEvenWhenBlockedByMoveRange()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);
        SetFloat(fixture.StatsAsset, "<TrackSpeed>k__BackingField", 5f);
        fixture.Enemy.Initialize(fixture.StatsAsset);
        SetMoveRange(fixture.Enemy, min: 0f, max: 5f);
        fixture.Enemy.SetHeading(Enemy.Heading.Right);
        fixture.Enemy.ChangeState(fixture.Enemy.TrackState);

        fixture.Enemy.TrackState.Tick(fixture.Enemy);

        Assert.That(fixture.EnemyRenderer.flipX, Is.False);
    }

    [Test]
    public void TrackStateKeepsMoveAnimationEnabledUntilExit()
    {
        string source = ReadProjectFile(TrackStatePath);

        int enterIndex = IndexOfRequired(source, "public override void Enter(Enemy enemy)");
        int enableIndex = IndexOfRequired(source, "enemy.SetMoveAnimation(true);");
        int exitIndex = IndexOfRequired(source, "public override void Exit(Enemy enemy)");
        int disableIndex = IndexOfRequired(source, "enemy.SetMoveAnimation(false);");

        Assert.That(enableIndex, Is.GreaterThan(enterIndex));
        Assert.That(disableIndex, Is.GreaterThan(exitIndex));
        Assert.That(source, Does.Not.Contain("OnIdleAnimation(enemy)"));
    }

    [Test]
    public void AttackStateStopsMoveAnimationBeforePlayingAttackAnimation()
    {
        string source = ReadProjectFile(AttackStatePath);

        int enterIndex = IndexOfRequired(source, "public override void Enter(Enemy enemy)");
        int stopMoveIndex = IndexOfRequired(source, "enemy.SetMoveAnimation(false);");
        int attackIndex = IndexOfRequired(source, "enemy.PlayAttackAnimation();");

        Assert.That(stopMoveIndex, Is.GreaterThan(enterIndex));
        Assert.That(attackIndex, Is.GreaterThan(stopMoveIndex));
    }

    [Test]
    public void AttackStateFacesPaddleWhenAttackStarts()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);
        fixture.Enemy.SetHeading(Enemy.Heading.Left);
        fixture.Enemy.SetHeading(Enemy.Heading.Right);

        fixture.Enemy.ChangeState(fixture.Enemy.AttackState);

        Assert.That(fixture.EnemyRenderer.flipX, Is.False);
    }

    [Test]
    public void AttackStateKeepsFacingPaddleWhileAttackAnimationIsPlaying()
    {
        using TrackingFixture fixture = new TrackingFixture(enemyX: 0f, enemyY: 0f, paddleX: -2f);
        fixture.Enemy.SetHeading(Enemy.Heading.Left);
        fixture.Enemy.ChangeState(fixture.Enemy.AttackState);
        fixture.PaddleObject.transform.position = new Vector3(2f, 0f, 0f);

        fixture.Enemy.AttackState.Tick(fixture.Enemy);

        Assert.That(fixture.EnemyRenderer.flipX, Is.True);
    }

    [Test]
    public void DieAnimationClearsCompetingAnimationParametersBeforeSettingDieTrigger()
    {
        string source = ReadProjectFile(EnemyPath);

        int methodIndex = IndexOfRequired(source, "public void PlayDieAnimation()");
        int stopMoveIndex = IndexOfRequired(source, "this.animator.SetBool(\"IsMoving\", false);");
        int resetEncounterIndex = IndexOfRequired(source, "this.animator.ResetTrigger(\"Encounter\");");
        int resetAttackIndex = IndexOfRequired(source, "this.animator.ResetTrigger(\"Attack\");");
        int resetDamagedIndex = IndexOfRequired(source, "this.animator.ResetTrigger(\"Damaged\");");
        int dieTriggerIndex = IndexOfRequired(source, "this.animator.SetTrigger(\"Die\");");

        Assert.That(stopMoveIndex, Is.GreaterThan(methodIndex));
        Assert.That(resetEncounterIndex, Is.GreaterThan(methodIndex));
        Assert.That(resetAttackIndex, Is.GreaterThan(methodIndex));
        Assert.That(resetDamagedIndex, Is.GreaterThan(methodIndex));
        Assert.That(stopMoveIndex, Is.LessThan(dieTriggerIndex));
        Assert.That(resetEncounterIndex, Is.LessThan(dieTriggerIndex));
        Assert.That(resetAttackIndex, Is.LessThan(dieTriggerIndex));
        Assert.That(resetDamagedIndex, Is.LessThan(dieTriggerIndex));
    }

    [Test]
    public void EnemyAnimatorPrioritizesDieBeforeEncounterWhenBothTriggersAreQueued()
    {
        string controller = ReadProjectFile(EnemyAnimatorControllerPath);
        string dieTransitionId = GetTransitionIdForCondition(controller, "Die");
        string encounterTransitionId = GetTransitionIdForCondition(controller, "Encounter");

        int anyStateIndex = IndexOfRequired(controller, "m_AnyStateTransitions:");
        int dieTransitionIndex = IndexOfRequired(controller, $"- {{fileID: {dieTransitionId}}}");
        int encounterTransitionIndex = IndexOfRequired(controller, $"- {{fileID: {encounterTransitionId}}}");

        Assert.That(dieTransitionIndex, Is.GreaterThan(anyStateIndex));
        Assert.That(encounterTransitionIndex, Is.GreaterThan(anyStateIndex));
        Assert.That(dieTransitionIndex, Is.LessThan(encounterTransitionIndex));
    }

    private sealed class TrackingFixture : IDisposable
    {
        private readonly GameManager previousManager;

        public TrackingFixture(float enemyX, float enemyY, float paddleX)
        {
            this.previousManager = GameManager.Instance;

            this.ManagerObject = new GameObject("TestGameManager");
            this.ManagerObject.SetActive(false);
            this.Manager = this.ManagerObject.AddComponent<GameManager>();

            this.PaddleObject = new GameObject("TestPaddle");
            this.PaddleObject.transform.position = new Vector3(paddleX, 0f, 0f);
            this.Paddle = this.PaddleObject.AddComponent<Paddle>();

            SetPrivateField(this.Manager, "paddle", this.Paddle);
            SetGameManagerInstance(this.Manager);

            this.EnemyObject = new GameObject("TestEnemy");
            this.EnemyObject.transform.position = new Vector3(enemyX, enemyY, 0f);
            this.EnemyRenderer = this.EnemyObject.AddComponent<SpriteRenderer>();
            this.Enemy = this.EnemyObject.AddComponent<Enemy>();

            this.StatsAsset = ScriptableObject.CreateInstance<EnemyStats>();
        }

        public GameObject ManagerObject { get; }
        public GameManager Manager { get; }
        public GameObject PaddleObject { get; }
        public Paddle Paddle { get; }
        public GameObject EnemyObject { get; }
        public SpriteRenderer EnemyRenderer { get; }
        public Enemy Enemy { get; }
        public EnemyStats StatsAsset { get; }

        public void Dispose()
        {
            SetGameManagerInstance(this.previousManager);

            if (this.Enemy != null && this.Enemy.Stats != null)
                UnityEngine.Object.DestroyImmediate(this.Enemy.Stats);

            UnityEngine.Object.DestroyImmediate(this.StatsAsset);
            UnityEngine.Object.DestroyImmediate(this.EnemyObject);
            UnityEngine.Object.DestroyImmediate(this.PaddleObject);
            UnityEngine.Object.DestroyImmediate(this.ManagerObject);
        }
    }

    private static void SetMoveRange(Enemy enemy, float min, float max)
    {
        SetPrivateField(enemy, "<MoveRange>k__BackingField", (min, max));
    }

    private static void SetGameManagerInstance(GameManager manager)
    {
        FieldInfo field = typeof(GameManager).GetField(
            "<Instance>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, "GameManager.Instance backing field is missing.");
        field.SetValue(null, manager);
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

    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
        method.Invoke(target, null);
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static string ReadProjectFile(string projectRelativePath)
    {
        return File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
    }

    private static int IndexOfRequired(string source, string requiredText)
    {
        int index = source.IndexOf(requiredText, StringComparison.Ordinal);
        Assert.That(index, Is.GreaterThanOrEqualTo(0), $"{requiredText} is missing.");
        return index;
    }

    private static string GetTransitionIdForCondition(string controller, string conditionName)
    {
        int conditionIndex = IndexOfRequired(controller, $"m_ConditionEvent: {conditionName}");
        int headerIndex = controller.LastIndexOf("--- !u!1101 &", conditionIndex, StringComparison.Ordinal);
        Assert.That(headerIndex, Is.GreaterThanOrEqualTo(0), $"{conditionName} transition header is missing.");

        int idStart = headerIndex + "--- !u!1101 &".Length;
        int idEnd = controller.IndexOf('\n', idStart);
        Assert.That(idEnd, Is.GreaterThan(idStart), $"{conditionName} transition id is missing.");

        return controller.Substring(idStart, idEnd - idStart).Trim();
    }
}
#endif
