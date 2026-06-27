using UnityEngine;

[DisallowMultipleComponent]
public class MobDamageOverlay : MonoBehaviour
{
	private const string OverlayObjectName = "DamageOverlay";

	[SerializeField] private Color overlayColor = new Color(1f, 0f, 0f, 1f);
	[SerializeField] private float fadeInDuration = 0.04f;
	[SerializeField] private float fadeOutDuration = 0.16f;
	[SerializeField] private int sortingOrderOffset = 1;

	private SpriteRenderer targetRenderer;
	private SpriteRenderer overlayRenderer;
	private float flashTime;
	private int playedFrame = -1;

	public bool IsFlashing { get; private set; }

	private void Awake()
	{
		this.targetRenderer = FindTargetRenderer();
	}

	private void Update()
	{
		Tick(Time.deltaTime);
	}

	public void Play()
	{
		EnsureOverlay();

		if (this.overlayRenderer == null)
			return;

		this.flashTime = 0f;
		this.playedFrame = Time.frameCount;
		this.IsFlashing = true;
		SyncOverlayRenderer();
		SetOverlayAlpha(this.overlayColor.a);
	}

	public void Tick(float deltaTime)
	{
		EnsureOverlay();

		if (this.overlayRenderer == null)
			return;

		SyncOverlayRenderer();

		if (!this.IsFlashing)
			return;

		if (Application.isPlaying && Time.frameCount == this.playedFrame)
			return;

		this.flashTime += Mathf.Max(0f, deltaTime);
		SetOverlayAlpha(EvaluateAlpha(this.flashTime));

		if (this.flashTime >= TotalDuration)
		{
			this.IsFlashing = false;
			SetOverlayAlpha(0f);
		}
	}

	private float TotalDuration => GetFadeInDuration() + GetFadeOutDuration();

	private float EvaluateAlpha(float elapsed)
	{
		float fadeIn = GetFadeInDuration();
		float fadeOut = GetFadeOutDuration();
		float peakAlpha = Mathf.Clamp01(this.overlayColor.a);

		if (elapsed <= fadeIn)
			return peakAlpha;

		float fadeOutElapsed = elapsed - fadeIn;
		if (fadeOutElapsed >= fadeOut)
			return 0f;

		return Mathf.Lerp(peakAlpha, 0f, fadeOutElapsed / fadeOut);
	}

	private float GetFadeInDuration()
	{
		return Mathf.Max(0.001f, this.fadeInDuration);
	}

	private float GetFadeOutDuration()
	{
		return Mathf.Max(0.001f, this.fadeOutDuration);
	}

	private void EnsureOverlay()
	{
		if (ShouldFindNewTarget())
			this.targetRenderer = FindTargetRenderer();

		if (this.targetRenderer == null)
			return;

		if (this.overlayRenderer == null)
		{
			Transform existingOverlay = transform.Find(OverlayObjectName);
			if (existingOverlay != null)
				this.overlayRenderer = existingOverlay.GetComponent<SpriteRenderer>();

			if (this.overlayRenderer == null)
			{
				GameObject overlayObject = new GameObject(OverlayObjectName);
				this.overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
			}
		}

		SyncOverlayTransform();
		SyncOverlayRenderer();
	}

	private SpriteRenderer FindTargetRenderer()
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (IsUsableTarget(renderer))
			return renderer;

		SpriteRenderer fallbackRenderer = null;
		foreach (SpriteRenderer childRenderer in GetComponentsInChildren<SpriteRenderer>(true))
		{
			if (childRenderer == this.overlayRenderer || childRenderer.name == OverlayObjectName)
				continue;

			if (IsUsableTarget(childRenderer))
				return childRenderer;

			fallbackRenderer ??= childRenderer;
		}

		return renderer != null && renderer != this.overlayRenderer ? renderer : fallbackRenderer;
	}

	private bool ShouldFindNewTarget()
	{
		return this.targetRenderer == null ||
		       this.targetRenderer == this.overlayRenderer ||
		       !this.targetRenderer.enabled ||
		       this.targetRenderer.sprite == null;
	}

	private bool IsUsableTarget(SpriteRenderer renderer)
	{
		return renderer != null &&
		       renderer != this.overlayRenderer &&
		       renderer.name != OverlayObjectName &&
		       renderer.enabled &&
		       renderer.sprite != null;
	}

	private void SyncOverlayRenderer()
	{
		if (ShouldFindNewTarget())
			this.targetRenderer = FindTargetRenderer();

		if (this.targetRenderer == null || this.overlayRenderer == null)
			return;

		this.overlayRenderer.sprite = this.targetRenderer.sprite;
		this.overlayRenderer.flipX = this.targetRenderer.flipX;
		this.overlayRenderer.flipY = this.targetRenderer.flipY;
		this.overlayRenderer.sortingLayerID = this.targetRenderer.sortingLayerID;
		this.overlayRenderer.sortingOrder = this.targetRenderer.sortingOrder + this.sortingOrderOffset;
		this.overlayRenderer.maskInteraction = this.targetRenderer.maskInteraction;
		this.overlayRenderer.drawMode = this.targetRenderer.drawMode;
		this.overlayRenderer.size = this.targetRenderer.size;
	}

	private void SyncOverlayTransform()
	{
		if (this.targetRenderer == null || this.overlayRenderer == null)
			return;

		Transform overlayTransform = this.overlayRenderer.transform;
		if (overlayTransform.parent != transform)
			overlayTransform.SetParent(transform, false);

		Transform targetTransform = this.targetRenderer.transform;
		if (targetTransform == transform)
		{
			overlayTransform.localPosition = Vector3.zero;
			overlayTransform.localRotation = Quaternion.identity;
			overlayTransform.localScale = Vector3.one;
			return;
		}

		overlayTransform.localPosition = targetTransform.localPosition;
		overlayTransform.localRotation = targetTransform.localRotation;
		overlayTransform.localScale = targetTransform.localScale;
	}

	private void SetOverlayAlpha(float alpha)
	{
		if (this.overlayRenderer == null)
			return;

		Color color = this.overlayColor;
		color.a = Mathf.Clamp01(alpha);
		this.overlayRenderer.color = color;
		this.overlayRenderer.enabled = this.targetRenderer != null && this.targetRenderer.enabled && color.a > 0f;
	}
}
