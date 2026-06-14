using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[SerializeField] private AudioClip ballHitClip;
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

		LoadVolumeSettings();
	}

	public void PlayBallHitSound()
	{
		if (this.ballHitClip != null && this.audioSource != null)
		{
			this.audioSource.volume = masterVolume * sfxVolume;
			this.audioSource.PlayOneShot(this.ballHitClip);
		}
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

	private void LoadVolumeSettings()
	{
		masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
		sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
	}
}
