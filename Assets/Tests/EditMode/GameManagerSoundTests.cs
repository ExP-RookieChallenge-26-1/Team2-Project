#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class GameManagerSoundTests
{
    private const string GameManagerPath = "Assets/_Global/GameManager.cs";
    private const string ReturnToTitleButtonPath = "Assets/UI/ReturnToTitleButton.cs";
    private const string TitleUIPath = "Assets/UI/Title/TitleUI.cs";
    private const string UserHealthPath = "Assets/_Global/UserHealth.cs";
    private const string GiantSkillPath = "Assets/Gameplay/Ball/GiantSkill.cs";
    private const string AudioManagerPath = "Assets/_Global/AudioManager.cs";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string DownsizingSoundGuid = "a6d87d0d327ecb54497e0cb3c21bcf4d";

    [Test]
    public void GameOverSoundIsHandledByGameManagerStateChange()
    {
        string gameManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameManagerPath));
        Match gameOverCase = Regex.Match(
            gameManager,
            @"case\s+GameStateMachine\.State\.GameOver:(?<body>.*?)\n\s*break;",
            RegexOptions.Singleline);

        Assert.That(gameOverCase.Success, Is.True, "GameManager is missing the GameOver state branch.");
        Assert.That(
            gameOverCase.Groups["body"].Value,
            Does.Contain("AudioManager.Instance.PlayGameOverSound()"),
            "GameManager should play the game-over sound when the game enters GameOver.");
    }

    [Test]
    public void GameStartUsesDeterministicGameBgm()
    {
        string gameManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameManagerPath));

        Assert.That(
            gameManager,
            Does.Contain("AudioManager.Instance.PlayGameBgm()"),
            "GameManager should start the fixed game-scene BGM.");
        Assert.That(
            gameManager,
            Does.Not.Contain("PlayRandomGameBgm"),
            "GameManager should not start gameplay with random BGM selection.");
    }

    [Test]
    public void ReturningToTitleStopsPersistentBgm()
    {
        string gameManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameManagerPath));
        Match goToTitle = Regex.Match(
            gameManager,
            @"public\s+void\s+GoToTitle\(\)(?<body>.*?)\n\s*\}",
            RegexOptions.Singleline);

        Assert.That(goToTitle.Success, Is.True, "GameManager.GoToTitle is missing.");
        Assert.That(
            goToTitle.Groups["body"].Value,
            Does.Contain("AudioManager.Instance.StopBgm()"),
            "Returning to TitleScene should stop persistent gameplay or boss BGM before the title AudioSource starts.");
    }

    [Test]
    public void ReturningToTitleResetsSessionState()
    {
        string gameManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameManagerPath));
        Match goToTitle = Regex.Match(
            gameManager,
            @"public\s+void\s+GoToTitle\(\)(?<body>.*?)\n\s*\}",
            RegexOptions.Singleline);

        Assert.That(goToTitle.Success, Is.True, "GameManager.GoToTitle is missing.");
        Assert.That(
            goToTitle.Groups["body"].Value,
            Does.Contain("ResetSessionState()"),
            "Returning to TitleScene should clear gameplay session state.");
    }

    [Test]
    public void ResultPanelReturnButtonStopsPersistentBgm()
    {
        string returnButton = File.ReadAllText(Path.Combine(Application.dataPath, "../", ReturnToTitleButtonPath));
        Match returnToTitle = Regex.Match(
            returnButton,
            @"private\s+void\s+ReturnToTitle\(\)(?<body>.*?)\n\s*\}",
            RegexOptions.Singleline);

        Assert.That(returnToTitle.Success, Is.True, "ReturnToTitleButton.ReturnToTitle is missing.");
        Assert.That(
            returnToTitle.Groups["body"].Value,
            Does.Contain("AudioManager.Instance.StopBgm()"),
            "Result-panel title return should stop persistent gameplay or boss BGM before loading TitleScene.");
    }

    [Test]
    public void ResultPanelReturnButtonResetsSessionState()
    {
        string returnButton = File.ReadAllText(Path.Combine(Application.dataPath, "../", ReturnToTitleButtonPath));
        Match returnToTitle = Regex.Match(
            returnButton,
            @"private\s+void\s+ReturnToTitle\(\)(?<body>.*?)\n\s*\}",
            RegexOptions.Singleline);

        Assert.That(returnToTitle.Success, Is.True, "ReturnToTitleButton.ReturnToTitle is missing.");
        Assert.That(
            returnToTitle.Groups["body"].Value,
            Does.Contain("GameManager.ResetSessionState()"),
            "Result-panel title return should clear gameplay session state.");
    }

    [Test]
    public void StartingGameResetsSessionState()
    {
        string titleUI = File.ReadAllText(Path.Combine(Application.dataPath, "../", TitleUIPath));
        Match startClicked = Regex.Match(
            titleUI,
            @"private\s+void\s+OnStartClicked\(\)(?<body>.*?)\n\s*\}",
            RegexOptions.Singleline);

        Assert.That(startClicked.Success, Is.True, "TitleUI.OnStartClicked is missing.");
        Assert.That(
            startClicked.Groups["body"].Value,
            Does.Contain("GameManager.ResetSessionState()"),
            "Starting a game from TitleScene should begin from a clean gameplay session.");
    }

    [Test]
    public void UserHealthDoesNotPlayGameOverSoundDirectly()
    {
        string userHealth = File.ReadAllText(Path.Combine(Application.dataPath, "../", UserHealthPath));

        Assert.That(
            userHealth,
            Does.Not.Contain("PlayGameOverSound"),
            "Game-over sound belongs to GameManager's GameOver state handling to avoid duplicate playback.");
    }

    [Test]
    public void BallSizeSkillDoesNotPlayDownsizingSound()
    {
        string giantSkill = File.ReadAllText(Path.Combine(Application.dataPath, "../", GiantSkillPath));
        string audioManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", AudioManagerPath));
        string gameScene = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameScenePath));

        Assert.That(giantSkill, Does.Not.Contain("PlayGiantSkillSound"));
        Assert.That(audioManager, Does.Not.Contain("giantSkillClip"));
        Assert.That(audioManager, Does.Not.Contain("PlayGiantSkillSound"));
        Assert.That(gameScene, Does.Not.Contain(DownsizingSoundGuid));
    }
}
#endif
