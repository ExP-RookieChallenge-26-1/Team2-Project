using UnityEngine;

[CreateAssetMenu(fileName = "BallStats", menuName = "Stats/BallStats")]
public class BallStats : ScriptableObject
{
	[field: SerializeField] public float Speed { get; private set; }
	[field: SerializeField] public float BaseRadius { get; private set; }
	[field: SerializeField] public float GiantSize { get; private set; }
	[field: SerializeField] public float AttackPower { get; private set; }
	[field: SerializeField] public float CriticalChance { get; private set; }
	[field: SerializeField] public float CriticalDamage { get; private set; }
	[field: SerializeField] public bool Skill1HasManualTrigger { get; private set; }
	[field: SerializeField] public bool Skill1HasAutoTrigger { get; private set; }
	[field: SerializeField] public float Skill1ManualCooldown { get; private set; }
	[field: SerializeField] public float Skill1AutoCooldown { get; private set; }
	[field: SerializeField] public bool Skill2HasManualTrigger { get; private set; }
	[field: SerializeField] public bool Skill2HasAutoTrigger { get; private set; }
	[field: SerializeField] public float Skill2ManualCooldown { get; private set; }
	[field: SerializeField] public float Skill2AutoCooldown { get; private set; }
	[field: SerializeField] public float Skill1Duration { get; private set; }
	[field: SerializeField] public float Skill2Duration { get; private set; }

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

	public void ReduceSkill1ManualCooldown(float amount)
	{
		this.Skill1ManualCooldown = ReduceCooldown(this.Skill1ManualCooldown, amount);
		Debug.Log($"Skill1 Manual Cooldown: {this.Skill1ManualCooldown}");
	}

	public void ReduceSkill1AutoCooldown(float amount)
	{
		this.Skill1AutoCooldown = ReduceCooldown(this.Skill1AutoCooldown, amount);
		Debug.Log($"Skill1 Auto Cooldown: {this.Skill1AutoCooldown}");
	}

	public void ReduceSkill2ManualCooldown(float amount)
	{
		this.Skill2ManualCooldown = ReduceCooldown(this.Skill2ManualCooldown, amount);
		Debug.Log($"Skill2 Manual Cooldown: {this.Skill2ManualCooldown}");
	}

	public void ReduceSkill2AutoCooldown(float amount)
	{
		this.Skill2AutoCooldown = ReduceCooldown(this.Skill2AutoCooldown, amount);
		Debug.Log($"Skill2 Auto Cooldown: {this.Skill2AutoCooldown}");
	}

	private static float ReduceCooldown(float cooldown, float amount)
	{
		if (cooldown <= 0f)
			return 0f;

		return Mathf.Max(1f, cooldown - amount);
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
