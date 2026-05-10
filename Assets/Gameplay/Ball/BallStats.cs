using UnityEngine;

[CreateAssetMenu(fileName = "BallStats", menuName = "Stats/BallStats")]
public class BallStats : ScriptableObject
{
	[field: SerializeField] public float Speed { get; private set; }
	[field: SerializeField] public float Radius { get; private set; }
	[field: SerializeField] public float AttackPower { get; private set; }
	[field: SerializeField] public float CriticalChance { get; private set; }
	[field: SerializeField] public float CriticalDamage { get; private set; }

	public void IncreaseSpeed(float amount)
	{
		this.Speed = Mathf.Max(0.1f, this.Speed + amount);
		Debug.Log($"Speed: {this.Speed}");
	}

	public void IncreaseRadius(float amount)
	{
		this.Radius = Mathf.Max(0.01f, this.Radius + amount);
		Debug.Log($"Radius: {this.Radius}");
	}

	public void IncreaseAttackPower(float amount)
	{
		this.AttackPower = Mathf.Max(0f, this.AttackPower + amount);
		Debug.Log($"AttackPower: {this.AttackPower}");
	}

	public void IncreaseCriticalChance(float amount)
	{
		this.CriticalChance = Mathf.Clamp(this.CriticalChance + amount, 0f, 1f);
		Debug.Log($"Critical Chance: {this.CriticalChance}");
	}

	public void IncreaseCriticalDamage(float amount)
	{
		this.CriticalDamage = Mathf.Max(1f, this.CriticalDamage + amount);
		Debug.Log($"Critical Damage: {this.CriticalDamage}");
	}
}