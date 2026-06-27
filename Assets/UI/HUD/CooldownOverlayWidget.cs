using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class CooldownOverlayWidget : MonoBehaviour
{
	private const int CircleTextureSize = 64;
	private static Sprite circleSprite;

	[SerializeField] private Image overlayImage;
	[SerializeField] private TextMeshProUGUI cooldownText;
	[SerializeField] private LockIconGraphic lockIcon;

	private Func<CooldownOverlayState> stateProvider;

	private void Awake()
	{
		if (this.overlayImage == null)
			this.overlayImage = GetComponent<Image>();

		ConfigureOverlayImage();
	}

	private void OnEnable()
	{
		RenderCurrentState();
	}

	private void Update()
	{
		RenderCurrentState();
	}

	public void ConfigureVisuals(Image overlay, TextMeshProUGUI label, LockIconGraphic lockGraphic)
	{
		this.overlayImage = overlay;
		this.cooldownText = label;
		this.lockIcon = lockGraphic;
		ConfigureOverlayImage();
	}

	public void SetStateProvider(Func<CooldownOverlayState> provider)
	{
		this.stateProvider = provider;
		RenderCurrentState();
	}

	public void Render(CooldownOverlayState state)
	{
		ConfigureOverlayImage();

		if (!state.IsAcquired)
		{
			SetEnabled(this.overlayImage, true);
			SetActive(this.cooldownText, false);
			SetActive(this.lockIcon, true);

			if (this.overlayImage != null)
				this.overlayImage.fillAmount = 1f;

			return;
		}

		if (!state.IsCoolingDown)
		{
			SetEnabled(this.overlayImage, false);
			SetActive(this.cooldownText, false);
			SetActive(this.lockIcon, false);
			return;
		}

		SetEnabled(this.overlayImage, true);
		SetActive(this.cooldownText, true);
		SetActive(this.lockIcon, false);

		if (this.overlayImage != null)
			this.overlayImage.fillAmount = state.CooldownRatio;

		if (this.cooldownText != null)
			this.cooldownText.text = Mathf.CeilToInt(state.RemainingSeconds).ToString();
	}

	private void RenderCurrentState()
	{
		if (this.stateProvider == null)
			return;

		Render(this.stateProvider());
	}

	private void ConfigureOverlayImage()
	{
		if (this.overlayImage == null)
			return;

		this.overlayImage.sprite = GetCircleSprite();
		this.overlayImage.color = new Color(0f, 0f, 0f, 0.58f);
		this.overlayImage.raycastTarget = false;
		this.overlayImage.type = Image.Type.Filled;
		this.overlayImage.fillMethod = Image.FillMethod.Radial360;
		this.overlayImage.fillOrigin = (int)Image.Origin360.Top;
		this.overlayImage.fillClockwise = true;
		this.overlayImage.preserveAspect = true;
	}

	private static Sprite GetCircleSprite()
	{
		if (circleSprite != null)
			return circleSprite;

		Texture2D texture = new(CircleTextureSize, CircleTextureSize, TextureFormat.RGBA32, false)
		{
			name = "CooldownOverlayCircle",
			hideFlags = HideFlags.HideAndDontSave,
			filterMode = FilterMode.Bilinear,
			wrapMode = TextureWrapMode.Clamp
		};

		float center = (CircleTextureSize - 1) * 0.5f;
		float radius = center;
		Color32 clear = new(255, 255, 255, 0);
		Color32 white = new(255, 255, 255, 255);

		for (int y = 0; y < CircleTextureSize; y++)
		{
			for (int x = 0; x < CircleTextureSize; x++)
			{
				float dx = x - center;
				float dy = y - center;
				texture.SetPixel(x, y, dx * dx + dy * dy <= radius * radius ? white : clear);
			}
		}

		texture.Apply();
		circleSprite = Sprite.Create(
			texture,
			new Rect(0f, 0f, CircleTextureSize, CircleTextureSize),
			new Vector2(0.5f, 0.5f),
			CircleTextureSize);
		circleSprite.name = "CooldownOverlayCircle";
		circleSprite.hideFlags = HideFlags.HideAndDontSave;
		return circleSprite;
	}

	private static void SetEnabled(Behaviour behaviour, bool enabled)
	{
		if (behaviour != null)
			behaviour.enabled = enabled;
	}

	private static void SetActive(Behaviour behaviour, bool active)
	{
		if (behaviour != null)
			behaviour.gameObject.SetActive(active);
	}
}
