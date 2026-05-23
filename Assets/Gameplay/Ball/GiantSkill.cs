using UnityEngine;

public class GiantSkill : BallSkill
{
	[field: SerializeField] public float[] RadiusMultiplierPerLevel { get; private set; }
	[field: SerializeField] public int SizeLevel { get; private set; }
	public int MaxSizeLevel => this.RadiusMultiplierPerLevel.Length - 1;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Subscribe()
	{
		this.skillEventChannel.OnSkill1Activated += TryManualActivate;
	}

	protected override void Unsubscribe()
	{
		this.skillEventChannel.OnSkill1Activated -= TryManualActivate;
	}

	protected override void OnActivate()
	{
		this.ball.Stats.SetRadiusMultiplier(GetCurrentMultiplier());
	}

	protected override void OnDeactivate()
	{
		this.ball.Stats.ResetRadiusMultiplier();
	}

	public void IncreaseSizeLevel(int amount)
	{
		this.SizeLevel = System.Math.Clamp(this.SizeLevel + amount, 0, this.MaxSizeLevel);
	}

	public float GetCurrentMultiplier()
	{
		if (this.RadiusMultiplierPerLevel == null || this.RadiusMultiplierPerLevel.Length == 0)
			return 1f;
		
		return this.RadiusMultiplierPerLevel[System.Math.Clamp(this.SizeLevel, 0, this.RadiusMultiplierPerLevel.Length - 1)];
	}
}