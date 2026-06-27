#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnemyAttackColliderTests
{
	private const string EnemyPrefabPath = "Assets/Gameplay/Enemy/Enemy.prefab";
	private const string EnemyAttackAnimationPath = "Assets/Gameplay/Enemy/Enemy_Attack.anim";
	private const string EnemyAttackSpriteFolder = "Assets/Art/Enemy/attack";
	private const string AttackStatePath = "Assets/Gameplay/Enemy/AttackState.cs";
	private const string EnemyPath = "Assets/Gameplay/Enemy/Enemy.cs";
	private const float ExpectedMinX = -2.375f;
	private const float ExpectedMaxX = 2.375f;
	private const float ExpectedMinY = -2.440404f;
	private const float ExpectedMaxY = 1.1725f;
	private const float ExpectedEnemySpritePixelsPerUnit = 100f;

	[Test]
	public void EnemyAttackTriggerColliderKeepsWidthAndExtendsDownwardOnly()
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
		Assert.That(prefab, Is.Not.Null);

		PolygonCollider2D attackTrigger = prefab.GetComponent<PolygonCollider2D>();
		Assert.That(attackTrigger, Is.Not.Null);
		Assert.That(attackTrigger.isTrigger, Is.True);

		Bounds bounds = CalculateLocalBounds(attackTrigger);

		Assert.That(bounds.min.x, Is.EqualTo(ExpectedMinX).Within(0.001f));
		Assert.That(bounds.max.x, Is.EqualTo(ExpectedMaxX).Within(0.001f));
		Assert.That(bounds.min.y, Is.EqualTo(ExpectedMinY).Within(0.001f));
		Assert.That(bounds.max.y, Is.EqualTo(ExpectedMaxY).Within(0.001f));
	}

	[Test]
	public void EnemyAttackAnimationInvokesDamageCallbackAtClipEnd()
	{
		AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(EnemyAttackAnimationPath);
		Assert.That(clip, Is.Not.Null);

		AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
		AnimationEvent hitEvent = Array.Find(events, e => e.functionName == "OnAttackAnimationFinished");

		Assert.That(hitEvent, Is.Not.Null);
		Assert.That(hitEvent.time, Is.EqualTo(clip.length).Within(1f / clip.frameRate));
	}

	[Test]
	public void EnemyAttackSpritesUseConsistentPixelsPerUnit()
	{
		string[] spritePaths = Directory.GetFiles(EnemyAttackSpriteFolder, "*.png");
		Assert.That(spritePaths, Is.Not.Empty);

		foreach (string spritePath in spritePaths)
		{
			TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
			Assert.That(importer, Is.Not.Null, spritePath);
			Assert.That(
				importer.spritePixelsPerUnit,
				Is.EqualTo(ExpectedEnemySpritePixelsPerUnit).Within(0.001f),
				spritePath);
		}
	}

	[Test]
	public void AttackStateChecksPaddleOverlapBeforeDamageAtAttackAnimationEnd()
	{
		string attackStateSource = ReadProjectFile(AttackStatePath);
		string enemySource = ReadProjectFile(EnemyPath);

		Assert.That(attackStateSource, Does.Not.Contain("AttackDelay"));
		Assert.That(attackStateSource, Does.Not.Contain("attackTimer"));
		Assert.That(attackStateSource, Does.Not.Contain("Time.deltaTime"));
		Assert.That(attackStateSource, Does.Contain("public void CompleteAttack(Enemy enemy)"));

		int completeAttackIndex = IndexOfRequired(attackStateSource, "public void CompleteAttack(Enemy enemy)");
		int overlapCheckIndex = IndexOfRequired(attackStateSource, "enemy.IsPaddleOverlappingAttackCollider()");
		int damageIndex = IndexOfRequired(attackStateSource, "GameManager.Instance.User.Health.TakeDamage(enemy.Stats.AttackDamage);");
		int idleIndex = IndexOfRequired(attackStateSource, "enemy.ChangeState(enemy.IdleState);");
		Assert.That(damageIndex, Is.GreaterThan(completeAttackIndex));
		Assert.That(overlapCheckIndex, Is.GreaterThan(completeAttackIndex));
		Assert.That(damageIndex, Is.GreaterThan(overlapCheckIndex));
		Assert.That(idleIndex, Is.GreaterThan(damageIndex));

		Assert.That(enemySource, Does.Contain("public void OnAttackAnimationFinished()"));
		Assert.That(enemySource, Does.Contain("public bool IsPaddleOverlappingAttackCollider()"));
		Assert.That(enemySource, Does.Contain("Physics2D.SyncTransforms();"));
		Assert.That(enemySource, Does.Contain("enemyCollider.Distance(paddleCollider).isOverlapped"));
		Assert.That(enemySource, Does.Contain("this.currentState == this.AttackState"));
		Assert.That(enemySource, Does.Contain("this.AttackState.CompleteAttack(this);"));
	}

	private static Bounds CalculateLocalBounds(PolygonCollider2D collider)
	{
		bool hasPoint = false;
		Vector2 min = Vector2.zero;
		Vector2 max = Vector2.zero;

		for (int pathIndex = 0; pathIndex < collider.pathCount; pathIndex++)
		{
			foreach (Vector2 point in collider.GetPath(pathIndex))
			{
				if (!hasPoint)
				{
					min = point;
					max = point;
					hasPoint = true;
					continue;
				}

				min = Vector2.Min(min, point);
				max = Vector2.Max(max, point);
			}
		}

		Assert.That(hasPoint, Is.True);

		Vector2 center = (min + max) * 0.5f;
		Vector2 size = max - min;
		return new Bounds(center, size);
	}

	private static string ReadProjectFile(string projectRelativePath)
	{
		return File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
	}

	private static int IndexOfRequired(string source, string requiredText)
	{
		int index = source.IndexOf(requiredText, StringComparison.Ordinal);
		Assert.That(index, Is.GreaterThanOrEqualTo(0), $"{requiredText} is missing.");
		return index;
	}
}
#endif
