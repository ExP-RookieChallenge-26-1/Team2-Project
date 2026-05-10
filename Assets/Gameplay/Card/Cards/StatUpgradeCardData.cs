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
	[field: SerializeField] public float Skill1CooldownReductionRate { get; private set; }
	[field: SerializeField] public float Skill2CooldownReductionRate { get; private set; }

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
		
		if (this.Skill1CooldownReductionRate != 0)
			; // TODO
		if (this.Skill2CooldownReductionRate != 0)
			; // TODO
	}
}
