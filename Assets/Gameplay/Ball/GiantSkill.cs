using UnityEngine;

public class GiantSkill : BallSkill
{
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
		this.ball.Stats.SetRadiusMultiplier(this.ball.Stats.GiantSize);
	}

	protected override void OnDeactivate()
	{
		this.ball.Stats.ResetRadiusMultiplier();
	}
}
