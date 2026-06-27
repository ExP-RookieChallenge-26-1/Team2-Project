using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	private const float BgmOutputHeadroom = 0.5f;
	private const float SfxOutputHeadroom = 0.25f;
	private const int MaxSfxVoices = 4;
	private const float BallHitSfxCooldownSeconds = 0.12f;

	[SerializeField] private AudioClip ballHitClip;
    [SerializeField] private AudioClip bossDieClip;
    [SerializeField] private AudioClip gameClearClip;
    [SerializeField] private AudioClip userDamagedClip;
    [SerializeField] private AudioClip respawnClip;
    [SerializeField] private AudioClip getItemClip;
    [SerializeField] private AudioClip cloneClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip attackVoiceClip;
    [SerializeField] private AudioClip mobDieClip;
    [SerializeField] private AudioClip mobDamagedVoiceClip;
    [SerializeField] private AudioClip upgradeSelectClip;
    [SerializeField] private AudioClip bossVoiceClip;
    [SerializeField] private AudioClip bossIdleClip;
    [SerializeField] private AudioClip bossFallClip;
    [SerializeField] private AudioClip bossEntryClip;
    [SerializeField] private AudioClip bossAttackReadyClip;
    [SerializeField] private AudioClip bossAttackOutloopClip;
    [SerializeField] private AudioClip bossAttackLoopClip;
    [SerializeField] private AudioClip bossAttackInloopClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip bossBgmClip;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip gameBgmClip;
    [SerializeField] private AudioClip cloneSkillClip;

    private AudioSource audioSource;
    private AudioSource bossAttackSource;
    private Coroutine bossAttackLoopRoutine;
    private readonly List<AudioSource> sfxSources = new List<AudioSource>();
    private readonly System.Collections.Generic.Dictionary<AudioClip, float> lastSfxPlayTimes = new System.Collections.Generic.Dictionary<AudioClip, float>();

	private float masterVolume = 1f;
	private float bgmVolume = 1f;
	private float sfxVolume = 1f;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		this.audioSource = GetComponent<AudioSource>();
		if (this.audioSource == null)
		{
			this.audioSource = gameObject.AddComponent<AudioSource>();
		}
		this.audioSource.playOnAwake = false;
		this.audioSource.spatialBlend = 0f;
		this.audioSource.volume = 1f;
		this.audioSource.loop = false;

        this.sfxSources.Add(this.audioSource);
        EnsureSfxSourcePool();

		EnsureBgmSource();
		EnsureBossAttackSource();

		LoadVolumeSettings();
	}

    public void PlayBallHitSound()
    {
        PlaySfx(ballHitClip, BallHitSfxCooldownSeconds);
    }


    public void PlayBossDieSound()
    {
        PlaySfx(bossDieClip);
    }

    public void PlayGameClearSound()
    {
        PlaySfx(gameClearClip);
    }
    public void PlayUserDamagedSound()
    {
        PlaySfx(userDamagedClip);
    }

    public void PlayRespawnSound()
    {
        PlaySfx(respawnClip);
    }

    public void PlayGetItemSound()
    {
        PlaySfx(getItemClip);
    }

    public void PlayCloneSound()
    {
        PlaySfx(cloneClip);
    }

    public void PlayAttackSound()
    {
        PlaySfx(attackClip);
    }

    public void PlayAttackVoiceSound()
    {
        PlaySfx(attackVoiceClip);
    }

    public void PlayMobDieSound()
    {
        PlaySfx(mobDieClip);
    }

    public void PlayMobDamagedVoiceSound()
    {
        PlaySfx(mobDamagedVoiceClip);
    }

    public void PlayUpgradeSelectSound()
    {
        PlaySfx(upgradeSelectClip);
    }
    public void PlayBossVoiceSound()
    {
        PlaySfx(bossVoiceClip);
    }

    public void PlayBossIdleSound()
    {
        PlaySfx(bossIdleClip);
    }

    public void PlayBossFallSound()
    {
        PlaySfx(bossFallClip);
    }

    public void PlayBossEntrySound()
    {
        PlaySfx(bossEntryClip);
    }

    public void PlayBossAttackReadySound()
    {
        PlayManagedBossAttackClip(bossAttackReadyClip, false);
    }

    public void PlayBossAttackOutloopSound()
    {
        PlayManagedBossAttackClip(bossAttackOutloopClip, false);
    }

    public void PlayBossAttackLoopSound()
    {
        StopBossAttackLoopRoutine();
        bossAttackLoopRoutine = StartCoroutine(PlayBossAttackLoopRoutine());
    }

    public void PlayCloneSkillSound()
    {
        PlaySfx(cloneSkillClip);
    }
    public void PlayBossAttackInloopSound()
    {
        PlayManagedBossAttackClip(bossAttackInloopClip, false);
    }
    public void PlayGameOverSound()
    {
        PlaySfx(gameOverClip);
    }
    public void PlayGameBgm()
    {
        if (gameBgmClip == null)
        {
            Debug.LogWarning("Game BGM Clip이 연결되지 않았습니다.");
            return;
        }

        PlayBgm(gameBgmClip, $"게임 BGM 재생: {gameBgmClip.name}");
    }

    public void PlayBossBgm()
    {
        if (bossBgmClip == null)
        {
            Debug.LogWarning("Boss BGM Clip이 연결되지 않았습니다.");
            return;
        }

        PlayBgm(bossBgmClip, "우마왕 BGM 재생");
    }
    public void StopBgm()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }
	public void SetMasterVolume(float volume)
	{
		masterVolume = Mathf.Clamp01(volume);
		UpdateBgmVolume();
		UpdateSfxVolume();
		SaveVolumeSettings();
	}

	public void SetBGMVolume(float volume)
	{
		bgmVolume = Mathf.Clamp01(volume);
		UpdateBgmVolume();
		SaveVolumeSettings();
	}

	public void SetSFXVolume(float volume)
	{
		sfxVolume = Mathf.Clamp01(volume);
		UpdateSfxVolume();
		SaveVolumeSettings();
	}

	public float GetMasterVolume() => masterVolume;
	public float GetBGMVolume() => bgmVolume;
	public float GetSFXVolume() => sfxVolume;

	private void SaveVolumeSettings()
	{
		PlayerPrefs.SetFloat("MasterVolume", masterVolume);
		PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
		PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
		PlayerPrefs.Save();
	}
    private void PlaySfx(AudioClip clip)
    {
        PlaySfx(clip, 0f);
    }

    private void PlaySfx(AudioClip clip, float cooldownSeconds)
    {
        if (clip == null || !CanPlaySfx(clip, cooldownSeconds))
            return;

        AudioSource source = GetAvailableSfxSource();
        if (source == null)
            return;

        lastSfxPlayTimes[clip] = Time.unscaledTime;
        ConfigureSfxSource(source);
        source.clip = clip;
        source.volume = GetSfxVolumeScale();
        source.Play();
    }

    private void PlayBgm(AudioClip clip, string logMessage)
    {
        AudioSource source = EnsureBgmSource();

        if (source.clip == clip && source.isPlaying)
        {
            UpdateBgmVolume();
            return;
        }

        source.Stop();
        source.clip = clip;
        source.volume = GetBgmVolumeScale();
        source.Play();

        Debug.Log(logMessage);
    }

    private AudioSource EnsureBgmSource()
    {
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        return bgmSource;
    }

    private AudioSource EnsureBossAttackSource()
    {
        if (bossAttackSource == null)
            bossAttackSource = gameObject.AddComponent<AudioSource>();

        bossAttackSource.playOnAwake = false;
        bossAttackSource.spatialBlend = 0f;
        bossAttackSource.volume = GetSfxVolumeScale();
        return bossAttackSource;
    }

    private void EnsureSfxSourcePool()
    {
        while (sfxSources.Count < MaxSfxVoices)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            ConfigureSfxSource(source);
            sfxSources.Add(source);
        }
    }

    private void ConfigureSfxSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = false;
    }

    private AudioSource GetAvailableSfxSource()
    {
        EnsureSfxSourcePool();

        for (int i = 0; i < sfxSources.Count; i++)
        {
            if (!sfxSources[i].isPlaying)
                return sfxSources[i];
        }

        return null;
    }

    private bool CanPlaySfx(AudioClip clip, float cooldownSeconds)
    {
        if (cooldownSeconds <= 0f)
            return true;

        if (!lastSfxPlayTimes.TryGetValue(clip, out float lastPlayTime))
            return true;

        return Time.unscaledTime - lastPlayTime >= cooldownSeconds;
    }

    private IEnumerator PlayBossAttackLoopRoutine()
    {
        AudioSource source = EnsureBossAttackSource();
        source.Stop();

        if (bossAttackInloopClip != null)
        {
            ConfigureManagedBossAttackSource(source, bossAttackInloopClip, false);
            source.Play();
            yield return new WaitForSecondsRealtime(bossAttackInloopClip.length);
        }

        if (bossAttackLoopClip != null)
        {
            ConfigureManagedBossAttackSource(source, bossAttackLoopClip, true);
            source.Play();
        }

        bossAttackLoopRoutine = null;
    }

    private void PlayManagedBossAttackClip(AudioClip clip, bool loop)
    {
        if (clip == null)
            return;

        StopBossAttackLoopRoutine();

        AudioSource source = EnsureBossAttackSource();
        source.Stop();
        ConfigureManagedBossAttackSource(source, clip, loop);
        source.Play();
    }

    private void ConfigureManagedBossAttackSource(AudioSource source, AudioClip clip, bool loop)
    {
        source.clip = clip;
        source.loop = loop;
        source.volume = GetSfxVolumeScale();
    }

    private void StopBossAttackLoopRoutine()
    {
        if (bossAttackLoopRoutine == null)
            return;

        StopCoroutine(bossAttackLoopRoutine);
        bossAttackLoopRoutine = null;
    }

    private void UpdateBgmVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = GetBgmVolumeScale();
    }

    private void UpdateSfxVolume()
    {
        foreach (AudioSource source in sfxSources)
            source.volume = GetSfxVolumeScale();

        if (bossAttackSource != null)
            bossAttackSource.volume = GetSfxVolumeScale();
    }

    private float GetBgmVolumeScale()
    {
        return Mathf.Clamp01(masterVolume * bgmVolume * BgmOutputHeadroom);
    }

    private float GetSfxVolumeScale()
    {
        return Mathf.Clamp01(masterVolume * sfxVolume * SfxOutputHeadroom);
    }

	private void LoadVolumeSettings()
	{
		masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
		sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
	}

}
