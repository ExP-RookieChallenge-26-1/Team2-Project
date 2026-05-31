using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[SerializeField] private AudioClip ballHitClip;
	private AudioSource audioSource;

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
	}

	public void PlayBallHitSound()
	{
		if (this.ballHitClip != null && this.audioSource != null)
		{
			this.audioSource.PlayOneShot(this.ballHitClip);
		}
	}
}
