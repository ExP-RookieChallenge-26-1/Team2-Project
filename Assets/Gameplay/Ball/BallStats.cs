using UnityEngine;

[CreateAssetMenu(fileName = "BallStats", menuName = "Stats/BallStats")]
public class BallStats : ScriptableObject
{
	[field: SerializeField] public float Speed { get; private set; }
	[field: SerializeField] public float BaseRadius { get; private set; }
	[field: SerializeField] public float AttackPower { get; private set; }
	[field: SerializeField] public float CriticalChance { get; private set; }
	[field: SerializeField] public float CriticalDamage { get; private set; }

	public float Radius => this.BaseRadius * this.RadiusMultiplier;
	public float RadiusMultiplier { get; private set; } = 1f;

	public void IncreaseSpeed(float amount)
	{
		this.Speed = Mathf.Max(0.1f, this.Speed + amount);
		Debug.Log($"Speed: {this.Speed}");
	}

	public void IncreaseRadius(float amount)
	{
		this.BaseRadius = Mathf.Max(0.01f, this.BaseRadius + amount);
		Debug.Log($"BaseRadius: {this.BaseRadius}");
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

	public void SetRadiusMultiplier(float multiplier)
	{
		this.RadiusMultiplier = Mathf.Max(0.01f, multiplier);
	}

	public void ResetRadiusMultiplier()
	{
		this.RadiusMultiplier = 1f;
	}
}