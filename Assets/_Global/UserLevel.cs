using System;
using UnityEngine;

public class UserLevel : MonoBehaviour
{
	private static readonly int[] DefaultRequiredExpByLevel =
	{
		1, 7, 13, 20, 27, 33,
		60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180,
		271, 286, 300, 314, 329, 343, 357, 371, 386, 400, 414, 429, 443, 457, 471,
		680, 700, 720, 740, 760, 780, 800, 820, 840, 860, 880, 900, 920, 940, 960
	};

	private static readonly int[] KnownUpgradeCardIds = CreateKnownUpgradeCardIds();

	private const int DefaultUpgradePoolCount = 49;

	[System.Serializable]
	public class UpgradePool
	{
		[SerializeField] private int[] upgradeIds = { 1005, 2005, 3010 };
		[SerializeField] private float[] upgradeWeights = { 2f, 1f, 1f };

		public int[] UpgradeIds => this.upgradeIds;
		public float[] UpgradeWeights => this.upgradeWeights;

		public UpgradePool() { }

		public UpgradePool(int[] upgradeIds, float[] upgradeWeights)
		{
			this.upgradeIds = upgradeIds;
			this.upgradeWeights = upgradeWeights;
		}
	}

	[SerializeField] private int currentLevel = 1;
	[SerializeField] private int currentExp = 0;
	[SerializeField] private int[] requiredExpByLevel = CreateDefaultRequiredExpByLevel();

	[Header("Default Upgrade Pools")]
	[SerializeField] private UpgradePool[] defaultUpgradePools = CreateDefaultUpgradePools();

	[SerializeField] private EnhanceUI enhanceUI;

	public int CurrentLevel => this.currentLevel;
	public int CurrentExp => this.currentExp;
	public int RequiredExp => this.currentLevel - 1 < this.requiredExpByLevel.Length
		? this.requiredExpByLevel[this.currentLevel - 1]
		: 1;

	public event Action<int, int> OnExpChanged;

	private static int[] CreateKnownUpgradeCardIds()
	{
		System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>
		{
			1005,
			2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010,
			3010, 3015, 3020, 3025, 3030, 3035, 3040, 3045, 3050, 3055,
			3060, 3065, 3070, 3075, 3080, 3085, 3090, 3095, 3100, 3105,
			3110, 3115, 3120, 3125, 3130, 3135, 3140, 3145, 3150,
			CardIds.GiantAutoCooldown, CardIds.GiantManualCooldown,
			CardIds.CloneAutoCooldown, CardIds.CloneManualCooldown,
			CardIds.GiantSizeUp, CardIds.GiantDurationUp,
			CardIds.CloneCountUp, CardIds.GiantInstant, CardIds.CloneInstant
		};

		ids.AddRange(CardIds.GetScoreBonusIds());
		return ids.ToArray();
	}

	public static bool IsKnownUpgradeCardId(int id)
	{
		for (int i = 0; i < KnownUpgradeCardIds.Length; ++i)
		{
			if (KnownUpgradeCardIds[i] == id)
				return true;
		}

		return false;
	}

	private static int[] CreateDefaultRequiredExpByLevel()
	{
		return (int[])DefaultRequiredExpByLevel.Clone();
	}

	private static UpgradePool[] CreateDefaultUpgradePools()
	{
		UpgradePool[] pools = new UpgradePool[DefaultUpgradePoolCount];

		for (int level = 2; level <= DefaultUpgradePoolCount + 1; ++level)
		{
			pools[level - 2] = CreateDefaultUpgradePoolForLevel(level);
		}

		return pools;
	}

	private static UpgradePool CreateDefaultUpgradePoolForLevel(int level)
	{
		System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>();
		System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>();

		ids.Add(CardIds.Attack5);
		weights.Add(GetStatWeightForLevel(level));

		int criticalDamageId = GetCriticalDamageCardIdForLevel(level);
		if (criticalDamageId != 0)
		{
			ids.Add(criticalDamageId);
			weights.Add(GetStatWeightForLevel(level));
		}

		int criticalChanceId = GetCriticalChanceCardIdForLevel(level);
		if (criticalChanceId != 0)
		{
			ids.Add(criticalChanceId);
			weights.Add(GetStatWeightForLevel(level));
		}

		AddSkillCardsForLevel(ids, weights, level);
		CardIds.AddScoreBonusCards(ids, weights);

		return new UpgradePool(ids.ToArray(), weights.ToArray());
	}

	private static int GetCriticalDamageCardIdForLevel(int level)
	{
		int[] criticalDamageByLevel =
		{
			0, 0, 0,
			3010, 3015, 3020, 3025, 3030,
			3030, 3035, 3035, 3040, 3040,
			3045, 3045, 3050, 3055, 3055, 3060, 3065, 3070,
			3070, 3075, 3075, 3080, 3080,
			3085, 3085, 3090, 3095, 3095,
			3100, 3100, 3105, 3110, 3110,
			3115, 3115, 3120, 3120, 3125,
			3130, 3130, 3135, 3135, 3140,
			3140, 3145, 3145, 3150, 3150
		};

		return criticalDamageByLevel[Mathf.Clamp(level, 0, criticalDamageByLevel.Length - 1)];
	}

	private static int GetCriticalChanceCardIdForLevel(int level)
	{
		return level >= 3 ? CardIds.GetCriticalChanceIdForIndex(level) : 0;
	}

	private static float GetStatWeightForLevel(int level)
	{
		if (level <= 2)
			return 1f;

		if (level == 3)
			return 5f;

		return GetSkillWeightTotalForLevel(level) * 5f / 3f;
	}

	private static void AddSkillCardsForLevel(System.Collections.Generic.List<int> ids, System.Collections.Generic.List<float> weights, int level)
	{
		if (level < 3)
			return;

		if (level == 3)
		{
			ids.Add(CardIds.CloneInstant);
			weights.Add(1000f);
			return;
		}

		ids.Add(CardIds.CloneInstant);
		weights.Add(GetLevelSkillCardWeight(CardIds.CloneInstant));

		ids.Add(CardIds.GiantAutoCooldown);
		weights.Add(GetLevelSkillCardWeight(CardIds.GiantAutoCooldown));

		if (level >= 5)
		{
			ids.Add(CardIds.GiantSizeUp);
			weights.Add(GetLevelSkillCardWeight(CardIds.GiantSizeUp));
			ids.Add(CardIds.GiantDurationUp);
			weights.Add(GetLevelSkillCardWeight(CardIds.GiantDurationUp));
			ids.Add(CardIds.CloneCountUp);
			weights.Add(GetLevelSkillCardWeight(CardIds.CloneCountUp));
			ids.Add(CardIds.GiantInstant);
			weights.Add(GetLevelSkillCardWeight(CardIds.GiantInstant));
		}

		if (level >= 8)
		{
			ids.Add(CardIds.CloneAutoCooldown);
			weights.Add(GetLevelSkillCardWeight(CardIds.CloneAutoCooldown));
			ids.Add(CardIds.GiantManualCooldown);
			weights.Add(GetLevelSkillCardWeight(CardIds.GiantManualCooldown));
		}

		if (level >= 15)
		{
			ids.Add(CardIds.CloneManualCooldown);
			weights.Add(GetLevelSkillCardWeight(CardIds.CloneManualCooldown));
		}
	}

	private static float GetSkillWeightTotalForLevel(int level)
	{
		if (level < 3)
			return 0f;
		if (level < 4)
			return GetLevelSkillCardWeight(CardIds.CloneInstant);
		if (level < 5)
			return GetLevelSkillCardWeight(CardIds.CloneInstant) +
			       GetLevelSkillCardWeight(CardIds.GiantAutoCooldown);
		if (level < 8)
			return GetLevelSkillCardWeight(CardIds.CloneInstant) +
			       GetLevelSkillCardWeight(CardIds.GiantAutoCooldown) +
			       GetLevelSkillCardWeight(CardIds.GiantSizeUp) +
			       GetLevelSkillCardWeight(CardIds.GiantDurationUp) +
			       GetLevelSkillCardWeight(CardIds.CloneCountUp) +
			       GetLevelSkillCardWeight(CardIds.GiantInstant);
		if (level < 15)
			return GetLevelSkillCardWeight(CardIds.CloneInstant) +
			       GetLevelSkillCardWeight(CardIds.GiantAutoCooldown) +
			       GetLevelSkillCardWeight(CardIds.GiantSizeUp) +
			       GetLevelSkillCardWeight(CardIds.GiantDurationUp) +
			       GetLevelSkillCardWeight(CardIds.CloneCountUp) +
			       GetLevelSkillCardWeight(CardIds.GiantInstant) +
			       GetLevelSkillCardWeight(CardIds.CloneAutoCooldown) +
			       GetLevelSkillCardWeight(CardIds.GiantManualCooldown);

		return GetLevelSkillCardWeight(CardIds.CloneInstant) +
		       GetLevelSkillCardWeight(CardIds.GiantAutoCooldown) +
		       GetLevelSkillCardWeight(CardIds.GiantSizeUp) +
		       GetLevelSkillCardWeight(CardIds.GiantDurationUp) +
		       GetLevelSkillCardWeight(CardIds.CloneCountUp) +
		       GetLevelSkillCardWeight(CardIds.GiantInstant) +
		       GetLevelSkillCardWeight(CardIds.CloneAutoCooldown) +
		       GetLevelSkillCardWeight(CardIds.GiantManualCooldown) +
		       GetLevelSkillCardWeight(CardIds.CloneManualCooldown);
	}

	private static float GetLevelSkillCardWeight(int cardId)
	{
		return 3f;
	}

	private void Awake()
	{
		EnsureDefaultProgressionData();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		EnsureDefaultProgressionData();
	}
#endif

	private void EnsureDefaultProgressionData()
	{
		if (!AreRequiredExpValuesValid(this.requiredExpByLevel))
			this.requiredExpByLevel = CreateDefaultRequiredExpByLevel();

		if (!AreValidUpgradePools(this.defaultUpgradePools))
			this.defaultUpgradePools = CreateDefaultUpgradePools();
	}

	private static bool AreValidUpgradePools(UpgradePool[] upgradePools)
	{
		if (upgradePools == null || upgradePools.Length != DefaultUpgradePoolCount)
			return false;

		for (int i = 0; i < upgradePools.Length; ++i)
		{
			UpgradePool upgradePool = upgradePools[i];
			if (upgradePool == null ||
			    upgradePool.UpgradeIds == null ||
			    upgradePool.UpgradeWeights == null ||
			    upgradePool.UpgradeIds.Length == 0 ||
			    upgradePool.UpgradeIds.Length != upgradePool.UpgradeWeights.Length)
				return false;

			for (int j = 0; j < upgradePool.UpgradeIds.Length; ++j)
			{
				if (!IsKnownUpgradeCardId(upgradePool.UpgradeIds[j]))
					return false;
			}

			if (!CardIds.HasCompleteScoreBonusWeightShare(upgradePool.UpgradeIds, upgradePool.UpgradeWeights))
				return false;

			UpgradePool expectedPool = CreateDefaultUpgradePoolForLevel(i + 2);
			if (!AreIntArraysEqual(upgradePool.UpgradeIds, expectedPool.UpgradeIds) ||
			    !AreFloatArraysEqual(upgradePool.UpgradeWeights, expectedPool.UpgradeWeights))
				return false;
		}

		return true;
	}

	private static bool AreIntArraysEqual(int[] left, int[] right)
	{
		if (left == null || right == null || left.Length != right.Length)
			return false;

		for (int i = 0; i < left.Length; ++i)
		{
			if (left[i] != right[i])
				return false;
		}

		return true;
	}

	private static bool AreFloatArraysEqual(float[] left, float[] right)
	{
		if (left == null || right == null || left.Length != right.Length)
			return false;

		for (int i = 0; i < left.Length; ++i)
		{
			if (Mathf.Abs(left[i] - right[i]) > 0.0001f)
				return false;
		}

		return true;
	}

	private static bool AreRequiredExpValuesValid(int[] requiredExp)
	{
		if (requiredExp == null || requiredExp.Length != DefaultRequiredExpByLevel.Length)
			return false;

		for (int i = 0; i < DefaultRequiredExpByLevel.Length; ++i)
		{
			if (requiredExp[i] != DefaultRequiredExpByLevel[i])
				return false;
		}

		return true;
	}

	private void Start()
	{
		if (this.enhanceUI != null)
			this.enhanceUI.gameObject.SetActive(false);
	}

	public void AddExp(int amount)
	{
		this.currentExp += amount;
		Debug.Log($"경험치 획득: +{amount}, 현재 EXP: {this.currentExp}");
		CheckLevelUp();
		OnExpChanged?.Invoke(this.currentExp, this.RequiredExp);
	}

	private void CheckLevelUp()
	{
		while (this.currentLevel - 1 < this.requiredExpByLevel.Length &&
		       this.currentExp >= this.requiredExpByLevel[this.currentLevel - 1])
		{
			this.currentExp -= this.requiredExpByLevel[this.currentLevel - 1];
			this.currentLevel++;

			Debug.Log($"레벨업! 현재 레벨: {this.currentLevel}");
			OpenDefaultUpgradeUI();
		}
	}

	private void OpenDefaultUpgradeUI()
	{
		UpgradePool upgradePool = GetDefaultUpgradePoolForLevel(this.currentLevel);
		if (upgradePool == null)
		{
			Debug.LogWarning("Default upgrade pool is not assigned.");
			return;
		}

		OpenUpgradeUI(
			upgradePool.UpgradeIds,
			upgradePool.UpgradeWeights,
			new CardUseContext(CardOfferSource.LevelUp, this.currentLevel));
	}

	public UpgradePool GetDefaultUpgradePoolForLevel(int level)
	{
		if (this.defaultUpgradePools == null || this.defaultUpgradePools.Length == 0)
			return null;

		int poolIndex = Mathf.Clamp(level - 2, 0, this.defaultUpgradePools.Length - 1);
		return this.defaultUpgradePools[poolIndex];
	}

	public void OpenUpgradeUI(int[] ids, float[] weights)
	{
		OpenUpgradeUI(ids, weights, CardUseContext.None);
	}

	public void OpenUpgradeUI(int[] ids, float[] weights, CardUseContext context)
	{
		if (this.enhanceUI == null)
		{
			Debug.LogWarning("EnhanceUI is not assigned.");
			return;
		}

		if (ids == null || weights == null || ids.Length == 0 || ids.Length != weights.Length)
		{
			Debug.LogWarning("Upgrade UI open skipped because the card pool is invalid.");
			return;
		}

		GameManager.Instance.State.Change(GameStateMachine.State.Enhancement);
		this.enhanceUI.gameObject.SetActive(true);
		this.enhanceUI.ShowCardsByIds(ids, weights, context);
	}
}
