using UnityEngine;

public abstract class BallSkill : MonoBehaviour
{
	protected Ball ball;
	[SerializeField] protected SkillEventChannel skillEventChannel;
	[field: SerializeField] public float Duration { get; private set; }
	[SerializeField] private float durationTimer;

	[field: SerializeField] public bool HasManualTrigger { get; private set; }
	[field: SerializeField] public float ManualCooldown { get; private set; }
	[SerializeField] private float manualTimer;

	[field: SerializeField] public bool HasAutoTrigger { get; private set; }
	[field: SerializeField] public float AutoCooldown { get; private set; }
	[SerializeField] private float autoTimer;

	public float CooldownReductionRate { get; private set; }
	public bool IsActive { get; private set; }
	public bool IsManualReady => !this.IsActive && this.manualTimer <= 0f;

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

		cooldown = this.AutoCooldown * (1f - this.CooldownReductionRate);
		
		if (cooldown <= 0f)
			return;

		this.autoTimer -= Time.deltaTime;

		if (this.autoTimer <= 0f)
		{
			this.autoTimer = cooldown;
			Activate();
		}
	}

	public void ApplyCooldownReduction(float rate)
	{
		float before;
		float delta;

		before = this.CooldownReductionRate;
		this.CooldownReductionRate = Mathf.Clamp01(this.CooldownReductionRate + rate);
		delta = this.CooldownReductionRate - before;

		this.manualTimer = Mathf.Max(0f, this.manualTimer - this.ManualCooldown * delta);
		this.autoTimer = Mathf.Max(0f, this.autoTimer - this.AutoCooldown * delta);
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
	public void TryManualActivate()
	{
		if (!this.HasManualTrigger)
			return;
		
		if (!this.IsManualReady)
			return;
		
		this.manualTimer = this.ManualCooldown * (1f - this.CooldownReductionRate);
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