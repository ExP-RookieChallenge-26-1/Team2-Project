#if UNITY_EDITOR
using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class CardProgressionTests
{
	[Test]
	public void CriticalChanceCardScalesDownByProgressionIndex()
	{
		Assert.That(StatUpgradeCardData.CalculateCriticalChanceBonus(0), Is.EqualTo(0.10f).Within(0.001f));
		Assert.That(StatUpgradeCardData.CalculateCriticalChanceBonus(1), Is.EqualTo(0.10f).Within(0.001f));
		Assert.That(StatUpgradeCardData.CalculateCriticalChanceBonus(5), Is.EqualTo(0.10f).Within(0.001f));
		Assert.That(StatUpgradeCardData.CalculateCriticalChanceBonus(6), Is.EqualTo(0.09f).Within(0.001f));
		Assert.That(StatUpgradeCardData.CalculateCriticalChanceBonus(50), Is.EqualTo(0.01f).Within(0.001f));
	}

	[Test]
	public void DamageScoreUsesFlooredCumulativeLostHealthPercent()
	{
		Assert.That(ScoreManager.CalculateLostHealthScore(30, 100), Is.EqualTo(70));
		Assert.That(ScoreManager.CalculateLostHealthScore(0, 100), Is.EqualTo(100));
		Assert.That(ScoreManager.CalculateLostHealthScore(1, 3), Is.EqualTo(66));
		Assert.That(ScoreManager.CalculateDamageScoreDelta(100, 30, 100), Is.EqualTo(70));
		Assert.That(ScoreManager.CalculateDamageScoreDelta(30, 0, 100), Is.EqualTo(30));
		Assert.That(ScoreManager.CalculateDamageScoreDelta(2, 1, 3), Is.EqualTo(33));
	}

	[Test]
	public void ScoreBonusIdsRepresentTwentyFiveToTwoHundredByFive()
	{
		int[] ids = CardIds.GetScoreBonusIds();

		Assert.That(ids, Has.Length.EqualTo(36));
		Assert.That(ids.First(), Is.EqualTo(CardIds.GetScoreBonusId(25)));
		Assert.That(ids.Last(), Is.EqualTo(CardIds.GetScoreBonusId(200)));
		Assert.That(ids.All(CardIds.IsScoreBonus), Is.True);
		Assert.That(ids.Select(CardIds.GetScoreBonusAmount), Is.EqualTo(Enumerable.Range(5, 36).Select(value => value * 5)));
	}

	[Test]
	public void ScoreBonusCardAddsConfiguredScore()
	{
		GameObject scoreObject = new GameObject("ScoreManager");
		ScoreManager scoreManager = scoreObject.AddComponent<ScoreManager>();
		ScoreBonusCardData card = ScriptableObject.CreateInstance<ScoreBonusCardData>();

		try
		{
			card.InitializeForRuntime(75);

			card.Apply();

			Assert.That(scoreManager.CurrentScore, Is.EqualTo(75));
		}
		finally
		{
			Object.DestroyImmediate(card);
			Object.DestroyImmediate(scoreObject);
		}
	}

	[Test]
	public void LevelThirtyPoolUsesConfiguredTemplate()
	{
		GameObject gameObject = new GameObject("UserLevel");
		UserLevel userLevel = gameObject.AddComponent<UserLevel>();

		UserLevel.UpgradePool pool = userLevel.GetDefaultUpgradePoolForLevel(30);
		int[] scoreIds = CardIds.GetScoreBonusIds();
		int[] baseIds =
		{
			CardIds.Attack5,
			3095,
			2005,
			CardIds.CloneInstant,
			CardIds.GiantAutoCooldown,
			CardIds.GiantSizeUp,
			CardIds.GiantDurationUp,
			CardIds.CloneCountUp,
			CardIds.GiantInstant,
			CardIds.CloneAutoCooldown,
			CardIds.GiantManualCooldown,
			CardIds.CloneManualCooldown
		};
		float[] baseWeights = { 45f, 45f, 45f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f };
		float scoreWeight = baseWeights.Sum() * 1f / 5f / scoreIds.Length;

		Assert.That(pool.UpgradeIds, Is.EqualTo(baseIds.Concat(scoreIds).ToArray()));
		Assert.That(pool.UpgradeWeights, Is.EqualTo(baseWeights.Concat(Enumerable.Repeat(scoreWeight, scoreIds.Length)).ToArray()));

		float nonScoreWeight = pool.UpgradeWeights.Where((_, index) => !CardIds.IsScoreBonus(pool.UpgradeIds[index])).Sum();
		float scoreTotalWeight = pool.UpgradeWeights.Where((_, index) => CardIds.IsScoreBonus(pool.UpgradeIds[index])).Sum();
		Assert.That(scoreTotalWeight / (nonScoreWeight + scoreTotalWeight), Is.EqualTo(1f / 6f).Within(0.0001f));

		Object.DestroyImmediate(gameObject);
	}
}
#endif
