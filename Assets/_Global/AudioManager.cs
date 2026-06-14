using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[SerializeField] private AudioClip ballHitClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private AudioClip bossHitClip;
    [SerializeField] private AudioClip bossBreathClip;
    [SerializeField] private AudioClip bossDieClip;
    [SerializeField] private AudioClip gameClearClip;
    [SerializeField] private AudioClip userDamagedClip;
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
    public void PlayEnemyHitSound()
    {
        if (this.enemyHitClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.enemyHitClip);
        }
    }

    public void PlayBossHitSound()
    {
        if (this.bossHitClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.bossHitClip);
        }
    }

    public void PlayBossBreathSound()
    {
        if (this.bossBreathClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.bossBreathClip);
        }
    }

    public void PlayBossDieSound()
    {
        if (this.bossDieClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.bossDieClip);
        }
    }

    public void PlayGameClearSound()
    {
        if (this.gameClearClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.gameClearClip);
        }
    }
    public void PlayUserDamagedSound()
    {
        if (this.userDamagedClip != null && this.audioSource != null)
        {
            this.audioSource.PlayOneShot(this.userDamagedClip);
        }
    }
}
