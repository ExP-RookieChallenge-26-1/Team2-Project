public static class CardIds
{
	public const int Attack5 = 1005;
	public const int CriticalChanceMin = 2001;
	public const int CriticalChanceMax = 2010;
	public const int CriticalChance = 2005;
	public const int ScoreBonusMinAmount = 25;
	public const int ScoreBonusMaxAmount = 200;
	public const int ScoreBonusStep = 5;
	public const int ScoreBonusIdOffset = 5000;

	public const int GiantAutoCooldown = 4101;
	public const int GiantManualCooldown = 4102;
	public const int GiantSizeUp = 4103;
	public const int GiantDurationUp = 4104;
	public const int GiantInstant = 4105;

	public const int CloneAutoCooldown = 4201;
	public const int CloneManualCooldown = 4202;
	public const int CloneCountUp = 4203;
	public const int CloneInstant = 4204;

	public static bool IsCriticalChance(int id)
	{
		return id >= CriticalChanceMin && id <= CriticalChanceMax;
	}

	public static int GetCriticalChanceIdForIndex(int progressionIndex)
	{
		int index = UnityEngine.Mathf.Max(1, progressionIndex);
		int percent = UnityEngine.Mathf.Max(1, 10 - UnityEngine.Mathf.FloorToInt((index - 1) / 5f));
		return 2000 + percent;
	}

	public static float GetCriticalChanceBonus(int id)
	{
		if (!IsCriticalChance(id))
			return 0f;

		return (id - 2000) / 100f;
	}

	public static bool IsScoreBonus(int id)
	{
		int amount = id - ScoreBonusIdOffset;
		return amount >= ScoreBonusMinAmount &&
		       amount <= ScoreBonusMaxAmount &&
		       amount % ScoreBonusStep == 0;
	}

	public static int GetScoreBonusId(int amount)
	{
		int clamped = UnityEngine.Mathf.Clamp(amount, ScoreBonusMinAmount, ScoreBonusMaxAmount);
		int stepped = clamped / ScoreBonusStep * ScoreBonusStep;
		return ScoreBonusIdOffset + stepped;
	}

	public static int GetScoreBonusAmount(int id)
	{
		return IsScoreBonus(id) ? id - ScoreBonusIdOffset : 0;
	}

	public static int[] GetScoreBonusIds()
	{
		int count = (ScoreBonusMaxAmount - ScoreBonusMinAmount) / ScoreBonusStep + 1;
		int[] ids = new int[count];
		for (int i = 0; i < count; ++i)
			ids[i] = GetScoreBonusId(ScoreBonusMinAmount + i * ScoreBonusStep);

		return ids;
	}

	public static void AddScoreBonusCards(System.Collections.Generic.List<int> ids, System.Collections.Generic.List<float> weights)
	{
		if (ids == null || weights == null || ids.Count != weights.Count)
			return;

		int[] scoreIds = GetScoreBonusIds();
		float nonScoreWeight = 0f;
		for (int i = 0; i < ids.Count; ++i)
		{
			if (!IsScoreBonus(ids[i]))
				nonScoreWeight += weights[i];
		}

		float scoreWeight = nonScoreWeight * 1f / 5f;
		float individualWeight = scoreIds.Length > 0 ? scoreWeight / scoreIds.Length : 0f;
		for (int i = 0; i < scoreIds.Length; ++i)
		{
			ids.Add(scoreIds[i]);
			weights.Add(individualWeight);
		}
	}

	public static bool HasCompleteScoreBonusWeightShare(int[] ids, float[] weights)
	{
		if (ids == null || weights == null || ids.Length != weights.Length)
			return false;

		int scoreCount = 0;
		float scoreWeight = 0f;
		float nonScoreWeight = 0f;
		float individualScoreWeight = -1f;

		for (int i = 0; i < ids.Length; ++i)
		{
			if (!IsScoreBonus(ids[i]))
			{
				nonScoreWeight += weights[i];
				continue;
			}

			scoreCount++;
			scoreWeight += weights[i];
			if (individualScoreWeight < 0f)
				individualScoreWeight = weights[i];
			else if (UnityEngine.Mathf.Abs(individualScoreWeight - weights[i]) > 0.0001f)
				return false;
		}

		if (scoreCount != GetScoreBonusIds().Length)
			return false;

		float totalWeight = scoreWeight + nonScoreWeight;
		return totalWeight > 0f && UnityEngine.Mathf.Abs(scoreWeight / totalWeight - 1f / 6f) <= 0.0001f;
	}
}
