using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeCard", menuName = "CardData/StatUpgrade")]
public class StatUpgradeCardData : CardData
{
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

	public override void Apply()
	{
		BallStats ballStats = GameManager.Instance.BallStats;
		PaddleStats paddleStats = GameManager.Instance.PaddleStats;

		if (this.AttackPower != 0)
			ballStats.IncreaseAttackPower(this.AttackPower);
		if (this.CriticalChance != 0)
			ballStats.IncreaseCriticalChance(this.CriticalChance);
		if (this.CriticalDamage != 0)
			ballStats.IncreaseCriticalDamage(this.CriticalDamage);
		if (this.Speed != 0)
			ballStats.IncreaseSpeed(this.Speed);
		if (this.BallSize != 0)
			ballStats.IncreaseRadius(this.BallSize);
		
		if (this.PaddleSizeLevel != 0)
			paddleStats.IncreasePaddleSizeLevel(this.PaddleSizeLevel);

		if (this.Skill1ManualCooldownReduction != 0)
			ballStats.ReduceSkill1ManualCooldown(this.Skill1ManualCooldownReduction);
		if (this.Skill1AutoCooldownReduction != 0)
			ballStats.ReduceSkill1AutoCooldown(this.Skill1AutoCooldownReduction);
		if (this.Skill2ManualCooldownReduction != 0)
			ballStats.ReduceSkill2ManualCooldown(this.Skill2ManualCooldownReduction);
		if (this.Skill2AutoCooldownReduction != 0)
			ballStats.ReduceSkill2AutoCooldown(this.Skill2AutoCooldownReduction);

		ApplySkillCooldownsToActiveBalls(ballStats);
	}

	private static void ApplySkillCooldownsToActiveBalls(BallStats ballStats)
	{
		foreach (Ball ball in Object.FindObjectsByType<Ball>(FindObjectsSortMode.None))
		{
			ball.SetSkillCooldowns(
				ballStats.Skill1ManualCooldown,
				ballStats.Skill1AutoCooldown,
				ballStats.Skill2ManualCooldown,
				ballStats.Skill2AutoCooldown);
		}
	}
}
