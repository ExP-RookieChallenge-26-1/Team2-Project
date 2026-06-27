using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeCard", menuName = "CardData/StatUpgrade")]
public class StatUpgradeCardData : CardData
{
	[field: Header("Badge"), SerializeField] public BuffBadgeData BadgeData { get; private set; }
	[field: SerializeField] public float BadgeDuration { get; private set; }

	[field: Header("Upgrade Value"), SerializeField] public float AttackPower { get; private set; }
	[field: SerializeField] public float CriticalChance { get; private set; }
	[field: SerializeField] public float CriticalDamage { get; private set; }
	[field: SerializeField] public float Speed { get; private set; }
	[field: SerializeField] public float BallSize { get; private set; }
	[field: SerializeField] public int PaddleSizeLevel { get; private set; }
	[field: SerializeField] public float Skill1ManualCooldownReduction { get; private set; }
	[field: SerializeField] public float Skill1AutoCooldownReduction { get; private set; }
	[field: SerializeField] public float Skill2ManualCooldownReduction { get; private set; }
	[field: SerializeField] public float Skill2AutoCooldownReduction { get; private set; }

	[field: Header("Skill Unlock"), SerializeField] public bool UnlockSkill1ManualTrigger { get; private set; }
	[field: SerializeField] public bool UnlockSkill1AutoTrigger { get; private set; }
	[field: SerializeField] public bool UnlockSkill2ManualTrigger { get; private set; }
	[field: SerializeField] public bool UnlockSkill2AutoTrigger { get; private set; }
	[field: SerializeField] public float SkillUnlockFallbackCooldownReduction { get; private set; } = 1f;

	[field: Header("Skill Activation"), SerializeField] public bool ActivateSkill1Immediately { get; private set; }
	[field: SerializeField] public bool ActivateSkill2Immediately { get; private set; }

	[field: Header("Skill Level"), SerializeField] public int Skill1SizeLevelIncrease { get; private set; }
	[field: SerializeField] public float Skill1SizeIncrease { get; private set; }
	[field: SerializeField] public float Skill1DurationIncrease { get; private set; }
	[field: SerializeField] public int Skill2CloneLevelIncrease { get; private set; }

	public override void Apply()
	{
		Apply(CardUseContext.None);
	}

	public override void Apply(CardUseContext context)
	{
		BallStats ballStats = GameManager.Instance.BallStats;
		PaddleStats paddleStats = GameManager.Instance.PaddleStats;
		float criticalChance = GetCriticalChance(context);
		SkillBadgeSnapshot beforeSkillStats = SkillBadgeSnapshot.From(ballStats);

		if (this.AttackPower != 0)
			ballStats.IncreaseAttackPower(this.AttackPower);
		if (criticalChance != 0)
			ballStats.IncreaseCriticalChance(criticalChance);
		if (this.CriticalDamage != 0)
			ballStats.IncreaseCriticalDamage(this.CriticalDamage);
		if (this.Speed != 0)
			ballStats.IncreaseSpeed(this.Speed);
		if (this.BallSize != 0)
			ballStats.IncreaseRadius(this.BallSize);
		
		if (this.PaddleSizeLevel != 0)
			paddleStats.IncreasePaddleSizeLevel(this.PaddleSizeLevel);

		ApplySkillUpgrades(ballStats);
		bool skillStatsChanged = !beforeSkillStats.Equals(SkillBadgeSnapshot.From(ballStats));

		ApplySkillStatsToActiveBalls(ballStats);
		ActivateImmediateSkills();

		AttachUpgradeBadge(context, skillStatsChanged);
	}

	public override string GetCardName(CardUseContext context)
	{
		if (CardIds.IsCriticalChance(Id))
			return $"치명타 확률 +{GetCriticalChancePercent(context)}%";

		string replacementName = GetSkillUnlockReplacementName();
		if (!string.IsNullOrEmpty(replacementName))
			return replacementName;

		return base.GetCardName(context);
	}

	public override string GetDescription(CardUseContext context)
	{
		if (CardIds.IsCriticalChance(Id))
			return $"치명타 확률이 {GetCriticalChancePercent(context)}% 증가합니다.";

		string replacementDescription = GetSkillUnlockReplacementDescription();
		if (!string.IsNullOrEmpty(replacementDescription))
			return replacementDescription;

		return base.GetDescription(context);
	}

	public static float CalculateCriticalChanceBonus(int progressionIndex)
	{
		return CardIds.GetCriticalChanceBonus(CardIds.GetCriticalChanceIdForIndex(progressionIndex));
	}

	private float GetCriticalChance(CardUseContext context)
	{
		float chanceFromId = CardIds.GetCriticalChanceBonus(Id);
		if (chanceFromId > 0f)
			return chanceFromId;

		return this.CriticalChance;
	}

	private int GetCriticalChancePercent(CardUseContext context)
	{
		return Mathf.RoundToInt(GetCriticalChance(context) * 100f);
	}

	private void ApplySkillUpgrades(BallStats ballStats)
	{
		ApplySkillUnlocks(ballStats);

		if (this.Skill1ManualCooldownReduction != 0)
			ballStats.ReduceSkill1ManualCooldown(this.Skill1ManualCooldownReduction);
		if (this.Skill1AutoCooldownReduction != 0)
			ballStats.ReduceSkill1AutoCooldown(this.Skill1AutoCooldownReduction);
		if (this.Skill2ManualCooldownReduction != 0)
			ballStats.ReduceSkill2ManualCooldown(this.Skill2ManualCooldownReduction);
		if (this.Skill2AutoCooldownReduction != 0)
			ballStats.ReduceSkill2AutoCooldown(this.Skill2AutoCooldownReduction);

		if (this.Skill1SizeLevelIncrease != 0)
			ballStats.IncreaseSkill1SizeLevel(this.Skill1SizeLevelIncrease);
		if (this.Skill1SizeIncrease != 0f)
			ballStats.IncreaseGiantSize(this.Skill1SizeIncrease);
		if (this.Skill1DurationIncrease != 0f)
			ballStats.IncreaseSkill1Duration(this.Skill1DurationIncrease);
		if (this.Skill2CloneLevelIncrease != 0)
			ballStats.IncreaseSkill2CloneLevel(this.Skill2CloneLevelIncrease);
	}

	private void ApplySkillUnlocks(BallStats ballStats)
	{
		if (this.UnlockSkill1ManualTrigger)
			UnlockOrReduceManualCooldown(ballStats.Skill1HasManualTrigger, ballStats.UnlockSkill1ManualTrigger, ballStats.ReduceSkill1ManualCooldown);
		if (this.UnlockSkill1AutoTrigger)
			UnlockOrReduceAutoCooldown(ballStats.Skill1HasAutoTrigger, ballStats.UnlockSkill1AutoTrigger, ballStats.ReduceSkill1AutoCooldown);
		if (this.UnlockSkill2ManualTrigger)
			UnlockOrReduceManualCooldown(ballStats.Skill2HasManualTrigger, ballStats.UnlockSkill2ManualTrigger, ballStats.ReduceSkill2ManualCooldown);
		if (this.UnlockSkill2AutoTrigger)
			UnlockOrReduceAutoCooldown(ballStats.Skill2HasAutoTrigger, ballStats.UnlockSkill2AutoTrigger, ballStats.ReduceSkill2AutoCooldown);
	}

	private void UnlockOrReduceManualCooldown(bool isUnlocked, System.Action unlock, System.Action<float> reduceCooldown)
	{
		if (!isUnlocked)
		{
			unlock();
			return;
		}

		reduceCooldown(GetFallbackCooldownReduction());
	}

	private void UnlockOrReduceAutoCooldown(bool isUnlocked, System.Action unlock, System.Action<float> reduceCooldown)
	{
		if (!isUnlocked)
		{
			unlock();
			return;
		}

		reduceCooldown(GetFallbackCooldownReduction());
	}

	private float GetFallbackCooldownReduction()
	{
		return this.SkillUnlockFallbackCooldownReduction > 0f ? this.SkillUnlockFallbackCooldownReduction : 1f;
	}

	private string GetSkillUnlockReplacementName()
	{
		BallStats ballStats = GameManager.Instance != null ? GameManager.Instance.BallStats : null;
		if (ballStats == null)
			return null;

		if (this.UnlockSkill1ManualTrigger && ballStats.Skill1HasManualTrigger)
			return $"거대화 쿨타임 -{GetFallbackCooldownReduction():0.#}초";
		if (this.UnlockSkill1AutoTrigger && ballStats.Skill1HasAutoTrigger)
			return $"자동 거대화 쿨타임 -{GetFallbackCooldownReduction():0.#}초";
		if (this.UnlockSkill2ManualTrigger && ballStats.Skill2HasManualTrigger)
			return $"분신술 쿨타임 -{GetFallbackCooldownReduction():0.#}초";
		if (this.UnlockSkill2AutoTrigger && ballStats.Skill2HasAutoTrigger)
			return $"자동 분신술 쿨타임 -{GetFallbackCooldownReduction():0.#}초";

		return null;
	}

	private string GetSkillUnlockReplacementDescription()
	{
		BallStats ballStats = GameManager.Instance != null ? GameManager.Instance.BallStats : null;
		if (ballStats == null)
			return null;

		if (this.UnlockSkill1ManualTrigger && ballStats.Skill1HasManualTrigger)
			return $"거대화의 쿨타임이 {GetFallbackCooldownReduction():0.#}초 감소합니다.";
		if (this.UnlockSkill1AutoTrigger && ballStats.Skill1HasAutoTrigger)
			return $"자동 시전되는 거대화의 쿨타임이 {GetFallbackCooldownReduction():0.#}초 감소합니다.";
		if (this.UnlockSkill2ManualTrigger && ballStats.Skill2HasManualTrigger)
			return $"분신술의 쿨타임이 {GetFallbackCooldownReduction():0.#}초 감소합니다.";
		if (this.UnlockSkill2AutoTrigger && ballStats.Skill2HasAutoTrigger)
			return $"자동 시전되는 분신술의 쿨타임이 {GetFallbackCooldownReduction():0.#}초 감소합니다.";

		return null;
	}

	private static void ApplySkillStatsToActiveBalls(BallStats ballStats)
	{
		foreach (Ball ball in Object.FindObjectsByType<Ball>(FindObjectsSortMode.None))
		{
			ball.SetSkillCooldowns(
				ballStats.Skill1ManualCooldown,
				ballStats.Skill1AutoCooldown,
				ballStats.Skill2ManualCooldown,
				ballStats.Skill2AutoCooldown);
			ball.SetSkillTriggerSettings(
				ballStats.Skill1HasManualTrigger,
				ballStats.Skill1HasAutoTrigger,
				ballStats.Skill2HasManualTrigger,
				ballStats.Skill2HasAutoTrigger);

			if (ball.GiantSkill != null)
			{
				ball.GiantSkill.SetSizeLevel(ballStats.Skill1SizeLevel);
				ball.GiantSkill.SetDuration(ballStats.Skill1Duration);
			}
			if (ball.CloneSkill != null)
			{
				ball.CloneSkill.SetCloneLevel(ballStats.Skill2CloneLevel);
				ball.CloneSkill.SetDuration(ballStats.Skill2Duration);
			}
		}
	}

	private void ActivateImmediateSkills()
	{
		if (!this.ActivateSkill1Immediately && !this.ActivateSkill2Immediately)
			return;

		foreach (Ball ball in Object.FindObjectsByType<Ball>(FindObjectsSortMode.None))
		{
			if (this.ActivateSkill1Immediately && ball.GiantSkill != null)
				ball.GiantSkill.ActivateImmediately();
			if (this.ActivateSkill2Immediately && ball.CloneSkill != null)
				ball.CloneSkill.ActivateImmediately();
		}
	}

	private void AttachUpgradeBadge(CardUseContext context, bool skillStatsChanged)
	{
		if (BadgeData == null)
			return;

		if (HasNonSkillStatUpgrade(context) || skillStatsChanged)
			BuffBadgeManager.Instance?.Attach(BadgeData, BadgeDuration);
	}

	private bool HasNonSkillStatUpgrade(CardUseContext context)
	{
		return this.AttackPower != 0
			|| GetCriticalChance(context) != 0
			|| this.CriticalDamage != 0
			|| this.Speed != 0
			|| this.BallSize != 0
			|| this.PaddleSizeLevel != 0;
	}

	private readonly struct SkillBadgeSnapshot
	{
		private readonly bool skill1HasManualTrigger;
		private readonly bool skill1HasAutoTrigger;
		private readonly bool skill2HasManualTrigger;
		private readonly bool skill2HasAutoTrigger;
		private readonly float skill1ManualCooldown;
		private readonly float skill1AutoCooldown;
		private readonly float skill2ManualCooldown;
		private readonly float skill2AutoCooldown;
		private readonly float skill1Duration;
		private readonly float skill2Duration;
		private readonly float giantSize;
		private readonly int skill1SizeLevel;
		private readonly int skill2CloneLevel;

		private SkillBadgeSnapshot(BallStats stats)
		{
			this.skill1HasManualTrigger = stats.Skill1HasManualTrigger;
			this.skill1HasAutoTrigger = stats.Skill1HasAutoTrigger;
			this.skill2HasManualTrigger = stats.Skill2HasManualTrigger;
			this.skill2HasAutoTrigger = stats.Skill2HasAutoTrigger;
			this.skill1ManualCooldown = stats.Skill1ManualCooldown;
			this.skill1AutoCooldown = stats.Skill1AutoCooldown;
			this.skill2ManualCooldown = stats.Skill2ManualCooldown;
			this.skill2AutoCooldown = stats.Skill2AutoCooldown;
			this.skill1Duration = stats.Skill1Duration;
			this.skill2Duration = stats.Skill2Duration;
			this.giantSize = stats.GiantSize;
			this.skill1SizeLevel = stats.Skill1SizeLevel;
			this.skill2CloneLevel = stats.Skill2CloneLevel;
		}

		public static SkillBadgeSnapshot From(BallStats stats)
		{
			return new SkillBadgeSnapshot(stats);
		}

		public bool Equals(SkillBadgeSnapshot other)
		{
			return this.skill1HasManualTrigger == other.skill1HasManualTrigger
				&& this.skill1HasAutoTrigger == other.skill1HasAutoTrigger
				&& this.skill2HasManualTrigger == other.skill2HasManualTrigger
				&& this.skill2HasAutoTrigger == other.skill2HasAutoTrigger
				&& Mathf.Approximately(this.skill1ManualCooldown, other.skill1ManualCooldown)
				&& Mathf.Approximately(this.skill1AutoCooldown, other.skill1AutoCooldown)
				&& Mathf.Approximately(this.skill2ManualCooldown, other.skill2ManualCooldown)
				&& Mathf.Approximately(this.skill2AutoCooldown, other.skill2AutoCooldown)
				&& Mathf.Approximately(this.skill1Duration, other.skill1Duration)
				&& Mathf.Approximately(this.skill2Duration, other.skill2Duration)
				&& Mathf.Approximately(this.giantSize, other.giantSize)
				&& this.skill1SizeLevel == other.skill1SizeLevel
				&& this.skill2CloneLevel == other.skill2CloneLevel;
		}
	}
}
