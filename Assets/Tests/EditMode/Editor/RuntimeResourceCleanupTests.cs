#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class RuntimeResourceCleanupTests
{
    private const string BallTrajectoryPath = "Assets/Gameplay/Ball/BallTrajectory.cs";
    private const string EnemyPath = "Assets/Gameplay/Enemy/Enemy.cs";
    private const string GameManagerPath = "Assets/_Global/GameManager.cs";
    private const string TitleUIPath = "Assets/UI/Title/TitleUI.cs";

    [Test]
    public void BallTrajectoryDestroysRuntimeMaterialAndTexture()
    {
        string source = ReadProjectFile(BallTrajectoryPath);
        string destroyBody = ExtractMethodBody(source, "OnDestroy");

        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.trajectoryMaterial)"));
        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.dashTexture)"));
    }

    [Test]
    public void EnemyDestroysInstantiatedStats()
    {
        string source = ReadProjectFile(EnemyPath);
        string destroyBody = ExtractMethodBody(source, "OnDestroy");

        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.Stats)"));
    }

    [Test]
    public void GameManagerDestroysInstantiatedRuntimeStats()
    {
        string source = ReadProjectFile(GameManagerPath);
        string destroyBody = ExtractMethodBody(source, "OnDestroy");

        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.BallStats)"));
        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.PaddleStats)"));
        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.WorldStats)"));
    }

    [Test]
    public void TitleUIDestroysGeneratedSprites()
    {
        string source = ReadProjectFile(TitleUIPath);
        string destroyBody = ExtractMethodBody(source, "OnDestroy");

        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.runtimeBackgroundSprite)"));
        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.runtimeLogoSprite)"));
        Assert.That(destroyBody, Does.Contain("DestroyRuntimeObject(this.runtimeSettingButtonSprite)"));
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
}
#endif
