using UnityEngine;

[CreateAssetMenu(fileName = "StatBoostItem", menuName = "ItemData/StatBoost")]
public class StatBoostItemData : ItemData
{
    [field: Header("Boost Value"), SerializeField] public float AttackPower { get; private set; }
    [field: SerializeField] public float CriticalChance { get; private set; }
    [field: SerializeField] public float CriticalDamage { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float BallSize { get; private set; }
    [field: SerializeField] public int PaddleSizeLevel { get; private set; }

    public override void Apply()
    {
        BallStats ballStats = GameManager.Instance.BallStats;
        PaddleStats paddleStats = GameManager.Instance.PaddleStats;

        if (this.AttackPower != 0f)
            ballStats.IncreaseAttackPower(this.AttackPower);
        if (this.CriticalChance != 0f)
            ballStats.IncreaseCriticalChance(this.CriticalChance);
        if (this.CriticalDamage != 0f)
            ballStats.IncreaseCriticalDamage(this.CriticalDamage);
        if (this.Speed != 0f)
            ballStats.IncreaseSpeed(this.Speed);
        if (this.BallSize != 0f)
            ballStats.IncreaseRadius(this.BallSize);
        if (this.PaddleSizeLevel != 0)
            paddleStats.IncreasePaddleSizeLevel(this.PaddleSizeLevel);
    }
}