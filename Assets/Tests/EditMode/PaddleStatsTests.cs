#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PaddleStatsTests
{
	[Test]
	public void SetPaddleSizeLevelClampsAndExposesCurrentSize()
	{
		PaddleStats stats = ScriptableObject.CreateInstance<PaddleStats>();

		try
		{
			SetField(stats, "<PaddleSizeLevel>k__BackingField", 0);
			SetField(stats, "<MaxPaddleSizeLevel>k__BackingField", 2);
			SetField(stats, "<PaddleSizeMultiplierPerLevel>k__BackingField", new[] { 1f, 1.4f, 1.8f });
			SetField(stats, "<BaseColliderSize>k__BackingField", new Vector2(1.5f, 0.45f));

			int eventCount = 0;
			int eventLevel = -1;
			stats.OnPaddleSizeLevelChanged += level =>
			{
				eventCount++;
				eventLevel = level;
			};

			bool changed = stats.SetPaddleSizeLevel(5);

			Assert.That(changed, Is.True);
			Assert.That(stats.PaddleSizeLevel, Is.EqualTo(2));
			Assert.That(eventCount, Is.EqualTo(1));
			Assert.That(eventLevel, Is.EqualTo(2));
			Assert.That(stats.CurrentPaddleSizeMultiplier, Is.EqualTo(1.8f).Within(0.001f));
			Assert.That(stats.CurrentColliderSize.x, Is.EqualTo(2.7f).Within(0.001f));
			Assert.That(stats.CurrentColliderSize.y, Is.EqualTo(0.45f).Within(0.001f));
		}
		finally
		{
			Object.DestroyImmediate(stats);
		}
	}

	private static void SetField<T>(object target, string fieldName, T value)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

		Assert.That(field, Is.Not.Null, $"{fieldName} field should exist");
		field.SetValue(target, value);
	}
}
#endif
