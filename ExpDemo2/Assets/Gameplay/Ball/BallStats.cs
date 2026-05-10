using UnityEngine;

[CreateAssetMenu(fileName = "BallStats", menuName = "Stats/BallStats")]
public class BallStats : ScriptableObject
{
	[field: SerializeField] public float Speed { get; private set; }
	[field: SerializeField] public float Radius { get; private set; }
	[field: SerializeField] public float AttackPower { get; private set; }
	[field: SerializeField] public float CriticalChance { get; private set; }
	[field: SerializeField] public float CriticalDamage { get; private set; }
}