using System;
using System.Collections.Generic;
using UnityEngine;

public enum PaddleVisualState
{
	Idle,
	Damaged,
	GetItem
}

public class PaddleVisual : MonoBehaviour
{
	private const string SpritePathPrefix = "Paddle/NimbusCloud/";

	[SerializeField] private float transientStateDuration = 0.22f;

	private readonly Dictionary<string, Sprite> spriteCache = new();
	private Paddle paddle;
	private SpriteRenderer spriteRenderer;
	private BoxCollider2D boxCollider;
	private PaddleVisualState state = PaddleVisualState.Idle;
	private float transientTimer;
	private bool isInitialized;

	public void Initialize(Paddle paddle)
	{
		Initialize(paddle, Resources.Load<Sprite>, Resources.Load<Texture2D>);
	}

	public void Initialize(
		Paddle paddle,
		Func<string, Sprite> spriteLoader,
		Func<string, Texture2D> textureLoader)
	{
		if (this.paddle != null && this.paddle.Stats != null)
			this.paddle.Stats.OnPaddleSizeLevelChanged -= HandlePaddleSizeLevelChanged;

		this.paddle = paddle;
		this.spriteRenderer = EnsureSpriteRenderer();
		this.boxCollider = GetComponent<BoxCollider2D>();

		LoadSprites(spriteLoader, textureLoader);

		if (this.paddle != null && this.paddle.Stats != null)
			this.paddle.Stats.OnPaddleSizeLevelChanged += HandlePaddleSizeLevelChanged;

		this.isInitialized = true;
		ApplySize();
		ApplySprite();
	}

	private void OnDestroy()
	{
		if (this.paddle != null && this.paddle.Stats != null)
			this.paddle.Stats.OnPaddleSizeLevelChanged -= HandlePaddleSizeLevelChanged;
	}

	public void Tick()
	{
		if (!this.isInitialized || this.transientTimer <= 0f)
			return;

		this.transientTimer -= Time.deltaTime;

		if (this.transientTimer <= 0f)
			SetState(PaddleVisualState.Idle);
	}

	public void SetState(PaddleVisualState state)
	{
		this.state = state;
		this.transientTimer = 0f;
		ApplySprite();
	}

	public void PlayDamaged(float duration = 0f)
	{
		PlayTransientState(PaddleVisualState.Damaged, duration);
	}

	public void PlayGetItem(float duration = 0f)
	{
		PlayTransientState(PaddleVisualState.GetItem, duration);
	}

	private void PlayTransientState(PaddleVisualState state, float duration)
	{
		this.state = state;
		this.transientTimer = duration > 0f ? duration : this.transientStateDuration;
		ApplySprite();
	}

	private SpriteRenderer EnsureSpriteRenderer()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();

		if (renderer == null)
			renderer = gameObject.AddComponent<SpriteRenderer>();

		CopyLegacyRendererSettings(renderer);
		return renderer;
	}

	private void CopyLegacyRendererSettings(SpriteRenderer target)
	{
		foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
		{
			if (renderer == target)
				continue;

			target.sortingLayerID = renderer.sortingLayerID;
			target.sortingOrder = renderer.sortingOrder;
			target.sharedMaterial = renderer.sharedMaterial;
			return;
		}
	}

	private void SetLegacyChildRenderersEnabled(bool enabled)
	{
		foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
		{
			if (renderer == this.spriteRenderer)
				continue;

			renderer.enabled = enabled;
		}
	}

	private void LoadSprites(Func<string, Sprite> spriteLoader, Func<string, Texture2D> textureLoader)
	{
		this.spriteCache.Clear();
		CacheSprite(PaddleVisualState.Idle, false, spriteLoader, textureLoader);
		CacheSprite(PaddleVisualState.Idle, true, spriteLoader, textureLoader);
		CacheSprite(PaddleVisualState.Damaged, false, spriteLoader, textureLoader);
		CacheSprite(PaddleVisualState.Damaged, true, spriteLoader, textureLoader);
		CacheSprite(PaddleVisualState.GetItem, false, spriteLoader, textureLoader);
		CacheSprite(PaddleVisualState.GetItem, true, spriteLoader, textureLoader);
	}

	private void CacheSprite(
		PaddleVisualState state,
		bool isBig,
		Func<string, Sprite> spriteLoader,
		Func<string, Texture2D> textureLoader)
	{
		string key = GetSpriteKey(state, isBig);
		string path = SpritePathPrefix + key;
		Sprite sprite = spriteLoader?.Invoke(path);

		if (sprite == null)
			sprite = CreateSpriteFromTexture(path, textureLoader);

		if (sprite == null)
			Debug.LogError($"PaddleVisual: sprite not found at Resources/{path}");

		this.spriteCache[key] = sprite;
	}

	private void HandlePaddleSizeLevelChanged(int level)
	{
		ApplySize();
		ApplySprite();
	}

	private void ApplySize()
	{
		if (this.boxCollider == null || this.paddle == null || this.paddle.Stats == null)
			return;

		this.boxCollider.size = this.paddle.Stats.CurrentColliderSize;
		this.boxCollider.offset = Vector2.zero;
	}

	private void ApplySprite()
	{
		if (this.spriteRenderer == null)
			return;

		string key = GetSpriteKey(this.state, ShouldUseBigSprite());

		if (!this.spriteCache.TryGetValue(key, out Sprite sprite) || sprite == null)
		{
			SetLegacyChildRenderersEnabled(this.spriteRenderer.sprite == null);
			return;
		}

		this.spriteRenderer.sprite = sprite;
		this.spriteRenderer.enabled = true;
		SetLegacyChildRenderersEnabled(false);
	}

	private static Sprite CreateSpriteFromTexture(string path, Func<string, Texture2D> textureLoader)
	{
		Texture2D texture = textureLoader?.Invoke(path);

		if (texture == null)
			return null;

		Rect rect = new(0f, 0f, texture.width, texture.height);
		return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 700f);
	}

	private bool ShouldUseBigSprite()
	{
		return this.paddle != null &&
		       this.paddle.Stats != null &&
		       this.paddle.Stats.PaddleSizeLevel > 0;
	}

	private static string GetSpriteKey(PaddleVisualState state, bool isBig)
	{
		string stateName = state switch
		{
			PaddleVisualState.Damaged => "damaged",
			PaddleVisualState.GetItem => "getitem",
			_ => "idle"
		};
		string sizeName = isBig ? "big" : "small";
		return $"nimbus_{stateName}_{sizeName}";
	}
}
