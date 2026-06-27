using UnityEngine;

public class Paddle : MonoBehaviour
{
	public PaddleStats Stats { get; private set; }
	public PaddlePhysics Physics { get; private set; }
	public PaddleVisual Visual { get; private set; }
	private MobDamageOverlay damageOverlay;

	private void Awake()
	{
		this.Physics = new PaddlePhysics(this);
		this.Visual = GetComponent<PaddleVisual>();

		if (this.Visual == null)
			this.Visual = gameObject.AddComponent<PaddleVisual>();

		this.damageOverlay = GetComponent<MobDamageOverlay>();
		if (this.damageOverlay == null)
			this.damageOverlay = gameObject.AddComponent<MobDamageOverlay>();
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.PaddleStats;
		this.Visual.Initialize(this);
	}

	private void Update()
	{
		this.Physics.Tick();
		this.Visual.Tick();
	}

	public bool SetPaddleSizeLevel(int level)
	{
		return this.Stats != null && this.Stats.SetPaddleSizeLevel(level);
	}

	public bool IncreasePaddleSizeLevel(int amount)
	{
		return this.Stats != null && this.Stats.IncreasePaddleSizeLevel(amount);
	}

	public void PlayDamaged()
	{
		this.Visual?.PlayDamaged();
		this.damageOverlay?.Play();
	}

	public void PlayGetItem()
	{
		this.Visual?.PlayGetItem();
	}
}
