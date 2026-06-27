using UnityEngine;

public abstract class BallSkill : MonoBehaviour
{
	protected Ball ball;
	[SerializeField] protected SkillEventChannel skillEventChannel;
	[field: SerializeField] public float Duration { get; private set; }
	[SerializeField] private float durationTimer;

	public bool HasManualTrigger { get; private set; }
	[field: SerializeField] public float ManualCooldown { get; private set; }
	[SerializeField] private float manualTimer;
	private bool manualCooldownInitialized;

	public bool HasAutoTrigger { get; private set; }
	[field: SerializeField] public float AutoCooldown { get; private set; }
	[SerializeField] private float autoTimer;
	private bool isSubscribed;

	public bool IsActive { get; private set; }
	private bool IsRespawning => this.ball != null && this.ball.IsRespawning;
	public bool IsManualReady => !this.IsRespawning && !this.IsActive && this.manualTimer <= 0f;

	public float AutoCooldownRemaining => Mathf.Max(0f, this.autoTimer);
	public float AutoCooldownRatio => this.AutoCooldown > 0f
		? Mathf.Clamp01(this.autoTimer / this.AutoCooldown)
		: 0f;

	public float ManualCooldownRemaining => Mathf.Max(0f, this.manualTimer);
	public float ManualCooldownRatio => this.ManualCooldown > 0f
		? Mathf.Clamp01(this.manualTimer / this.ManualCooldown)
		: 0f;

	protected virtual void Awake()
	{
		this.ball = GetComponent<Ball>();
	}

	protected virtual void Start()
	{
		SyncSubscription();
	}

	protected virtual void OnDestroy()
	{
		if (this.isSubscribed)
		{
			Unsubscribe();
			this.isSubscribed = false;
		}
	}

	public void Tick()
	{
		if (Time.deltaTime <= 0f)
			return;

		if (this.IsRespawning)
			return;

		TickDuration();

		if (this.HasManualTrigger)
			TickManual();

		if (this.HasAutoTrigger)
			TickAuto();
	}

	private void TickDuration()
	{
		if (!this.IsActive)
			return;

		this.durationTimer -= Time.deltaTime;

		if (this.durationTimer <= 0f)
		{
			this.IsActive = false;
			OnDeactivate();
		}
	}

	private void TickManual()
	{
		if (this.manualTimer <= 0f)
			return;

		this.manualTimer -= Time.deltaTime;
	}

	private void TickAuto()
	{
		float cooldown;

		cooldown = this.AutoCooldown;
		
		if (cooldown <= 0f)
			return;

		this.autoTimer -= Time.deltaTime;

		if (this.autoTimer <= 0f)
		{
			this.autoTimer = Mathf.Max(0f, cooldown + this.autoTimer);
			Activate();
		}
	}

	public void SetManualCooldown(float cooldown)
	{
		this.ManualCooldown = Mathf.Max(0f, cooldown);

		if (!this.manualCooldownInitialized)
		{
			this.manualTimer = 0f;
			this.manualCooldownInitialized = true;
			return;
		}

		this.manualTimer = Mathf.Min(this.manualTimer, this.ManualCooldown);
	}

	public void SetAutoCooldown(float cooldown)
	{
		this.AutoCooldown = Mathf.Max(0f, cooldown);
		this.autoTimer = Mathf.Min(this.autoTimer, this.AutoCooldown);
	}

	public void SetTriggerSettings(bool hasManualTrigger, bool hasAutoTrigger)
	{
		bool manualTriggerWasAcquired = !this.HasManualTrigger && hasManualTrigger;
		bool autoTriggerWasAcquired = !this.HasAutoTrigger && hasAutoTrigger;

		this.HasManualTrigger = hasManualTrigger;
		this.HasAutoTrigger = hasAutoTrigger;

		if (manualTriggerWasAcquired && this.ManualCooldown > 0f && this.manualTimer <= 0f)
			this.manualTimer = this.ManualCooldown;

		if (autoTriggerWasAcquired && this.AutoCooldown > 0f && this.autoTimer <= 0f)
			this.autoTimer = this.AutoCooldown;

		SyncSubscription();
	}

	public void SetDuration(float duration)
	{
		this.Duration = Mathf.Max(0f, duration);
		this.durationTimer = Mathf.Min(this.durationTimer, this.Duration);
	}

	private void Activate()
	{
		if (this.Duration > 0f)
		{
			this.IsActive = true;
			this.durationTimer = this.Duration;
		}

		OnActivate();
	}

	public void ActivateImmediately()
	{
		if (this.IsRespawning)
			return;

		Activate();
	}

	public void TryManualActivate()
	{
		if (!this.HasManualTrigger)
			return;

		if (this.IsRespawning)
			return;

		if (!this.IsManualReady)
			return;

		this.manualTimer = this.ManualCooldown;
		Activate();
	}

	protected virtual void Subscribe()
	{
	}
	protected virtual void Unsubscribe()
	{
	}

	private void SyncSubscription()
	{
		if (this.skillEventChannel == null)
			return;

		if (this.HasManualTrigger && !this.isSubscribed)
		{
			Subscribe();
			this.isSubscribed = true;
		}
		else if (!this.HasManualTrigger && this.isSubscribed)
		{
			Unsubscribe();
			this.isSubscribed = false;
		}
	}

	protected abstract void OnActivate();
	protected abstract void OnDeactivate();
}
