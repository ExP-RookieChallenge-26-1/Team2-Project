#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CardPoolConfigurationTests
{
	private static readonly HashSet<int> AllowedCardIds = CreateAllowedCardIds();

	private static HashSet<int> CreateAllowedCardIds()
	{
		HashSet<int> ids = new HashSet<int>
		{
			1005,
			2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010,
			3010, 3015, 3020, 3025, 3030, 3035, 3040, 3045, 3050, 3055,
			3060, 3065, 3070, 3075, 3080, 3085, 3090, 3095, 3100, 3105,
			3110, 3115, 3120, 3125, 3130, 3135, 3140, 3145, 3150,
			CardIds.GiantAutoCooldown, CardIds.GiantManualCooldown,
			CardIds.GiantSizeUp, CardIds.GiantDurationUp, CardIds.GiantInstant,
			CardIds.CloneAutoCooldown, CardIds.CloneManualCooldown,
			CardIds.CloneCountUp, CardIds.CloneInstant
		};

		foreach (int scoreId in CardIds.GetScoreBonusIds())
			ids.Add(scoreId);

		return ids;
	}

	[Test]
	public void CardDatabaseContainsOnlyRegisteredCards()
	{
		CardDatabase cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/Cards/CardDatabase.asset");
		List<CardData> cards = GetField<List<CardData>>(cardDatabase, "cards");

		Assert.That(cards.Select(card => card.Id), Is.SubsetOf(AllowedCardIds));
		Assert.That(cards.OfType<StatUpgradeCardData>().All(card => card.Speed == 0f), Is.True);

		foreach (int scoreId in CardIds.GetScoreBonusIds())
		{
			CardData scoreCard = cardDatabase.GetById(scoreId);
			Assert.That(scoreCard, Is.TypeOf<ScoreBonusCardData>());
			Assert.That(scoreCard.GetDescription(CardUseContext.None), Does.Contain(CardIds.GetScoreBonusAmount(scoreId).ToString()));
		}
	}

	[Test]
	public void GameSceneLevelUpPoolsReferenceOnlyRegisteredCards()
	{
		EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
		UserLevel userLevel = Object.FindFirstObjectByType<UserLevel>();
		InvokePrivateMethod(userLevel, "EnsureDefaultProgressionData");
		UserLevel.UpgradePool[] pools = GetField<UserLevel.UpgradePool[]>(userLevel, "defaultUpgradePools");

		Assert.That(pools, Has.Length.EqualTo(49));
		foreach (UserLevel.UpgradePool pool in pools)
		{
			Assert.That(pool.UpgradeIds, Has.Length.EqualTo(pool.UpgradeWeights.Length));
			Assert.That(pool.UpgradeIds.All(AllowedCardIds.Contains), Is.True);
			AssertScoreCardsTakeOneSixth(pool.UpgradeIds, pool.UpgradeWeights);
		}
	}

	[Test]
	public void GameSceneLevelUpPoolsMatchProgressionWeights()
	{
		EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
		UserLevel userLevel = Object.FindFirstObjectByType<UserLevel>();
		InvokePrivateMethod(userLevel, "EnsureDefaultProgressionData");
		UserLevel.UpgradePool[] pools = GetField<UserLevel.UpgradePool[]>(userLevel, "defaultUpgradePools");

		AssertPool(pools[0], new[] { CardIds.Attack5 }, new[] { 1f });
		AssertPool(
			pools[1],
			new[] { CardIds.Attack5, 3010, 2010, CardIds.CloneInstant },
			new[] { 5f, 5f, 5f, 1000f });
		AssertPool(
			pools[2],
			new[] { CardIds.Attack5, 3015, 2010, CardIds.CloneInstant, CardIds.GiantAutoCooldown },
			new[] { 10f, 10f, 10f, 3f, 3f });
		AssertPool(
			pools[3],
			new[]
			{
				CardIds.Attack5, 3020, 2010,
				CardIds.CloneInstant, CardIds.GiantAutoCooldown,
				CardIds.GiantSizeUp, CardIds.GiantDurationUp,
				CardIds.CloneCountUp, CardIds.GiantInstant
			},
			new[] { 30f, 30f, 30f, 3f, 3f, 3f, 3f, 3f, 3f });
		AssertPool(
			pools[6],
			new[]
			{
				CardIds.Attack5, 3030, 2009,
				CardIds.CloneInstant, CardIds.GiantAutoCooldown,
				CardIds.GiantSizeUp, CardIds.GiantDurationUp,
				CardIds.CloneCountUp, CardIds.GiantInstant,
				CardIds.CloneAutoCooldown, CardIds.GiantManualCooldown
			},
			new[] { 40f, 40f, 40f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f });
		AssertPool(
			pools[13],
			new[]
			{
				CardIds.Attack5, 3050, 2008,
				CardIds.CloneInstant, CardIds.GiantAutoCooldown,
				CardIds.GiantSizeUp, CardIds.GiantDurationUp,
				CardIds.CloneCountUp, CardIds.GiantInstant,
				CardIds.CloneAutoCooldown, CardIds.GiantManualCooldown,
				CardIds.CloneManualCooldown
			},
			new[] { 45f, 45f, 45f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f });
		AssertPool(
			pools[48],
			new[]
			{
				CardIds.Attack5, 3150, 2001,
				CardIds.CloneInstant, CardIds.GiantAutoCooldown,
				CardIds.GiantSizeUp, CardIds.GiantDurationUp,
				CardIds.CloneCountUp, CardIds.GiantInstant,
				CardIds.CloneAutoCooldown, CardIds.GiantManualCooldown,
				CardIds.CloneManualCooldown
			},
			new[] { 45f, 45f, 45f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f });
	}

	[Test]
	public void EnhancementTicketPoolReferencesOnlyRegisteredCards()
	{
		EnhancementTriggerItemData itemData = AssetDatabase.LoadAssetAtPath<EnhancementTriggerItemData>("Assets/Data/EnhancementTicketItemData.asset");
		int[] upgradeIds = GetField<int[]>(itemData, "upgradeIds");
		float[] upgradeWeights = GetField<float[]>(itemData, "upgradeWeights");

		Assert.That(upgradeIds, Has.Length.EqualTo(upgradeWeights.Length));
		Assert.That(upgradeIds.All(AllowedCardIds.Contains), Is.True);
	}

	[Test]
	public void DefaultItemPoolsGiveScoreCardsOneSixthTotalWeight()
	{
		int[] ids = InvokePrivateStaticMethod<int[]>(typeof(EnhancementTriggerItemData), "CreateDefaultItemUpgradeIds", 12);
		float[] weights = InvokePrivateStaticMethod<float[]>(typeof(EnhancementTriggerItemData), "CreateDefaultItemUpgradeWeights", 12);

		Assert.That(ids.All(AllowedCardIds.Contains), Is.True);
		AssertScoreCardsTakeOneSixth(ids, weights);
	}

	private static T GetField<T>(object target, string fieldName)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		return (T)field.GetValue(target);
	}

	private static void InvokePrivateMethod(object target, string methodName)
	{
		MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		method.Invoke(target, null);
	}

	private static T InvokePrivateStaticMethod<T>(System.Type type, string methodName, params object[] args)
	{
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
		return (T)method.Invoke(null, args);
	}

	private static void AssertScoreCardsTakeOneSixth(int[] ids, float[] weights)
	{
		float scoreWeight = 0f;
		float nonScoreWeight = 0f;
		float? individualScoreWeight = null;

		for (int i = 0; i < ids.Length; ++i)
		{
			if (!CardIds.IsScoreBonus(ids[i]))
			{
				nonScoreWeight += weights[i];
				continue;
			}

			scoreWeight += weights[i];
			if (individualScoreWeight.HasValue)
				Assert.That(weights[i], Is.EqualTo(individualScoreWeight.Value).Within(0.0001f));
			else
				individualScoreWeight = weights[i];
		}

		Assert.That(ids.Count(CardIds.IsScoreBonus), Is.EqualTo(CardIds.GetScoreBonusIds().Length));
		Assert.That(scoreWeight / (scoreWeight + nonScoreWeight), Is.EqualTo(1f / 6f).Within(0.0001f));
	}

	private static void AssertPool(UserLevel.UpgradePool pool, int[] expectedIds, float[] expectedWeights)
	{
		int[] scoreIds = CardIds.GetScoreBonusIds();
		float scoreWeight = expectedWeights.Sum() * 1f / 5f / scoreIds.Length;

		Assert.That(pool.UpgradeIds, Is.EqualTo(expectedIds.Concat(scoreIds).ToArray()));
		Assert.That(pool.UpgradeWeights, Is.EqualTo(expectedWeights.Concat(Enumerable.Repeat(scoreWeight, scoreIds.Length)).ToArray()));
		AssertScoreCardsTakeOneSixth(pool.UpgradeIds, pool.UpgradeWeights);
	}
}
#endif
