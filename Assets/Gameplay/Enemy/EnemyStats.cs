using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Stats/EnemyStats")]
public class EnemyStats : ScriptableObject
{
	[field: SerializeField] public int MaxHp { get; private set; }
	[field: SerializeField] public int ExpReward { get; private set; }
	[field: SerializeField] public float MoveSpeed { get; private set; }
	[field: SerializeField] public float StateChangeRate { get; private set; }
	[field: SerializeField] public float IdleDecisionInterval { get; private set; }
	[field: SerializeField] public float MoveDecisionInterval { get; private set; }
	[field: SerializeField] public float TrackYThreshold { get; private set; }
	[field: SerializeField] public float TrackSpeed { get; private set; }
	[field: SerializeField] public int AttackDamage { get; private set; }
}
