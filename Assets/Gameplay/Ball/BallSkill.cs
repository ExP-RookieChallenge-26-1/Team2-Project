using UnityEngine;

public abstract class BallSkill : MonoBehaviour
{
	protected Ball ball;
	[SerializeField] protected SkillEventChannel skillEventChannel;
	[field: SerializeField] public float Duration { get; private set; }
	[SerializeField] private float durationTimer;

	[SerializeField] private BuffBadgeData badgeData;
	[SerializeField] private float badgeDuration = 0f;
	private static int lastBadgeFrame = -1;

	public bool HasManualTrigger { get; private set; }
	[field: SerializeField] public float ManualCooldown { get; private set; }
	[SerializeField] private float manualTimer;

	public bool HasAutoTrigger { get; private set; }
	[field: SerializeField] public float AutoCooldown { get; private set; }
	[SerializeField] private float autoTimer;

	public bool IsActive { get; private set; }
	public bool IsManualReady => !this.IsActive && this.manualTimer <= 0f;

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
		if (this.skillEventChannel == null)
			return;
		
		if (this.HasManualTrigger)
			Subscribe();
	}

	protected virtual void OnDestroy()
	{
		if (this.skillEventChannel == null)
			return;

		if (this.HasManualTrigger)	
			Unsubscribe();
	}

	public void Tick()
	{
		if (Time.deltaTime <= 0f)
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
		if (this.IsActive)
			return;

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
			this.autoTimer = cooldown;
			Activate();
		}
	}

	public void SetManualCooldown(float cooldown)
	{
		this.ManualCooldown = Mathf.Max(0f, cooldown);
		this.manualTimer = Mathf.Min(this.manualTimer, this.ManualCooldown);
	}

	public void SetAutoCooldown(float cooldown)
	{
		this.AutoCooldown = Mathf.Max(0f, cooldown);
		this.autoTimer = Mathf.Min(this.autoTimer, this.AutoCooldown);
	}

	public void SetTriggerSettings(bool hasManualTrigger, bool hasAutoTrigger)
	{
		this.HasManualTrigger = hasManualTrigger;
		this.HasAutoTrigger = hasAutoTrigger;
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

		if (badgeData != null && lastBadgeFrame != Time.frameCount)
		{
			lastBadgeFrame = Time.frameCount;
			float attachDuration = badgeDuration > 0f ? badgeDuration : this.Duration;
			BuffBadgeManager.Instance?.Attach(badgeData, attachDuration);
		}

		OnActivate();
	}
	public void TryManualActivate()
	{
		if (!this.HasManualTrigger)
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

	protected abstract void OnActivate();
	protected abstract void OnDeactivate();
}
