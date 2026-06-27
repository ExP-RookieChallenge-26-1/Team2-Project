#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public class MobDamageOverlayTests
{
	[Test]
	public void PlayShowsOverlayImmediately()
	{
		GameObject enemyObject = new("Enemy");
		Texture2D texture = null;
		Sprite sprite = null;

		try
		{
			SpriteRenderer targetRenderer = enemyObject.AddComponent<SpriteRenderer>();
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
			targetRenderer.sprite = sprite;

			MobDamageOverlay overlay = enemyObject.AddComponent<MobDamageOverlay>();

			overlay.Play();

			SpriteRenderer overlayRenderer = enemyObject.transform
				.Find("DamageOverlay")
				.GetComponent<SpriteRenderer>();

			Assert.That(overlayRenderer.enabled, Is.True);
			Assert.That(overlayRenderer.color.a, Is.GreaterThan(0.95f));
		}
		finally
		{
			if (sprite != null)
				Object.DestroyImmediate(sprite);
			if (texture != null)
				Object.DestroyImmediate(texture);
			Object.DestroyImmediate(enemyObject);
		}
	}

	[Test]
	public void PlayCreatesRedOverlayAboveTargetAndFadesOutQuickly()
	{
		GameObject enemyObject = new("Enemy");
		Texture2D texture = null;
		Sprite sprite = null;

		try
		{
			SpriteRenderer targetRenderer = enemyObject.AddComponent<SpriteRenderer>();
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
			targetRenderer.sprite = sprite;
			targetRenderer.sortingOrder = 5;
			targetRenderer.flipX = true;

			MobDamageOverlay overlay = enemyObject.AddComponent<MobDamageOverlay>();

			overlay.Play();
			overlay.Tick(0.04f);

			Transform overlayTransform = enemyObject.transform.Find("DamageOverlay");
			Assert.That(overlayTransform, Is.Not.Null);

			SpriteRenderer overlayRenderer = overlayTransform.GetComponent<SpriteRenderer>();
			Assert.That(overlayRenderer, Is.Not.Null);
			Assert.That(overlayRenderer.sprite, Is.EqualTo(targetRenderer.sprite));
			Assert.That(overlayRenderer.sortingOrder, Is.EqualTo(targetRenderer.sortingOrder + 1));
			Assert.That(overlayRenderer.flipX, Is.True);
			Assert.That(overlayRenderer.color.r, Is.EqualTo(1f).Within(0.001f));
			Assert.That(overlayRenderer.color.g, Is.EqualTo(0f).Within(0.001f));
			Assert.That(overlayRenderer.color.b, Is.EqualTo(0f).Within(0.001f));
			Assert.That(overlayRenderer.color.a, Is.GreaterThan(0.95f));

			overlay.Tick(0.2f);

			Assert.That(overlayRenderer.color.a, Is.EqualTo(0f).Within(0.001f));
			Assert.That(overlay.IsFlashing, Is.False);
		}
		finally
		{
			if (sprite != null)
				Object.DestroyImmediate(sprite);
			if (texture != null)
				Object.DestroyImmediate(texture);
			Object.DestroyImmediate(enemyObject);
		}
	}
}
#endif
