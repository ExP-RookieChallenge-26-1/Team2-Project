#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DamageTextIndicatorTests
{
	[Test]
	public void DamageTextSpawnerPlacesSpawnedTextOnEffectSortingLayer()
	{
		using DamageTextContext context = DamageTextContext.Create();

		context.Spawner.Spawn(Vector3.zero, 12, Color.white);

		FloatingDamageText spawned = context.FindSpawnedText();
		Renderer textRenderer = spawned.GetComponentInChildren<Renderer>();
		Assert.That(textRenderer.sortingLayerName, Is.EqualTo("Effect"));
		Assert.That(textRenderer.sortingOrder, Is.EqualTo(50));
	}

	[Test]
	public void CowKingTakeDamageSpawnsDamageText()
	{
		using DamageTextContext context = DamageTextContext.Create();
		GameObject bossObject = new("CowKing");
		try
		{
			CowKing cowKing = bossObject.AddComponent<CowKing>();
			SetPrivateField(cowKing, "canTakeDamage", true);

			cowKing.TakeDamage(7);

			FloatingDamageText spawned = context.FindSpawnedText();
			TextMeshPro text = spawned.GetComponentInChildren<TextMeshPro>();
			Assert.That(text.text, Is.EqualTo("7"));
		}
		finally
		{
			Object.DestroyImmediate(bossObject);
		}
	}

	[Test]
	public void FloatingDamageTextPrefabUsesGameFontAsset()
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
			"Assets/UI/DamageText/FloatingDamageText.prefab");
		TextMeshPro text = prefab.GetComponentInChildren<TextMeshPro>(true);

		Assert.That(text.font.name, Does.Contain("Hakgyoansim"));
	}

	[Test]
	public void UserHealthTakeDamageDoesNotSpawnPaddleDamageTextIndicator()
	{
		string source = File.ReadAllText("Assets/_Global/UserHealth.cs");

		Assert.That(source, Does.Not.Contain("DamageTextSpawner"));
	}

	private static void SetPrivateField(object target, string fieldName, object value)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(field, Is.Not.Null, fieldName);
		field.SetValue(target, value);
	}

	private sealed class DamageTextContext : System.IDisposable
	{
		private DamageTextContext(GameObject spawnerObject, GameObject prefabObject, DamageTextSpawner spawner)
		{
			SpawnerObject = spawnerObject;
			PrefabObject = prefabObject;
			Spawner = spawner;
		}

		private GameObject SpawnerObject { get; }
		private GameObject PrefabObject { get; }
		public DamageTextSpawner Spawner { get; }

		public static DamageTextContext Create()
		{
			GameObject prefabObject = new("FloatingDamageText", typeof(FloatingDamageText));
			GameObject textObject = new("Text", typeof(TextMeshPro));
			textObject.transform.SetParent(prefabObject.transform, false);
			FloatingDamageText prefab = prefabObject.GetComponent<FloatingDamageText>();
			SetPrivateField(prefab, "text", textObject.GetComponent<TextMeshPro>());

			GameObject spawnerObject = new("DamageTextSpawner", typeof(DamageTextSpawner));
			DamageTextSpawner spawner = spawnerObject.GetComponent<DamageTextSpawner>();
			SetPrivateField(spawner, "prefab", prefab);
			return new DamageTextContext(spawnerObject, prefabObject, spawner);
		}

		public FloatingDamageText FindSpawnedText()
		{
			foreach (FloatingDamageText text in Object.FindObjectsByType<FloatingDamageText>(FindObjectsSortMode.None))
			{
				if (text.gameObject != PrefabObject)
					return text;
			}

			Assert.Fail("No spawned damage text was found.");
			return null;
		}

		public void Dispose()
		{
			foreach (FloatingDamageText text in Object.FindObjectsByType<FloatingDamageText>(FindObjectsSortMode.None))
			{
				if (text.gameObject != PrefabObject)
					Object.DestroyImmediate(text.gameObject);
			}

			Object.DestroyImmediate(SpawnerObject);
			Object.DestroyImmediate(PrefabObject);
		}
	}
}
#endif
