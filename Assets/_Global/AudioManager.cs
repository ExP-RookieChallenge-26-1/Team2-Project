using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

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
    [SerializeField] private AudioClip[] gameBgmClips;
    [SerializeField] private AudioClip giantSkillClip;
    [SerializeField] private AudioClip cloneSkillClip;

    private AudioSource audioSource;

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

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
        }

        LoadVolumeSettings();
	}

    public void PlayBallHitSound()
    {
        PlaySfx(ballHitClip);
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
        PlaySfx(bossAttackReadyClip);
    }

    public void PlayBossAttackOutloopSound()
    {
        PlaySfx(bossAttackOutloopClip);
    }

    public void PlayBossAttackLoopSound()
    {
        PlaySfx(bossAttackLoopClip);
    }
    public void PlayGiantSkillSound()
    {
        PlaySfx(giantSkillClip);
    }

    public void PlayCloneSkillSound()
    {
        PlaySfx(cloneSkillClip);
    }
    public void PlayBossAttackInloopSound()
    {
        PlaySfx(bossAttackInloopClip);
    }
    public void PlayGameOverSound()
    {
        PlaySfx(gameOverClip);
    }
    public void PlayRandomGameBgm()
    {
        if (gameBgmClips == null || gameBgmClips.Length == 0)
        {
            Debug.LogWarning("Game BGM Clips가 비어 있습니다.");
            return;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
        }

        int index = Random.Range(0, gameBgmClips.Length);

        bgmSource.Stop();
        bgmSource.clip = gameBgmClips[index];
        bgmSource.volume = masterVolume * bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log($"게임 BGM 재생: {bgmSource.clip.name}");
    }
    public void PlayBossBgm()
    {
        if (bossBgmClip == null)
        {
            Debug.LogWarning("Boss BGM Clip이 연결되지 않았습니다.");
            return;
        }

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.clip = bossBgmClip;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();

        Debug.Log("우마왕 BGM 재생");
    }
    public void StopBgm()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }
    public void SetMasterVolume(float volume)
	{
		masterVolume = Mathf.Clamp01(volume);
		SaveVolumeSettings();
	}

	public void SetBGMVolume(float volume)
	{
		bgmVolume = Mathf.Clamp01(volume);
		SaveVolumeSettings();
	}

	public void SetSFXVolume(float volume)
	{
		sfxVolume = Mathf.Clamp01(volume);
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
        if (clip == null || this.audioSource == null)
            return;

        this.audioSource.volume = masterVolume * sfxVolume;
        this.audioSource.PlayOneShot(clip);
    }
    private void LoadVolumeSettings()
	{
		masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
		sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
	}

}
