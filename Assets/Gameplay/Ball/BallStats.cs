using UnityEngine;

[CreateAssetMenu(fileName = "BallStats", menuName = "Stats/BallStats")]
public class BallStats : ScriptableObject
{
	private const float DefaultSkill1Cooldown = 15f;
	private const float DefaultSkill2Cooldown = 20f;

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
	[field: SerializeField] public int Skill1SizeLevel { get; private set; }
	[field: SerializeField] public int Skill2CloneLevel { get; private set; }

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

	public void IncreaseGiantSize(float amount)
	{
		this.GiantSize = Mathf.Max(1f, this.GiantSize + amount);
		Debug.Log($"Giant Size: {this.GiantSize}");
	}

	public void IncreaseSkill1Duration(float amount)
	{
		this.Skill1Duration = Mathf.Max(0f, this.Skill1Duration + amount);
		Debug.Log($"Skill1 Duration: {this.Skill1Duration}");
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

	public void UnlockSkill1ManualTrigger()
	{
		this.Skill1HasManualTrigger = true;
		this.Skill1ManualCooldown = DefaultSkill1Cooldown;
		Debug.Log("Skill1 Manual Trigger unlocked.");
	}

	public void UnlockSkill1AutoTrigger()
	{
		this.Skill1HasAutoTrigger = true;
		this.Skill1AutoCooldown = DefaultSkill1Cooldown;
		Debug.Log("Skill1 Auto Trigger unlocked.");
	}

	public void UnlockSkill2ManualTrigger()
	{
		this.Skill2HasManualTrigger = true;
		this.Skill2ManualCooldown = DefaultSkill2Cooldown;
		Debug.Log("Skill2 Manual Trigger unlocked.");
	}

	public void UnlockSkill2AutoTrigger()
	{
		this.Skill2HasAutoTrigger = true;
		this.Skill2AutoCooldown = DefaultSkill2Cooldown;
		Debug.Log("Skill2 Auto Trigger unlocked.");
	}

	public void IncreaseSkill1SizeLevel(int amount)
	{
		this.Skill1SizeLevel = Mathf.Max(0, this.Skill1SizeLevel + amount);
		Debug.Log($"Skill1 Size Level: {this.Skill1SizeLevel}");
	}

	public void IncreaseSkill2CloneLevel(int amount)
	{
		this.Skill2CloneLevel = Mathf.Max(0, this.Skill2CloneLevel + amount);
		Debug.Log($"Skill2 Clone Level: {this.Skill2CloneLevel}");
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
