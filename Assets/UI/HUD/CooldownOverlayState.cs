using UnityEngine;

public enum CooldownOverlayMode
{
	Auto,
	Manual
}

public readonly struct CooldownOverlayState
{
	public CooldownOverlayState(bool isAcquired, float remainingSeconds, float cooldownRatio)
	{
		IsAcquired = isAcquired;
		RemainingSeconds = Mathf.Max(0f, remainingSeconds);
		CooldownRatio = Mathf.Clamp01(cooldownRatio);
	}

	public bool IsAcquired { get; }
	public float RemainingSeconds { get; }
	public float CooldownRatio { get; }
	public bool IsCoolingDown => IsAcquired && RemainingSeconds > 0f && CooldownRatio > 0f;

	public static CooldownOverlayState FromSkill(BallSkill skill, CooldownOverlayMode mode)
	{
		if (skill == null)
			return new CooldownOverlayState(false, 0f, 0f);

		if (mode == CooldownOverlayMode.Auto)
		{
			return new CooldownOverlayState(
				skill.HasAutoTrigger,
				skill.AutoCooldownRemaining,
				skill.AutoCooldownRatio);
		}

		return new CooldownOverlayState(
			skill.HasManualTrigger,
			skill.ManualCooldownRemaining,
			skill.ManualCooldownRatio);
	}
}
