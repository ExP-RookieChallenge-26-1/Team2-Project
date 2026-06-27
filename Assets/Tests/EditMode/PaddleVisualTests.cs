#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PaddleVisualTests
{
	[Test]
	public void InitializeKeepsLegacyRendererVisibleWhenSpritesCannotLoad()
	{
		GameObject paddleObject = new("Paddle");
		GameObject legacyObject = new("Legacy Renderer");
		legacyObject.transform.SetParent(paddleObject.transform);
		SpriteRenderer legacyRenderer = legacyObject.AddComponent<SpriteRenderer>();

		try
		{
			Paddle paddle = paddleObject.AddComponent<Paddle>();
			PaddleVisual visual = paddleObject.GetComponent<PaddleVisual>();

			visual.Initialize(paddle, _ => null, _ => null);

			SpriteRenderer rootRenderer = paddleObject.GetComponent<SpriteRenderer>();
			Assert.That(rootRenderer, Is.Not.Null);
			Assert.That(rootRenderer.sprite, Is.Null);
			Assert.That(legacyRenderer.enabled, Is.True);
		}
		finally
		{
			Object.DestroyImmediate(paddleObject);
		}
	}

	[Test]
	public void InitializeLoadsNimbusSpritePathsWithoutDuplicatingPrefix()
	{
		GameObject paddleObject = new("Paddle");
		List<string> loadedPaths = new();

		try
		{
			Paddle paddle = paddleObject.AddComponent<Paddle>();
			PaddleVisual visual = paddleObject.GetComponent<PaddleVisual>();

			visual.Initialize(
				paddle,
				path =>
				{
					loadedPaths.Add(path);
					return null;
				},
				_ => null);

			Assert.That(loadedPaths, Does.Contain("Paddle/NimbusCloud/nimbus_idle_small"));
			Assert.That(loadedPaths, Does.Not.Contain("Paddle/NimbusCloud/nimbus_nimbus_idle_small"));
		}
		finally
		{
			Object.DestroyImmediate(paddleObject);
		}
	}

	[Test]
	public void PlayDamagedShowsRedDamageOverlay()
	{
		GameObject paddleObject = new("Paddle");
		Texture2D texture = null;
		Sprite sprite = null;

		try
		{
			SpriteRenderer targetRenderer = paddleObject.AddComponent<SpriteRenderer>();
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
			targetRenderer.sprite = sprite;

			Paddle paddle = paddleObject.AddComponent<Paddle>();

			paddle.PlayDamaged();

			MobDamageOverlay overlay = paddleObject.GetComponent<MobDamageOverlay>();
			Assert.That(overlay, Is.Not.Null);
			overlay.Tick(0.04f);

			Transform overlayTransform = paddleObject.transform.Find("DamageOverlay");
			Assert.That(overlayTransform, Is.Not.Null);

			SpriteRenderer overlayRenderer = overlayTransform.GetComponent<SpriteRenderer>();
			Assert.That(overlayRenderer, Is.Not.Null);
			Assert.That(overlayRenderer.color.r, Is.EqualTo(1f).Within(0.001f));
			Assert.That(overlayRenderer.color.g, Is.EqualTo(0f).Within(0.001f));
			Assert.That(overlayRenderer.color.b, Is.EqualTo(0f).Within(0.001f));
			Assert.That(overlayRenderer.color.a, Is.GreaterThan(0.95f));
		}
		finally
		{
			if (sprite != null)
				Object.DestroyImmediate(sprite);
			if (texture != null)
				Object.DestroyImmediate(texture);
			Object.DestroyImmediate(paddleObject);
		}
	}

	[Test]
	public void PlayDamagedShowsOverlayAfterPaddleVisualDisablesLegacyRenderers()
	{
		GameObject paddleObject = new("Paddle");
		GameObject legacyObject = new("Legacy Renderer");
		Texture2D texture = null;
		Sprite sprite = null;

		try
		{
			legacyObject.transform.SetParent(paddleObject.transform);
			SpriteRenderer legacyRenderer = legacyObject.AddComponent<SpriteRenderer>();
			texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
			legacyRenderer.sprite = sprite;

			Paddle paddle = paddleObject.AddComponent<Paddle>();
			PaddleVisual visual = paddleObject.GetComponent<PaddleVisual>();
			visual.Initialize(paddle, _ => sprite, _ => null);

			Assert.That(legacyRenderer.enabled, Is.False);

			paddle.PlayDamaged();

			MobDamageOverlay overlay = paddleObject.GetComponent<MobDamageOverlay>();
			Assert.That(overlay, Is.Not.Null);
			overlay.Tick(0.04f);

			Transform overlayTransform = paddleObject.transform.Find("DamageOverlay");
			Assert.That(overlayTransform, Is.Not.Null);

			SpriteRenderer overlayRenderer = overlayTransform.GetComponent<SpriteRenderer>();
			Assert.That(overlayRenderer, Is.Not.Null);
			Assert.That(overlayRenderer.enabled, Is.True);
			Assert.That(overlayRenderer.color.a, Is.GreaterThan(0.95f));
		}
		finally
		{
			if (sprite != null)
				Object.DestroyImmediate(sprite);
			if (texture != null)
				Object.DestroyImmediate(texture);
			Object.DestroyImmediate(paddleObject);
		}
	}
}
#endif
