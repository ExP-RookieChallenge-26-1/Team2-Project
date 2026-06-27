using System.Collections;
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
        this.ball.Animation.TriggerUpsizing();
		StartCoroutine(UpsizingReadyRoutine());
	}

	private IEnumerator UpsizingReadyRoutine()
	{
		float frameTime = 1f / 12f;
		float target = GetCurrentMultiplier();

		this.ball.Stats.SetRadiusMultiplier(1f);
		yield return new WaitForSeconds(frameTime);
		this.ball.Stats.SetRadiusMultiplier((1f + target) / 2f);
		yield return new WaitForSeconds(frameTime);
		this.ball.Stats.SetRadiusMultiplier(target);
	}

	protected override void OnDeactivate()
	{
		this.ball.Stats.ResetRadiusMultiplier();
		this.ball.Animation.TriggerDownsizing();
	}

	public void IncreaseSizeLevel(int amount)
	{
		SetSizeLevel(this.SizeLevel + amount);
	}

	public void SetSizeLevel(int level)
	{
		this.SizeLevel = System.Math.Clamp(level, 0, this.MaxSizeLevel);
	}

	public float GetCurrentMultiplier()
	{
		if (this.ball != null && this.ball.Stats != null)
			return Mathf.Max(1f, this.ball.Stats.GiantSize);

		if (this.RadiusMultiplierPerLevel == null || this.RadiusMultiplierPerLevel.Length == 0)
			return 1f;
		
		return this.RadiusMultiplierPerLevel[System.Math.Clamp(this.SizeLevel, 0, this.RadiusMultiplierPerLevel.Length - 1)];
	}
}
