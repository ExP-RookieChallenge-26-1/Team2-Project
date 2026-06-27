#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class BallSkillRespawnTests
{
    private const string BallSkillPath = "Assets/Gameplay/Ball/BallSkill.cs";

    [Test]
    public void ManualSkillDoesNotActivateWhileBallIsRespawning()
    {
        using RespawnSkillContext context = CreateRespawnSkillContext();
        context.Skill.SetTriggerSettings(hasManualTrigger: true, hasAutoTrigger: false);
        context.Skill.SetManualCooldown(10f);

        context.Skill.TryManualActivate();

        Assert.That(context.Skill.ActivateCount, Is.EqualTo(0));
        Assert.That(context.Skill.ManualCooldownRemaining, Is.EqualTo(0f).Within(0.001f));
        Assert.That(context.Skill.IsManualReady, Is.False);
    }

    [Test]
    public void SkillTickReturnsBeforeAnyCooldownTimersWhileBallIsRespawning()
    {
        string source = File.ReadAllText(Path.Combine(Application.dataPath, "../", BallSkillPath));
        string tickBody = ExtractMethodBody(source, "Tick");
        int respawnGuard = tickBody.IndexOf("IsRespawning", System.StringComparison.Ordinal);
        Assert.That(respawnGuard, Is.GreaterThanOrEqualTo(0), "BallSkill.Tick should guard against the ball respawn state.");

        Assert.That(respawnGuard, Is.LessThan(tickBody.IndexOf("TickDuration", System.StringComparison.Ordinal)));
        Assert.That(respawnGuard, Is.LessThan(tickBody.IndexOf("TickManual", System.StringComparison.Ordinal)));
        Assert.That(respawnGuard, Is.LessThan(tickBody.IndexOf("TickAuto", System.StringComparison.Ordinal)));
    }

    private static RespawnSkillContext CreateRespawnSkillContext()
    {
        GameObject ballObject = new("RespawningBall");
        ballObject.SetActive(false);
        Ball ball = ballObject.AddComponent<Ball>();
        SetPrivateField(ball, "isSpawning", true);

        GameObject skillObject = new("Skill");
        TestBallSkill skill = skillObject.AddComponent<TestBallSkill>();
        SetPrivateField(skill, "ball", ball);

        return new RespawnSkillContext(ballObject, skillObject, skill);
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        string signature = $"void {methodName}(";
        int signatureIndex = source.IndexOf(signature, System.StringComparison.Ordinal);
        Assert.That(signatureIndex, Is.GreaterThanOrEqualTo(0), $"{methodName} method was not found.");

        int openBraceIndex = source.IndexOf('{', signatureIndex);
        Assert.That(openBraceIndex, Is.GreaterThanOrEqualTo(0), $"{methodName} method has no body.");

        int depth = 0;
        for (int i = openBraceIndex; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return source.Substring(openBraceIndex + 1, i - openBraceIndex - 1);
            }
        }

        Assert.Fail($"{methodName} method body was not closed.");
        return string.Empty;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = FindPrivateField(target.GetType(), fieldName);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName} on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    private static FieldInfo FindPrivateField(System.Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field != null)
                return field;

            type = type.BaseType;
        }

        return null;
    }

    private sealed class TestBallSkill : BallSkill
    {
        public int ActivateCount { get; private set; }

        protected override void OnActivate()
        {
            this.ActivateCount++;
        }

        protected override void OnDeactivate()
        {
        }
    }

    private sealed class RespawnSkillContext : System.IDisposable
    {
        public RespawnSkillContext(GameObject ballObject, GameObject skillObject, TestBallSkill skill)
        {
            BallObject = ballObject;
            SkillObject = skillObject;
            Skill = skill;
        }

        private GameObject BallObject { get; }
        private GameObject SkillObject { get; }
        public TestBallSkill Skill { get; }

        public void Dispose()
        {
            Object.DestroyImmediate(SkillObject);
            Object.DestroyImmediate(BallObject);
        }
    }
}
#endif
