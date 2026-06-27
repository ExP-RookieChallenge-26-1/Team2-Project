#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class GameManagerSpawnFlowTests
{
    private const string GameManagerPath = "Assets/_Global/GameManager.cs";
    private const string BallPath = "Assets/Gameplay/Ball/Ball.cs";
    private const string BallAnimatorControllerPath = "Assets/Art/Ball/Animations/BallAnimationController.controller";

    [Test]
    public void GameStartUsesInitialSpawnInsteadOfRespawn()
    {
        string gameManager = ReadProjectFile(GameManagerPath);
        string startBody = ExtractMethodBody(gameManager, "Start");

        Assert.That(
            startBody,
            Does.Contain("TriggerInitialSpawn()"),
            "Game start should create and launch the first ball without using the respawn delay.");
        Assert.That(
            startBody,
            Does.Not.Contain("TriggerSpawn(false)"),
            "Disabling only the respawn sound still leaves the ball in the respawn state.");
    }

    [Test]
    public void InitialSpawnLaunchesBallImmediatelyWithoutRespawnState()
    {
        string gameManager = ReadProjectFile(GameManagerPath);
        string initialSpawnBody = ExtractMethodBody(gameManager, "TriggerInitialSpawn");

        Assert.That(
            initialSpawnBody,
            Does.Contain("ball.LaunchImmediately()"),
            "Initial spawn should launch the ball immediately instead of entering the respawn state.");
        Assert.That(
            initialSpawnBody,
            Does.Not.Contain("ball.Spawn()"),
            "Initial spawn must not call the respawn path.");
        Assert.That(
            initialSpawnBody,
            Does.Not.Contain("PlayRespawnSound"),
            "Initial game start should not play respawn audio.");
    }

    [Test]
    public void LostBallRespawnsWithoutDamagingUser()
    {
        string gameManager = ReadProjectFile(GameManagerPath);
        string checkBallStateBody = ExtractMethodBody(gameManager, "CheckBallState");

        Assert.That(
            checkBallStateBody,
            Does.Not.Contain("TakeDamage"),
            "Losing the ball should not reduce user HP.");
        Assert.That(
            checkBallStateBody,
            Does.Contain("TriggerSpawn()"),
            "Losing the ball should still respawn a new ball.");
    }

    [Test]
    public void BallImmediateLaunchSkipsSpawnTimerAndRespawnAnimation()
    {
        string ball = ReadProjectFile(BallPath);
        string launchImmediatelyBody = ExtractMethodBody(ball, "LaunchImmediately");

        Assert.That(launchImmediatelyBody, Does.Contain("this.isSpawning = false"));
        Assert.That(launchImmediatelyBody, Does.Contain("this.spawnTimer = 0f"));
        Assert.That(launchImmediatelyBody, Does.Contain("this.Animation.SetRespawning(false)"));
        Assert.That(launchImmediatelyBody, Does.Contain("Launch()"));
    }

    [Test]
    public void BallAnimatorDefaultsToIdleInsteadOfRespawn()
    {
        string controller = ReadProjectFile(BallAnimatorControllerPath);
        string defaultState = ExtractDefaultStateFileId(controller);
        string idleDownState = ExtractAnimatorStateFileId(controller, "Idle_down");
        string respawnState = ExtractAnimatorStateFileId(controller, "Respawn");

        Assert.That(
            defaultState,
            Is.EqualTo(idleDownState),
            "The ball animator should not enter Respawn before gameplay code runs.");
        Assert.That(defaultState, Is.Not.EqualTo(respawnState));
    }

    private static string ReadProjectFile(string projectRelativePath)
    {
        return File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        string signature = $"void {methodName}(";
        int signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
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

    private static string ExtractDefaultStateFileId(string controller)
    {
        string marker = "m_DefaultState: {fileID: ";
        int markerIndex = controller.IndexOf(marker, StringComparison.Ordinal);
        Assert.That(markerIndex, Is.GreaterThanOrEqualTo(0), "Animator default state was not found.");

        int valueStart = markerIndex + marker.Length;
        int valueEnd = controller.IndexOf('}', valueStart);
        Assert.That(valueEnd, Is.GreaterThan(valueStart), "Animator default state fileID was malformed.");
        return controller.Substring(valueStart, valueEnd - valueStart);
    }

    private static string ExtractAnimatorStateFileId(string controller, string stateName)
    {
        string marker = $"m_Name: {stateName}";
        int nameIndex = controller.IndexOf(marker, StringComparison.Ordinal);
        Assert.That(nameIndex, Is.GreaterThanOrEqualTo(0), $"{stateName} animator state was not found.");

        int stateStart = controller.LastIndexOf("--- !u!1102 &", nameIndex, StringComparison.Ordinal);
        Assert.That(stateStart, Is.GreaterThanOrEqualTo(0), $"{stateName} animator state fileID was not found.");

        int valueStart = stateStart + "--- !u!1102 &".Length;
        int valueEnd = controller.IndexOf('\n', valueStart);
        Assert.That(valueEnd, Is.GreaterThan(valueStart), $"{stateName} animator state fileID was malformed.");
        return controller.Substring(valueStart, valueEnd - valueStart).Trim();
    }
}
#endif
