#if UNITY_EDITOR
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BallAnimationDirectionTests
{
    private const string BallPath = "Assets/Gameplay/Ball/Ball.cs";
    private const string BallAnimationPath = "Assets/Gameplay/Ball/BallAnimation.cs";
    private const string BallCollisionPath = "Assets/Gameplay/Ball/BallCollision.cs";
    private const string AttackEffectSpawnerPath = "Assets/Gameplay/Ball/Effect/AttackEffectSpawner.cs";
    private const string BallAnimatorControllerPath = "Assets/Art/Ball/Animations/BallAnimationController.controller";
    private const string BallRotationPath = "Assets/Gameplay/Ball/BallRotation.cs";

    [Test]
    public void BallTicksAnimationButDoesNotRotateHeadingTowardVelocity()
    {
        string ball = ReadProjectFile(BallPath);

        Assert.That(
            ball,
            Does.Contain("this.Animation.Tick()"),
            "Ball should update animation each frame so horizontal flip and idle up/down follow velocity.");
        Assert.That(
            ball,
            Does.Not.Contain("useVelocityBasedVerticalAnimation"),
            "The flip feature should be removed, not left behind as a disabled toggle.");
        Assert.That(
            ball,
            Does.Not.Contain("UseVelocityBasedVerticalAnimation"),
            "BallAnimation should not have a ball-level switch for velocity-based vertical animation.");
        Assert.That(
            ball,
            Does.Not.Contain("BallRotation"),
            "Ball should not use a rotation helper to point its heading toward velocity.");
        Assert.That(
            ball,
            Does.Not.Contain("Rotation.Tick()"),
            "Ball should not rotate its transform toward velocity.");
    }

    [Test]
    public void BallAnimationDrivesHorizontalFlipAndVerticalIdleState()
    {
        string ballAnimation = ReadProjectFile(BallAnimationPath);

        AssertContainsDirectionCode(ballAnimation, "public void Tick()");
        AssertContainsDirectionCode(ballAnimation, "UpdateIdleUpDownState()");
        AssertContainsDirectionCode(ballAnimation, "UpdateFlipState()");
        AssertContainsDirectionCode(ballAnimation, "SpriteRenderer");
        AssertContainsDirectionCode(ballAnimation, "flipX");
        AssertContainsDirectionCode(ballAnimation, "IsMovingUp");
        AssertContainsDirectionCode(ballAnimation, "Physics.Velocity");
        AssertContainsDirectionCode(ballAnimation, "Velocity.y >= 0f");
        AssertContainsDirectionCode(ballAnimation, "Velocity.x < 0f");
        AssertNoDirectionCode(ballAnimation, "Quaternion.Euler");
        AssertNoDirectionCode(ballAnimation, "transform.rotation");
    }

    [Test]
    public void BallAnimationDoesNotTriggerRemovedCloneOrGiantAnimatorParameters()
    {
        string ballAnimation = ReadProjectFile(BallAnimationPath);

        Assert.That(ballAnimation, Does.Not.Contain("StringToHash(\"Clone\")"));
        Assert.That(ballAnimation, Does.Not.Contain("StringToHash(\"Upsizing\")"));
        Assert.That(ballAnimation, Does.Not.Contain("StringToHash(\"Downsizing\")"));
        Assert.That(ballAnimation, Does.Not.Contain("SetTrigger(this.cloneHash)"));
        Assert.That(ballAnimation, Does.Not.Contain("SetTrigger(this.upsizingHash)"));
        Assert.That(ballAnimation, Does.Not.Contain("SetTrigger(this.downsizingHash)"));
    }

    [Test]
    public void AttackEffectSpawnerDoesNotFlipSpriteFromBallVelocity()
    {
        string ballCollision = ReadProjectFile(BallCollisionPath);
        string attackEffectSpawner = ReadProjectFile(AttackEffectSpawnerPath);

        AssertNoDirectionCode(ballCollision, "flipX");
        AssertNoDirectionCode(ballCollision, "velocity.x < 0f");
        AssertNoDirectionCode(attackEffectSpawner, "flipX");
        AssertNoDirectionCode(attackEffectSpawner, "sr.flipX");
        AssertNoDirectionCode(attackEffectSpawner, "SpriteRenderer");
    }

    [Test]
    public void BallAnimatorControllerContainsVerticalIdleStateMachine()
    {
        string controller = ReadProjectFile(BallAnimatorControllerPath);

        AssertContainsDirectionCode(controller, "IsMovingUp");
        AssertContainsDirectionCode(controller, "Idle_up");
        AssertContainsDirectionCode(controller, "Idle_down");
    }

    [Test]
    public void BallAnimatorControllerOmitsCloneAndGiantAnimationStates()
    {
        string controller = ReadProjectFile(BallAnimatorControllerPath);

        AssertAnimatorParameterAbsent(controller, "Clone");
        AssertAnimatorParameterAbsent(controller, "Upsizing");
        AssertAnimatorParameterAbsent(controller, "Downsizing");
        AssertAnimatorConditionAbsent(controller, "Clone");
        AssertAnimatorConditionAbsent(controller, "Upsizing");
        AssertAnimatorConditionAbsent(controller, "Downsizing");
        AssertAnimatorStateAbsent(controller, "clone");
        AssertAnimatorStateAbsent(controller, "upsizing_ready");
        AssertAnimatorStateAbsent(controller, "upsizing_complete");
        AssertAnimatorStateAbsent(controller, "upsizing_idle");
        AssertAnimatorStateAbsent(controller, "Downsizing");
    }

    [Test]
    public void BallAnimatorControllerAnimationReferencesResolve()
    {
        string controller = ReadProjectFile(BallAnimatorControllerPath);
        MatchCollection motionReferences = Regex.Matches(
            controller,
            @"m_Motion: \{fileID: 7400000, guid: ([0-9a-f]{32}), type: 2\}");

        Assert.That(motionReferences.Count, Is.GreaterThan(0));

        foreach (Match motionReference in motionReferences)
        {
            string guid = motionReference.Groups[1].Value;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            Assert.That(assetPath, Is.Not.Empty, $"Animator motion GUID {guid} does not resolve to an asset.");
            Assert.That(
                AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath),
                Is.Not.Null,
                $"Animator motion GUID {guid} resolves to {assetPath}, but it is not an AnimationClip.");

            AssertSpriteReferencesResolve(assetPath);
        }
    }

    [Test]
    public void BallCloneAndGiantAnimationClipsAreRemovedButSpritesRemain()
    {
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/clone.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/clone 2.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_ready.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_ready 2.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_idle.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_idle 2.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_complete.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/upsizing_complete 2.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/Downsizing.anim");
        AssertProjectFileDoesNotExist("Assets/Art/Ball/Animations/Downsizing 2.anim");

        AssertProjectDirectoryExists("Assets/Art/Ball/Clone");
        AssertProjectDirectoryExists("Assets/Art/Ball/upsizing_ready");
        AssertProjectDirectoryExists("Assets/Art/Ball/upsizing_idle");
        AssertProjectDirectoryExists("Assets/Art/Ball/upsizing_complete");
        AssertProjectDirectoryExists("Assets/Art/Ball/downsizing");
    }

    [Test]
    public void BallAnimatorOneShotStatesExitAfterOnePlayback()
    {
        string controller = ReadProjectFile(BallAnimatorControllerPath);

        AssertOneShotStateExitsAfterPlayback(controller, "attack_up");
        AssertOneShotStateExitsAfterPlayback(controller, "attack_down");
    }

    [Test]
    public void BallDoesNotRotateHeadingTowardVelocity()
    {
        string ball = ReadProjectFile(BallPath);

        AssertNoDirectionCode(ball, "BallRotation");
        AssertNoDirectionCode(ball, "Rotation.Tick()");
        AssertNoDirectionCode(ball, "Rotation.IsEnabled");
        Assert.That(
            File.Exists(ProjectFilePath(BallRotationPath)),
            Is.False,
            "BallRotation.cs should be removed so the ball cannot point its heading toward velocity.");
    }

    private static string ReadProjectFile(string projectRelativePath)
    {
        return File.ReadAllText(ProjectFilePath(projectRelativePath));
    }

    private static string ProjectFilePath(string projectRelativePath)
    {
        return Path.Combine(Application.dataPath, "../", projectRelativePath);
    }

    private static void AssertProjectFileDoesNotExist(string projectRelativePath)
    {
        Assert.That(
            File.Exists(ProjectFilePath(projectRelativePath)),
            Is.False,
            $"{projectRelativePath} should be removed from ball animation clips.");
    }

    private static void AssertProjectDirectoryExists(string projectRelativePath)
    {
        Assert.That(
            Directory.Exists(ProjectFilePath(projectRelativePath)),
            Is.True,
            $"{projectRelativePath} should remain because sprite assets are kept.");
    }

    private static void AssertNoDirectionCode(string source, string forbiddenText)
    {
        Assert.That(
            source,
            Does.Not.Contain(forbiddenText),
            $"{forbiddenText} keeps direction-based ball orientation behavior active.");
    }

    private static void AssertContainsDirectionCode(string source, string requiredText)
    {
        Assert.That(
            source,
            Does.Contain(requiredText),
            $"{requiredText} is required for direction-based ball animation.");
    }

    private static void AssertAnimatorParameterAbsent(string controller, string parameterName)
    {
        Assert.That(
            controller,
            Does.Not.Match($@"(?m)^  - m_Name: {Regex.Escape(parameterName)}$"),
            $"Ball animator should not expose the removed {parameterName} animation parameter.");
    }

    private static void AssertAnimatorConditionAbsent(string controller, string conditionName)
    {
        Assert.That(
            controller,
            Does.Not.Match($@"(?m)^    m_ConditionEvent: {Regex.Escape(conditionName)}$"),
            $"Ball animator should not transition on the removed {conditionName} animation condition.");
    }

    private static void AssertAnimatorStateAbsent(string controller, string stateName)
    {
        Assert.That(
            controller,
            Does.Not.Match($@"(?m)^  m_Name: {Regex.Escape(stateName)}$"),
            $"Ball animator should not keep the removed {stateName} animation state.");
    }

    private static void AssertSpriteReferencesResolve(string animationClipPath)
    {
        string animationClip = ReadProjectFile(animationClipPath);
        MatchCollection spriteReferences = Regex.Matches(
            animationClip,
            @"\{fileID: 21300000, guid: ([0-9a-f]{32}), type: 3\}");

        Assert.That(spriteReferences.Count, Is.GreaterThan(0), $"{animationClipPath} has no sprite frames.");

        foreach (Match spriteReference in spriteReferences)
        {
            string guid = spriteReference.Groups[1].Value;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            Assert.That(assetPath, Is.Not.Empty, $"Animation sprite GUID {guid} in {animationClipPath} does not resolve to an asset.");
            Assert.That(
                AssetDatabase.LoadAssetAtPath<Sprite>(assetPath),
                Is.Not.Null,
                $"Animation sprite GUID {guid} resolves to {assetPath}, but it is not a Sprite.");
        }
    }

    private static void AssertOneShotStateExitsAfterPlayback(string controller, string stateName)
    {
        string stateBlock = FindAnimatorStateBlock(controller, stateName);
        MatchCollection transitionReferences = Regex.Matches(stateBlock, @"- \{fileID: (-?\d+)\}");

        Assert.That(transitionReferences.Count, Is.GreaterThan(0), $"{stateName} has no exit transitions.");

        foreach (Match transitionReference in transitionReferences)
        {
            string transitionBlock = FindAnimatorTransitionBlock(controller, transitionReference.Groups[1].Value);
            Match hasExitTime = Regex.Match(transitionBlock, @"m_HasExitTime: (\d+)");
            Match exitTime = Regex.Match(transitionBlock, @"m_ExitTime: ([^\n]+)");

            Assert.That(hasExitTime.Success, Is.True, $"{stateName} transition has no m_HasExitTime.");
            Assert.That(exitTime.Success, Is.True, $"{stateName} transition has no m_ExitTime.");
            Assert.That(hasExitTime.Groups[1].Value, Is.EqualTo("1"), $"{stateName} should wait for exit time before leaving.");
            Assert.That(
                float.Parse(exitTime.Groups[1].Value, CultureInfo.InvariantCulture),
                Is.GreaterThanOrEqualTo(1f),
                $"{stateName} exits before its clip can play once.");
        }
    }

    private static string FindAnimatorStateBlock(string controller, string stateName)
    {
        foreach (Match stateBlock in Regex.Matches(
            controller,
            @"^--- !u!1102 &-?\d+\n[\s\S]*?(?=^--- !u!|\z)",
            RegexOptions.Multiline))
        {
            if (stateBlock.Value.Contains($"\n  m_Name: {stateName}\n"))
                return stateBlock.Value;
        }

        Assert.Fail($"Animator state {stateName} was not found.");
        return string.Empty;
    }

    private static string FindAnimatorTransitionBlock(string controller, string transitionFileId)
    {
        Match transitionBlock = Regex.Match(
            controller,
            $@"^--- !u!1101 &{Regex.Escape(transitionFileId)}\n[\s\S]*?(?=^--- !u!|\z)",
            RegexOptions.Multiline);

        Assert.That(transitionBlock.Success, Is.True, $"Animator transition {transitionFileId} was not found.");
        return transitionBlock.Value;
    }
}
#endif
