#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BuffBadgeBehaviorTests
{
	private const string BallSkillPath = "Assets/Gameplay/Ball/BallSkill.cs";

	[Test]
	public void BadgeBarUsesTwoThirdsSizeCellsAndWrapsToItsWidthWithExistingHorizontalLayout()
	{
		GameObject barObject = new("BuffBadgeBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(BuffBadgeBarUI));
		BuffBadgeUI prefab = CreateBadgePrefab();
		BuffBadgeData badgeData = CreateBadgeData();

		try
		{
			RectTransform barRect = barObject.GetComponent<RectTransform>();
			barRect.sizeDelta = new Vector2(150f, 100f);
			BuffBadgeBarUI bar = barObject.GetComponent<BuffBadgeBarUI>();
			SetPrivateField(bar, "badgePrefab", prefab);

			Assert.DoesNotThrow(() =>
			{
				InvokePrivateMethod(bar, "OnBadgeAttached", new ActiveBuffBadge(badgeData, 0f));
				InvokePrivateMethod(bar, "OnBadgeAttached", new ActiveBuffBadge(badgeData, 0f));
				InvokePrivateMethod(bar, "OnBadgeAttached", new ActiveBuffBadge(badgeData, 0f));
			});

			HorizontalLayoutGroup horizontal = barObject.GetComponent<HorizontalLayoutGroup>();
			Assert.That(horizontal == null || !horizontal.enabled, Is.True);

			AssertBadgeRect(barObject.transform.GetChild(0), new Vector2(0f, 0f));
			AssertBadgeRect(barObject.transform.GetChild(1), new Vector2(BuffBadgeUI.BadgeSize.x, 0f));
			AssertBadgeRect(barObject.transform.GetChild(2), new Vector2(0f, -BuffBadgeUI.BadgeSize.y));
		}
		finally
		{
			Object.DestroyImmediate(barObject);
			Object.DestroyImmediate(prefab.gameObject);
			Object.DestroyImmediate(badgeData);
		}
	}

	private static void AssertBadgeRect(Transform transform, Vector2 expectedPosition)
	{
		RectTransform rect = transform as RectTransform;
		Assert.That(rect, Is.Not.Null);
		AssertVector(rect.anchorMin, new Vector2(0f, 1f));
		AssertVector(rect.anchorMax, new Vector2(0f, 1f));
		AssertVector(rect.pivot, new Vector2(0f, 1f));
		AssertVector(rect.sizeDelta, BuffBadgeUI.BadgeSize);
		AssertVector(rect.anchoredPosition, expectedPosition);
	}

	[Test]
	public void SkillStatUpgradeAttachesConfiguredBadgeData()
	{
		using TestManagers managers = new();
		StatUpgradeCardData card = ScriptableObject.CreateInstance<StatUpgradeCardData>();
		BuffBadgeData badgeData = CreateBadgeData();
		ActiveBuffBadge attachedBadge = null;

		try
		{
			SetPrivateField(card, "<BadgeData>k__BackingField", badgeData);
			SetPrivateField(card, "<Skill1DurationIncrease>k__BackingField", 1f);
			managers.BadgeManager.OnBadgeAttached += badge => attachedBadge = badge;

			card.Apply(CardUseContext.None);

			Assert.That(attachedBadge, Is.Not.Null);
			Assert.That(attachedBadge.Data, Is.SameAs(badgeData));
		}
		finally
		{
			Object.DestroyImmediate(card);
			Object.DestroyImmediate(badgeData);
		}
	}

	[Test]
	public void SkillStatUpgradeWithoutConfiguredBadgeDoesNotUseCardIcon()
	{
		using TestManagers managers = new();
		StatUpgradeCardData card = ScriptableObject.CreateInstance<StatUpgradeCardData>();
		Texture2D texture = new(1, 1);
		Sprite icon = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
		ActiveBuffBadge attachedBadge = null;

		try
		{
			SetPrivateField(card, "<Icon>k__BackingField", icon);
			SetPrivateField(card, "<Skill1DurationIncrease>k__BackingField", 1f);
			managers.BadgeManager.OnBadgeAttached += badge => attachedBadge = badge;

			card.Apply(CardUseContext.None);

			Assert.That(attachedBadge, Is.Null);
		}
		finally
		{
			Object.DestroyImmediate(card);
			Object.DestroyImmediate(icon);
			Object.DestroyImmediate(texture);
		}
	}

	[Test]
	public void ImmediateSkillActivationCardDoesNotAttachBadgeWithoutStatChange()
	{
		using TestManagers managers = new();
		StatUpgradeCardData card = ScriptableObject.CreateInstance<StatUpgradeCardData>();
		BuffBadgeData badgeData = CreateBadgeData();
		ActiveBuffBadge attachedBadge = null;

		try
		{
			SetPrivateField(card, "<BadgeData>k__BackingField", badgeData);
			SetPrivateField(card, "<ActivateSkill1Immediately>k__BackingField", true);
			managers.BadgeManager.OnBadgeAttached += badge => attachedBadge = badge;

			card.Apply(CardUseContext.None);

			Assert.That(attachedBadge, Is.Null);
		}
		finally
		{
			Object.DestroyImmediate(card);
			Object.DestroyImmediate(badgeData);
		}
	}

	[Test]
	public void BuffBadgeAssetsUseRequestedIconSprites()
	{
		AssertBadgeIconGuid("Assets/Data/BuffBadges/AttackBadge.asset", "8782d2b083526b04585a45adebeec599");
		AssertBadgeIconGuid("Assets/Data/BuffBadges/GiantBadge.asset", "b7cb60c4d5f7e794886dd6106c885426");
		AssertBadgeIconGuid("Assets/Data/BuffBadges/CritDamageBadge.asset", "6916f38f19c61c14ab266c45e7804677");
		AssertBadgeIconGuid("Assets/Data/BuffBadges/CritChanceBadge.asset", "893c43c8a57698d48a8d3cfaa7162c6c");
		AssertBadgeIconGuid("Assets/Data/BuffBadges/CloneBadge.asset", "7408578adc5e6444e8f3241f9eb9a618");
	}

	[Test]
	public void StatUpgradeCardsReferenceRequestedBadgeData()
	{
		AssertCardBadge("Assets/Data/Cards/Stats/Stat_Attack_005.asset", "Assets/Data/BuffBadges/AttackBadge.asset");
		AssertCardBadge("Assets/Data/Cards/Stats/Stat_CritDamage_050.asset", "Assets/Data/BuffBadges/CritDamageBadge.asset");
		AssertCardBadge("Assets/Data/Cards/Stats/Stat_CritChance_005.asset", "Assets/Data/BuffBadges/CritChanceBadge.asset");
		AssertCardBadge("Assets/Data/Cards/Skills/Skill_Giant_SizeUp.asset", "Assets/Data/BuffBadges/GiantBadge.asset");
		AssertCardBadge("Assets/Data/Cards/Skills/Skill_Clone_CountUp.asset", "Assets/Data/BuffBadges/CloneBadge.asset");
		AssertCardHasNoBadge("Assets/Data/Cards/Speed up.asset");
	}

	[Test]
	public void BallSkillDoesNotAttachBadgesWhenSkillActivates()
	{
		string source = File.ReadAllText(Path.Combine(Application.dataPath, "../", BallSkillPath));
		string activateBody = ExtractMethodBody(source, "Activate");

		Assert.That(activateBody, Does.Not.Contain("BuffBadgeManager.Instance"));
		Assert.That(activateBody, Does.Not.Contain(".Attach("));
	}

	private static BuffBadgeUI CreateBadgePrefab()
	{
		GameObject gameObject = new("BuffBadgePrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(BuffBadgeUI));
		SetPrivateField(gameObject.GetComponent<BuffBadgeUI>(), "icon", gameObject.GetComponent<Image>());
		return gameObject.GetComponent<BuffBadgeUI>();
	}

	private static BuffBadgeData CreateBadgeData()
	{
		return ScriptableObject.CreateInstance<BuffBadgeData>();
	}

	private static void AssertVector(Vector2 actual, Vector2 expected)
	{
		Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
		Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
	}

	private static void AssertBadgeIconGuid(string badgePath, string expectedIconGuid)
	{
		BuffBadgeData badge = AssetDatabase.LoadAssetAtPath<BuffBadgeData>(badgePath);
		Assert.That(badge, Is.Not.Null, $"{badgePath} is missing.");
		Assert.That(badge.Icon, Is.Not.Null, $"{badgePath} has no icon.");
		Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(badge.Icon)), Is.EqualTo(expectedIconGuid));
	}

	private static void AssertCardBadge(string cardPath, string badgePath)
	{
		StatUpgradeCardData card = AssetDatabase.LoadAssetAtPath<StatUpgradeCardData>(cardPath);
		BuffBadgeData expectedBadge = AssetDatabase.LoadAssetAtPath<BuffBadgeData>(badgePath);

		Assert.That(card, Is.Not.Null, $"{cardPath} is missing.");
		Assert.That(expectedBadge, Is.Not.Null, $"{badgePath} is missing.");
		Assert.That(card.BadgeData, Is.SameAs(expectedBadge), $"{cardPath} has the wrong badge.");
	}

	private static void AssertCardHasNoBadge(string cardPath)
	{
		StatUpgradeCardData card = AssetDatabase.LoadAssetAtPath<StatUpgradeCardData>(cardPath);
		Assert.That(card, Is.Not.Null, $"{cardPath} is missing.");
		Assert.That(card.BadgeData, Is.Null, $"{cardPath} should not attach an unmapped badge.");
	}

	private static string ExtractMethodBody(string source, string methodName)
	{
		string signature = $"void {methodName}(";
		int signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
		Assert.That(signatureIndex, Is.GreaterThanOrEqualTo(0), $"{methodName} method was not found.");

		int openBraceIndex = source.IndexOf('{', signatureIndex);
		Assert.That(openBraceIndex, Is.GreaterThanOrEqualTo(0), $"{methodName} method has no body.");

		int depth = 0;
		for (int i = openBraceIndex; i < source.Length; i++)
		{
			if (source[i] == '{')
				depth++;
			else if (source[i] == '}')
			{
				depth--;
				if (depth == 0)
					return source.Substring(openBraceIndex + 1, i - openBraceIndex - 1);
			}
		}

		Assert.Fail($"{methodName} method body was not closed.");
		return string.Empty;
	}

	private static void InvokePrivateMethod(object target, string methodName, params object[] args)
	{
		MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
		method.Invoke(target, args);
	}

	private static void SetPrivateField(object target, string fieldName, object value)
	{
		FieldInfo field = FindField(target.GetType(), fieldName);
		Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
		field.SetValue(target, value);
	}

	private static FieldInfo FindField(Type type, string fieldName)
	{
		while (type != null)
		{
			FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
				return field;

			type = type.BaseType;
		}

		return null;
	}

	private sealed class TestManagers : IDisposable
	{
		private readonly GameManager previousGameManager;
		private readonly BuffBadgeManager previousBadgeManager;
		private readonly GameObject gameManagerObject;
		private readonly GameObject badgeManagerObject;

		public TestManagers()
		{
			this.previousGameManager = GameManager.Instance;
			this.previousBadgeManager = BuffBadgeManager.Instance;
			SetGameManagerInstance(null);
			SetBadgeManagerInstance(null);

			this.gameManagerObject = new GameObject("TestGameManager");
			this.gameManagerObject.SetActive(false);
			this.GameManager = this.gameManagerObject.AddComponent<GameManager>();
			SetGameManagerInstance(this.GameManager);
			SetPrivateField(this.GameManager, "<BallStats>k__BackingField", ScriptableObject.CreateInstance<BallStats>());
			SetPrivateField(this.GameManager, "<PaddleStats>k__BackingField", ScriptableObject.CreateInstance<PaddleStats>());

			this.badgeManagerObject = new GameObject("TestBuffBadgeManager");
			this.BadgeManager = this.badgeManagerObject.AddComponent<BuffBadgeManager>();
		}

		public GameManager GameManager { get; }
		public BuffBadgeManager BadgeManager { get; }

		public void Dispose()
		{
			if (this.GameManager != null)
			{
				Object.DestroyImmediate(this.GameManager.BallStats);
				Object.DestroyImmediate(this.GameManager.PaddleStats);
			}

			Object.DestroyImmediate(this.badgeManagerObject);
			Object.DestroyImmediate(this.gameManagerObject);
			SetBadgeManagerInstance(this.previousBadgeManager);
			SetGameManagerInstance(this.previousGameManager);
		}
	}

	private static void SetGameManagerInstance(GameManager manager)
	{
		SetStaticAutoProperty(typeof(GameManager), "<Instance>k__BackingField", manager);
	}

	private static void SetBadgeManagerInstance(BuffBadgeManager manager)
	{
		SetStaticAutoProperty(typeof(BuffBadgeManager), "<Instance>k__BackingField", manager);
	}

	private static void SetStaticAutoProperty(Type type, string fieldName, object value)
	{
		FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
		Assert.That(field, Is.Not.Null, $"{type.Name}.{fieldName} is missing.");
		field.SetValue(null, value);
	}
}
#endif
