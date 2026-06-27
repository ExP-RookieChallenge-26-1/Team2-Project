#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class AudioManagerSceneConfigurationTests
{
    private const string AudioManagerPath = "Assets/_Global/AudioManager.cs";
    private const string CowKingPath = "Assets/Gameplay/Cowking/CowKing.cs";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string TitleBgmGuid = "2003e7f08474e53488d085732995a01d";
    private const string MainThemeV2Guid = "2f05ad70f06134142b7cd6f5860a8d40";
    private const string MainThemeV3Guid = "51f711c0d7d4a284da9c7ff6c370dea7";
    private const string BossThemeGuid = "b9ab4f416f339124d9185a8eae179498";

    private static readonly string[] RuntimeAudioClipMetaPaths =
    {
        "Assets/SFX/BGM/굴러가요손오공titlebgm.wav.meta",
        "Assets/SFX/BGM/손오공mainthemev3.wav.meta",
        "Assets/SFX/BGM/손오공 우마왕 theme.wav.meta",
        "Assets/SFX/효과음v3_2배속/0.2.0 18-Audio.wav.meta",
        "Assets/SFX/team2proj_soundeffects/attack.wav.meta",
        "Assets/SFX/team2proj_soundeffects/attack_voice.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_attack_inloop.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_attack_loop.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_attack_outloop.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_attack_ready.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_die.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_entry.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_fall.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_idle.wav.meta",
        "Assets/SFX/team2proj_soundeffects/boss_voice.wav.meta",
        "Assets/SFX/team2proj_soundeffects/bounce.wav.meta",
        "Assets/SFX/team2proj_soundeffects/card_select.wav.meta",
        "Assets/SFX/team2proj_soundeffects/clone.wav.meta",
        "Assets/SFX/team2proj_soundeffects/gameover.wav.meta",
        "Assets/SFX/team2proj_soundeffects/getitem.wav.meta",
        "Assets/SFX/team2proj_soundeffects/mob_damaged_voice.wav.meta",
        "Assets/SFX/team2proj_soundeffects/mob_die.wav.meta",
        "Assets/SFX/team2proj_soundeffects/respawn.wav.meta",
        "Assets/SFX/team2proj_soundeffects/user_dmaged.wav.meta",
    };

    private static readonly string[] RequiredSfxFields =
    {
        "ballHitClip",
        "bossDieClip",
        "userDamagedClip",
        "respawnClip",
        "getItemClip",
        "cloneClip",
        "attackClip",
        "attackVoiceClip",
        "mobDieClip",
        "mobDamagedVoiceClip",
        "upgradeSelectClip",
        "bossVoiceClip",
        "bossIdleClip",
        "bossFallClip",
        "bossEntryClip",
        "bossAttackReadyClip",
        "bossAttackOutloopClip",
        "bossAttackLoopClip",
        "bossAttackInloopClip",
        "gameOverClip",
        "cloneSkillClip",
    };

    [Test]
    public void GameSceneAudioManagerHasGameplaySfxReferences()
    {
        string audioManager = ReadGameSceneAudioManagerBlock();

        foreach (string fieldName in RequiredSfxFields)
        {
            StringAssert.DoesNotContain(
                $"{fieldName}: {{fileID: 0}}",
                audioManager,
                $"{fieldName} is not assigned in GameScene AudioManager.");
        }
    }

    [Test]
    public void GameSceneAudioManagerHasBgmReferences()
    {
        string audioManager = ReadGameSceneAudioManagerBlock();

        StringAssert.Contains($"bossBgmClip: {{fileID: 8300000, guid: {BossThemeGuid}, type: 3}}", audioManager);
        StringAssert.Contains($"gameBgmClip: {{fileID: 8300000, guid: {MainThemeV3Guid}, type: 3}}", audioManager);
        StringAssert.DoesNotContain(MainThemeV2Guid, audioManager);
    }

    [Test]
    public void TitleSceneAudioSourceUsesTitleBgm()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "../", TitleScenePath));
        Match match = Regex.Match(
            scene,
            $@"m_Name:\s*TitleBgm(?<body>.*?m_Resource:\s*\{{fileID:\s*8300000,\s*guid:\s*{TitleBgmGuid},\s*type:\s*3\}}.*?Loop:\s*1)",
            RegexOptions.Singleline);

        Assert.That(match.Success, Is.True, "TitleScene is missing the TitleBgm object.");
        StringAssert.Contains($"m_Resource: {{fileID: 8300000, guid: {TitleBgmGuid}, type: 3}}", match.Value);
        StringAssert.Contains("m_PlayOnAwake: 1", match.Value);
        StringAssert.Contains("m_Volume: 0.6", match.Value);
        StringAssert.Contains("Loop: 1", match.Value);
    }

    [Test]
    public void AudioManagerKeepsMixerHeadroom()
    {
        string audioManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", AudioManagerPath));

        StringAssert.Contains("private const float BgmOutputHeadroom", audioManager);
        StringAssert.Contains("private const float SfxOutputHeadroom", audioManager);
        StringAssert.Contains("private const int MaxSfxVoices", audioManager);
        StringAssert.Contains("GetBgmVolumeScale()", audioManager);
        StringAssert.Contains("GetSfxVolumeScale()", audioManager);
        StringAssert.Contains("GetAvailableSfxSource()", audioManager);
        StringAssert.DoesNotContain("PlayOneShot", audioManager);
        StringAssert.DoesNotContain("audioSource.volume = masterVolume * sfxVolume", audioManager);
        StringAssert.DoesNotContain("bgmSource.volume = masterVolume * bgmVolume", audioManager);
        StringAssert.DoesNotContain("source.volume = masterVolume * bgmVolume", audioManager);
    }

    [Test]
    public void AudioManagerLimitsRepeatedBallHitSfx()
    {
        string audioManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", AudioManagerPath));

        StringAssert.Contains("private const float BallHitSfxCooldownSeconds", audioManager);
        StringAssert.Contains("private readonly System.Collections.Generic.Dictionary<AudioClip, float> lastSfxPlayTimes", audioManager);
        StringAssert.Contains("PlaySfx(ballHitClip, BallHitSfxCooldownSeconds)", audioManager);
        StringAssert.Contains("CanPlaySfx(clip, cooldownSeconds)", audioManager);
    }

    [Test]
    public void RuntimeAudioClipsPreloadAudioData()
    {
        foreach (string metaPath in RuntimeAudioClipMetaPaths)
        {
            string meta = File.ReadAllText(Path.Combine(Application.dataPath, "../", metaPath));

            StringAssert.Contains(
                "preloadAudioData: 1",
                meta,
                $"{metaPath} should preload audio data to avoid runtime decode hitches during playback.");
        }
    }

    [Test]
    public void BossAttackSfxUsesSingleManagedSource()
    {
        string audioManager = File.ReadAllText(Path.Combine(Application.dataPath, "../", AudioManagerPath));
        string cowKing = File.ReadAllText(Path.Combine(Application.dataPath, "../", CowKingPath));

        StringAssert.Contains("private AudioSource bossAttackSource;", audioManager);
        StringAssert.Contains("private Coroutine bossAttackLoopRoutine;", audioManager);
        StringAssert.Contains("StartCoroutine(PlayBossAttackLoopRoutine())", audioManager);
        StringAssert.Contains("PlayManagedBossAttackClip(bossAttackReadyClip, false)", audioManager);
        StringAssert.Contains("PlayManagedBossAttackClip(bossAttackOutloopClip, false)", audioManager);
        StringAssert.Contains("EnsureBossAttackSource()", audioManager);

        StringAssert.DoesNotContain("AudioManager.Instance.PlayBossAttackInloopSound();", cowKing);
    }

    private static string ReadGameSceneAudioManagerBlock()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "../", GameScenePath));
        Match match = Regex.Match(
            scene,
            @"m_EditorClassIdentifier:\s*Assembly-CSharp::AudioManager(?<body>.*?)(?=\n--- !u!)",
            RegexOptions.Singleline);

        Assert.That(match.Success, Is.True, "GameScene is missing an AudioManager component.");
        return match.Value;
    }
}
#endif
