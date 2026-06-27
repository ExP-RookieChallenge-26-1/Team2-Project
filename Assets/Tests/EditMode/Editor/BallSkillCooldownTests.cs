#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class BallSkillCooldownTests
{
    [Test]
    public void ManualCooldownTicksWhileSkillIsActive()
    {
        using SkillContext context = CreateSkillContext();
        context.Skill.SetTriggerSettings(hasManualTrigger: true, hasAutoTrigger: false);
        context.Skill.SetManualCooldown(10f);
        context.Skill.SetDuration(5f);
        float tickDeltaTime = 0f;

        RunWithDeltaTime(0.25f, () =>
        {
            context.Skill.TryManualActivate();
            tickDeltaTime = Time.deltaTime;
            context.Skill.Tick();
        });

        Assert.That(tickDeltaTime, Is.GreaterThan(0f));
        Assert.That(context.Skill.ActivateCount, Is.EqualTo(1));
        Assert.That(context.Skill.IsActive, Is.True);
        Assert.That(context.Skill.ManualCooldownRemaining, Is.EqualTo(10f - tickDeltaTime).Within(0.001f));
    }

    [Test]
    public void AutoCooldownConsumesActivationFrame()
    {
        using SkillContext context = CreateSkillContext();
        context.Skill.SetTriggerSettings(hasManualTrigger: false, hasAutoTrigger: true);
        context.Skill.SetAutoCooldown(10f);
        context.Skill.SetDuration(5f);
        float tickDeltaTime = 0f;

        RunWithDeltaTime(0.25f, () =>
        {
            tickDeltaTime = Time.deltaTime;
            context.Skill.Tick();
        });

        Assert.That(tickDeltaTime, Is.GreaterThan(0f));
        Assert.That(context.Skill.ActivateCount, Is.EqualTo(1));
        Assert.That(context.Skill.IsActive, Is.True);
        Assert.That(context.Skill.AutoCooldownRemaining, Is.EqualTo(10f - tickDeltaTime).Within(0.001f));
    }

    [Test]
    public void ManualCooldownStartsWhenManualTriggerIsAcquired()
    {
        using SkillContext context = CreateSkillContext();
        context.Skill.SetManualCooldown(10f);

        context.Skill.SetTriggerSettings(hasManualTrigger: true, hasAutoTrigger: false);

        Assert.That(context.Skill.ManualCooldownRemaining, Is.EqualTo(10f).Within(0.001f));
        Assert.That(context.Skill.ManualCooldownRatio, Is.EqualTo(1f).Within(0.001f));
    }

    private static SkillContext CreateSkillContext()
    {
        GameObject skillObject = new("Skill");
        TestBallSkill skill = skillObject.AddComponent<TestBallSkill>();
        return new SkillContext(skillObject, skill);
    }

    private static void RunWithDeltaTime(float deltaTime, System.Action action)
    {
        float previousCaptureDeltaTime = Time.captureDeltaTime;
        Time.captureDeltaTime = deltaTime;

        try
        {
            action();
        }
        finally
        {
            Time.captureDeltaTime = previousCaptureDeltaTime;
        }
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

    private sealed class SkillContext : System.IDisposable
    {
        public SkillContext(GameObject skillObject, TestBallSkill skill)
        {
            SkillObject = skillObject;
            Skill = skill;
        }

        private GameObject SkillObject { get; }
        public TestBallSkill Skill { get; }

        public void Dispose()
        {
            Object.DestroyImmediate(SkillObject);
        }
    }
}
#endif
