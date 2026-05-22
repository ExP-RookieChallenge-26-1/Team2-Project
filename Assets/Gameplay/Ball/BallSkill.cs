using UnityEngine;

public abstract class BallSkill : MonoBehaviour
{
	protected Ball ball;
	[SerializeField] protected SkillEventChannel skillEventChannel;
	[field: SerializeField] public float Cooldown { get; private set; }
	[field: SerializeField] public float Duration { get; private set; }

	public float CooldownReductionRate { get; private set; }
	public bool IsActive { get; private set; }
	public bool IsReady => !this.IsActive && this.timer <= 0f;
	[SerializeField] private float timer;

	protected virtual void Awake()
	{
		this.ball = GetComponent<Ball>();
	}

	protected virtual void Start()
	{
		if (this.skillEventChannel == null)
			return;
		
		Subscribe();
	}

	protected virtual void OnDestroy()
	{
		if (this.skillEventChannel == null)
			return;
		Unsubscribe();
	}

	public void Tick()
	{
		if (Time.deltaTime <= 0f)
			return;
		
		if (this.timer > 0f)
		{
			this.timer -= Time.deltaTime;

			if (this.timer <= 0f && this.IsActive)
			{
				this.IsActive = false;
				OnDeactivate();
				this.timer = this.Cooldown * (1f - this.CooldownReductionRate);
			}
		}
	}

	public void TryActivate()
	{
		if (!this.IsReady)
		{
			Debug.Log($"{GetType().Name} Cooldown Remain: {this.timer}");
			return;
		}

		this.IsActive = true;

		if (this.Duration <= 0f)
		{
			this.IsActive = false;
			OnDeactivate();
			this.timer = this.Cooldown * (1f - this.CooldownReductionRate);
		}
		else
			this.timer = this.Duration;
			
		OnActivate();
	}

	public void ApplyCooldownReduction(float rate)
	{
		this.CooldownReductionRate = Mathf.Clamp01(this.CooldownReductionRate + rate);
	}

	protected abstract void OnActivate();
	protected abstract void OnDeactivate();

	protected abstract void Subscribe();
	protected abstract void Unsubscribe();
}