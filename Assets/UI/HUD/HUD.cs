using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	private const string RuntimeSkillButtonsName = "RuntimeSkillButtons";
	private const string LevelTextName = "LevelText";
	private static readonly Vector2 SkillButtonSize = new(120f, 120f);
	private static readonly Vector2 SkillButtonAnchor = new(1f, 0.5f);
	private static readonly Vector2 Skill1Position = new(-70f, -490f);
	private static readonly Vector2 Skill2Position = new(-70f, -350f);
	private static readonly Vector2 AutoOverlaySize = new(80f, 80f);
	private static readonly Vector2 CloneAutoOverlayPosition = new(115f, 72f);
	private static readonly Vector2 GiantAutoOverlayPosition = new(212f, 74f);
	private static readonly Vector2 LockIconSize = new(42f, 46f);
	private static readonly Vector2 LevelTextPosition = new(-144f, 84f);
	private static readonly Vector2 LevelTextSize = new(170f, 54f);
	private static readonly Color32 LevelTextColor = new(103, 48, 12, 255);

	[SerializeField] private Button skill1Button;
	[SerializeField] private Button skill2Button;
	[SerializeField] private SkillEventChannel skillEventChannel;

	private TextMeshProUGUI levelText;
	private UserLevel observedLevel;

	private enum CooldownOverlaySkill
	{
		Clone,
		Giant
	}

	private void Awake()
	{
		RebuildRuntimeSkillButtons();
	}

	private void Start()
	{
		if (this.skill1Button != null)
			this.skill1Button.onClick.AddListener(OnClickSkill1);

		if (this.skill2Button != null)
			this.skill2Button.onClick.AddListener(OnClickSkill2);

		EnsureLevelText();
		ObserveLevel(FindFirstObjectByType<UserLevel>());
	}

	private void OnDestroy()
	{
		if (this.skill1Button != null)
			this.skill1Button.onClick.RemoveListener(OnClickSkill1);

		if (this.skill2Button != null)
			this.skill2Button.onClick.RemoveListener(OnClickSkill2);

		ObserveLevel(null);
	}

	private void RebuildRuntimeSkillButtons()
	{
		Button serializedSkill1Button = this.skill1Button;
		Button serializedSkill2Button = this.skill2Button;

		if (serializedSkill1Button != null && serializedSkill2Button != null)
		{
			serializedSkill1Button.gameObject.SetActive(true);
			serializedSkill2Button.gameObject.SetActive(true);

			Transform runtimeContainer = transform.Find(RuntimeSkillButtonsName);
			if (runtimeContainer != null)
				DestroyUnityObject(runtimeContainer.gameObject);

			EnsureCooldownOverlays();
			EnsureLevelText();
			return;
		}

		Sprite skill1Sprite = GetButtonSprite(serializedSkill1Button);
		Sprite skill2Sprite = GetButtonSprite(serializedSkill2Button);

		if (serializedSkill1Button != null)
			serializedSkill1Button.gameObject.SetActive(false);

		if (serializedSkill2Button != null)
			serializedSkill2Button.gameObject.SetActive(false);

		Transform existing = transform.Find(RuntimeSkillButtonsName);
		if (existing != null)
			DestroyUnityObject(existing.gameObject);

		RectTransform container = CreateRectObject(RuntimeSkillButtonsName, transform);
		container.anchorMin = Vector2.zero;
		container.anchorMax = Vector2.one;
		container.anchoredPosition = Vector2.zero;
		container.sizeDelta = Vector2.zero;
		container.SetAsLastSibling();

		this.skill1Button = CreateRuntimeSkillButton(container, "Skill1Button", Skill1Position, skill1Sprite);
		this.skill2Button = CreateRuntimeSkillButton(container, "Skill2Button", Skill2Position, skill2Sprite);
		EnsureCooldownOverlays();
		EnsureLevelText();
	}

	private void EnsureLevelText()
	{
		RectTransform frame = FindFrameRect();
		if (frame == null)
			return;

		RectTransform textRect = frame.Find(LevelTextName) as RectTransform;
		if (textRect == null)
			textRect = CreateRectObject(LevelTextName, frame);

		textRect.anchorMin = new Vector2(0.5f, 0.5f);
		textRect.anchorMax = new Vector2(0.5f, 0.5f);
		textRect.pivot = new Vector2(0.5f, 0.5f);
		textRect.anchoredPosition = LevelTextPosition;
		textRect.sizeDelta = LevelTextSize;
		textRect.SetAsLastSibling();

		this.levelText = textRect.GetComponent<TextMeshProUGUI>();
		if (this.levelText == null)
			this.levelText = textRect.gameObject.AddComponent<TextMeshProUGUI>();

		this.levelText.alignment = TextAlignmentOptions.Center;
		this.levelText.enableAutoSizing = true;
		this.levelText.fontSizeMin = 18f;
		this.levelText.fontSizeMax = 42f;
		this.levelText.fontSize = 38f;
		this.levelText.fontStyle = FontStyles.Bold;
		this.levelText.color = LevelTextColor;
		this.levelText.raycastTarget = false;
		RefreshLevelText();
	}

	private void ObserveLevel(UserLevel level)
	{
		if (this.observedLevel == level)
			return;

		if (this.observedLevel != null)
			this.observedLevel.OnExpChanged -= HandleLevelExpChanged;

		this.observedLevel = level;

		if (this.observedLevel != null)
			this.observedLevel.OnExpChanged += HandleLevelExpChanged;

		RefreshLevelText();
	}

	private void HandleLevelExpChanged(int currentExp, int requiredExp)
	{
		RefreshLevelText();
	}

	private void RefreshLevelText()
	{
		if (this.levelText == null)
			return;

		UserLevel level = this.observedLevel != null
			? this.observedLevel
			: FindFirstObjectByType<UserLevel>();
		int currentLevel = level != null ? level.CurrentLevel : 1;
		this.levelText.text = $"Lv. {currentLevel}";
	}

	private void EnsureCooldownOverlays()
	{
		RectTransform frame = FindFrameRect();

		if (frame != null)
		{
			CreateCooldownOverlay(
				frame,
				"AutoCooldownOverlayClone",
				CloneAutoOverlayPosition,
				AutoOverlaySize,
				() => GetCooldownState(CooldownOverlaySkill.Clone, CooldownOverlayMode.Auto));
			CreateCooldownOverlay(
				frame,
				"AutoCooldownOverlayGiant",
				GiantAutoOverlayPosition,
				AutoOverlaySize,
				() => GetCooldownState(CooldownOverlaySkill.Giant, CooldownOverlayMode.Auto));
		}

		if (this.skill1Button != null && this.skill1Button.transform is RectTransform skill1Rect)
		{
			CreateCooldownOverlay(
				skill1Rect,
				"ManualCooldownOverlayGiant",
				Vector2.zero,
				Vector2.zero,
				() => GetCooldownState(CooldownOverlaySkill.Giant, CooldownOverlayMode.Manual));
		}

		if (this.skill2Button != null && this.skill2Button.transform is RectTransform skill2Rect)
		{
			CreateCooldownOverlay(
				skill2Rect,
				"ManualCooldownOverlayClone",
				Vector2.zero,
				Vector2.zero,
				() => GetCooldownState(CooldownOverlaySkill.Clone, CooldownOverlayMode.Manual));
		}
	}

	private RectTransform FindFrameRect()
	{
		Transform frame = transform.Find("PlayerStatus/Frame");
		if (frame == null)
			frame = transform.Find("Frame");

		return frame as RectTransform;
	}

	private CooldownOverlayState GetCooldownState(CooldownOverlaySkill skill, CooldownOverlayMode mode)
	{
		Ball observedBall = FindObservedBall();
		if (observedBall == null)
			return new CooldownOverlayState(false, 0f, 0f);

		BallSkill targetSkill = skill == CooldownOverlaySkill.Clone
			? observedBall.CloneSkill
			: observedBall.GiantSkill;
		return CooldownOverlayState.FromSkill(targetSkill, mode);
	}

	private static Ball FindObservedBall()
	{
		Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
		if (balls == null || balls.Length == 0)
			return null;

		Ball observed = balls[0];
		for (int i = 1; i < balls.Length; i++)
		{
			if (balls[i] != null && balls[i].GetInstanceID() < observed.GetInstanceID())
				observed = balls[i];
		}

		return observed;
	}

	private static RectTransform CreateCooldownOverlay(
		RectTransform parent,
		string name,
		Vector2 anchoredPosition,
		Vector2 size,
		System.Func<CooldownOverlayState> stateProvider)
	{
		DestroyExistingChild(parent, name);

		GameObject overlayObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CooldownOverlayWidget));
		RectTransform rect = overlayObject.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		rect.localScale = Vector3.one;
		rect.pivot = new Vector2(0.5f, 0.5f);

		if (size == Vector2.zero)
		{
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.anchoredPosition = Vector2.zero;
			rect.sizeDelta = Vector2.zero;
		}
		else
		{
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;
		}

		rect.SetAsLastSibling();

		Image overlayImage = overlayObject.GetComponent<Image>();
		TextMeshProUGUI cooldownText = CreateCooldownText(rect);
		LockIconGraphic lockIcon = CreateLockIcon(rect);
		CooldownOverlayWidget widget = overlayObject.GetComponent<CooldownOverlayWidget>();
		widget.ConfigureVisuals(overlayImage, cooldownText, lockIcon);
		widget.SetStateProvider(stateProvider);
		return rect;
	}

	private static TextMeshProUGUI CreateCooldownText(RectTransform parent)
	{
		RectTransform textRect = CreateRectObject("CooldownText", parent);
		textRect.anchorMin = Vector2.zero;
		textRect.anchorMax = Vector2.one;
		textRect.anchoredPosition = Vector2.zero;
		textRect.sizeDelta = Vector2.zero;

		TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
		text.alignment = TextAlignmentOptions.Center;
		text.enableAutoSizing = true;
		text.fontSizeMin = 12f;
		text.fontSizeMax = 32f;
		text.fontSize = 28f;
		text.fontStyle = FontStyles.Bold;
		text.color = Color.white;
		text.raycastTarget = false;
		return text;
	}

	private static LockIconGraphic CreateLockIcon(RectTransform parent)
	{
		RectTransform lockRect = CreateRectObject("LockIcon", parent);
		lockRect.anchorMin = new Vector2(0.5f, 0.5f);
		lockRect.anchorMax = new Vector2(0.5f, 0.5f);
		lockRect.pivot = new Vector2(0.5f, 0.5f);
		lockRect.anchoredPosition = Vector2.zero;
		lockRect.sizeDelta = LockIconSize;

		LockIconGraphic lockIcon = lockRect.gameObject.AddComponent<LockIconGraphic>();
		lockIcon.color = Color.white;
		lockIcon.raycastTarget = false;
		return lockIcon;
	}

	private static void DestroyExistingChild(Transform parent, string name)
	{
		Transform existing = parent.Find(name);
		if (existing != null)
			DestroyUnityObject(existing.gameObject);
	}

	private static Button CreateRuntimeSkillButton(
		RectTransform parent,
		string name,
		Vector2 anchoredPosition,
		Sprite sprite)
	{
		RectTransform rect = CreateRectObject(name, parent);
		rect.anchorMin = SkillButtonAnchor;
		rect.anchorMax = SkillButtonAnchor;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = SkillButtonSize;

		Image image = rect.gameObject.AddComponent<Image>();
		image.sprite = sprite;
		image.preserveAspect = true;
		image.raycastTarget = true;

		Button button = rect.gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		return button;
	}

	private static RectTransform CreateRectObject(string name, Transform parent)
	{
		GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer));
		RectTransform rect = gameObject.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		rect.localScale = Vector3.one;
		return rect;
	}

	private static Sprite GetButtonSprite(Button button)
	{
		return button != null && button.image != null
			? button.image.sprite
			: null;
	}

	private static void DestroyUnityObject(Object unityObject)
	{
		if (Application.isPlaying)
			Destroy(unityObject);
		else
			DestroyImmediate(unityObject);
	}

	private void OnClickSkill1()
	{
		this.skillEventChannel?.RaiseSkill1();
	}

	private void OnClickSkill2()
	{
		this.skillEventChannel?.RaiseSkill2();
	}
}
