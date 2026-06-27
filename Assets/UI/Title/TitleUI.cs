using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class TitleUI : MonoBehaviour, IPointerClickHandler
{
    private const string StartPromptText = "touch to start";
    private const string BackgroundResourcePath = "TitleSceneArtwork/background";
    private const string LogoResourcePath = "TitleSceneArtwork/logo";
    private const string SettingButtonSpriteResourcePath = "UI/setting-button-generated";
    private const float InvisibleClickAreaAlpha = 0.01f;
    private const float StartPromptPulseAmplitude = 0.06f;
    private const float StartPromptPulseCyclesPerSecond = 0.9f;
    private static readonly Color SettingButtonColor = Color.white;
    private static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 TopLeftAnchor = new Vector2(0f, 1f);
    private static readonly Vector2 TopRightAnchor = new Vector2(1f, 1f);
    private static readonly Vector2 SettingButtonOffset = new Vector2(-112f, -112f);
    private static readonly Vector2 SettingButtonSize = new Vector2(176f, 176f);

    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button creditButton;

    [SerializeField] private SettingPanel settingPanel;
    [SerializeField] private CreditPanel creditPanel;

    private bool isStarting;
    private Sprite runtimeBackgroundSprite;
    private Sprite runtimeLogoSprite;
    private Sprite runtimeSettingButtonSprite;
    private RectTransform startLabelRect;

    private void Awake()
    {
        ApplyTitleLayout();
    }

    private void OnEnable()
    {
        ApplyTitleLayout();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += ApplyTitleLayoutIfAlive;
#endif
    }

#if UNITY_EDITOR
    private void ApplyTitleLayoutIfAlive()
    {
        if (this != null)
            ApplyTitleLayout();
    }
#endif

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);
        if (creditButton != null)
            creditButton.onClick.AddListener(OnCreditClicked);
    }

    private void Update()
    {
        if (Application.isPlaying)
            ApplyStartPromptPulse(Time.unscaledTime * StartPromptPulseCyclesPerSecond);
    }

    private void OnStartClicked()
    {
        if (isStarting)
            return;

        isStarting = true;

        GameManager.ResetSessionState();
        if (GameManager.Instance != null)
            Object.Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("GameScene");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnStartClicked();
    }

    private void OnSettingClicked()
    {
        HideTitleUI();
        if (settingPanel != null)
            settingPanel.gameObject.SetActive(true);
    }

    private void OnCreditClicked()
    {
        HideTitleUI();
        if (creditPanel != null)
            creditPanel.ShowFrom(gameObject);
    }

    public void ShowTitleUI()
    {
        gameObject.SetActive(true);
        if (settingButton != null)
            settingButton.gameObject.SetActive(true);
        ApplyTitleLayout();
    }

    public void HideTitleUI()
    {
        if (settingButton != null)
            settingButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ApplyTitleLayout()
    {
        ApplyTitleArtwork();
        ConfigureFullScreenTapArea();

        ConfigureStartButton();
        ConfigureSettingButton();

        if (creditButton != null)
            creditButton.gameObject.SetActive(false);
    }

    private void ApplyTitleArtwork()
    {
        Image backgroundImage = EnsureBackgroundImage();
        if (backgroundImage != null)
        {
            backgroundImage.sprite = LoadResourceSprite(BackgroundResourcePath, ref runtimeBackgroundSprite);
            backgroundImage.color = Color.white;
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = false;
            backgroundImage.raycastTarget = false;
        }

        Image logoImage = EnsureLogoImage();
        if (logoImage != null)
        {
            logoImage.sprite = LoadResourceSprite(LogoResourcePath, ref runtimeLogoSprite);
            logoImage.color = Color.white;
            logoImage.type = Image.Type.Simple;
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
        }
    }

    private Image EnsureBackgroundImage()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return null;

        Transform backgroundTransform = canvas.transform.Find("TitleImage");
        if (backgroundTransform == null)
        {
            GameObject backgroundObject = new GameObject("TitleImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundObject.layer = gameObject.layer;
            backgroundObject.transform.SetParent(canvas.transform, false);
            backgroundObject.transform.SetSiblingIndex(0);
            backgroundTransform = backgroundObject.transform;
        }

        if (backgroundTransform is RectTransform backgroundRect)
        {
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = CenterAnchor;
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = Vector2.zero;
            backgroundRect.localScale = Vector3.one;
            backgroundRect.localRotation = Quaternion.identity;
        }

        Image image = backgroundTransform.GetComponent<Image>();
        return image != null ? image : backgroundTransform.gameObject.AddComponent<Image>();
    }

    private Image EnsureLogoImage()
    {
        Transform logoTransform = transform.Find("TitleLogo");
        if (logoTransform == null)
        {
            GameObject logoObject = new GameObject("TitleLogo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logoObject.layer = gameObject.layer;
            logoObject.transform.SetParent(transform, false);
            logoObject.transform.SetSiblingIndex(0);
            logoTransform = logoObject.transform;
        }

        if (logoTransform is RectTransform logoRect)
        {
            logoRect.anchorMin = TopLeftAnchor;
            logoRect.anchorMax = TopLeftAnchor;
            logoRect.pivot = TopLeftAnchor;
            logoRect.anchoredPosition = new Vector2(36f, -36f);
            logoRect.sizeDelta = new Vector2(312f, 637f);
            logoRect.localScale = Vector3.one;
            logoRect.localRotation = Quaternion.identity;
        }

        Image image = logoTransform.GetComponent<Image>();
        return image != null ? image : logoTransform.gameObject.AddComponent<Image>();
    }

    private static Sprite LoadResourceSprite(string resourcePath, ref Sprite cachedSprite)
    {
        if (cachedSprite != null)
            return cachedSprite;

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture != null)
        {
            cachedSprite = CreateFullTextureSprite(texture);
            return cachedSprite;
        }

        foreach (Sprite sprite in Resources.LoadAll<Sprite>(resourcePath))
        {
            cachedSprite = Sprite.Create(
                sprite.texture,
                new Rect(0f, 0f, sprite.texture.width, sprite.texture.height),
                CenterAnchor,
                100f);
            cachedSprite.name = sprite.texture.name;
            return cachedSprite;
        }

        Debug.LogError($"Title artwork resource not found: Resources/{resourcePath}");
        return null;
    }

    private static Sprite CreateFullTextureSprite(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            CenterAnchor,
            100f);
        sprite.name = texture.name;
        return sprite;
    }

    private void ConfigureFullScreenTapArea()
    {
        Image tapArea = GetComponent<Image>();
        if (tapArea == null)
            return;

        Color color = tapArea.color;
        color.a = InvisibleClickAreaAlpha;
        tapArea.color = color;
        tapArea.raycastTarget = true;
        tapArea.canvasRenderer.cullTransparentMesh = false;
    }

    private void ConfigureStartButton()
    {
        if (startButton == null)
            return;

        if (startButton.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = CenterAnchor;
            rectTransform.anchorMax = CenterAnchor;
            rectTransform.pivot = CenterAnchor;
            rectTransform.anchoredPosition = new Vector2(0f, -520f);
            rectTransform.sizeDelta = new Vector2(520f, 120f);
        }

        TextMeshProUGUI label = startButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = StartPromptText;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20f;
            label.fontSizeMax = 42f;
            label.color = Color.white;
            label.raycastTarget = false;
            startButton.targetGraphic = label;
            startLabelRect = label.transform as RectTransform;
        }

        Image image = startButton.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = InvisibleClickAreaAlpha;
            image.color = color;
            image.raycastTarget = true;
            image.canvasRenderer.cullTransparentMesh = false;
        }
    }

    private void ConfigureSettingButton()
    {
        if (settingButton == null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && settingButton.transform.parent != canvas.transform)
            settingButton.transform.SetParent(canvas.transform, false);

        if (settingButton.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = TopRightAnchor;
            rectTransform.anchorMax = TopRightAnchor;
            rectTransform.pivot = CenterAnchor;
            rectTransform.anchoredPosition = SettingButtonOffset;
            rectTransform.sizeDelta = SettingButtonSize;
        }

        settingButton.transform.SetAsLastSibling();

        Image image = settingButton.GetComponent<Image>();
        if (image != null)
        {
            Sprite settingSprite = LoadResourceSprite(SettingButtonSpriteResourcePath, ref runtimeSettingButtonSprite);
            if (settingSprite != null)
                image.sprite = settingSprite;

            image.preserveAspect = true;
            image.raycastTarget = true;
            image.color = SettingButtonColor;
            settingButton.targetGraphic = image;
        }

        settingButton.transition = Selectable.Transition.ColorTint;

        TextMeshProUGUI label = settingButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = string.Empty;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 36f;
            label.raycastTarget = false;
        }
    }

    private void ApplyStartPromptPulse(float cycleTime)
    {
        if (startLabelRect == null && startButton != null)
        {
            TextMeshProUGUI label = startButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                startLabelRect = label.transform as RectTransform;
        }

        if (startLabelRect == null)
            return;

        float scale = 1f + Mathf.Sin(cycleTime * Mathf.PI * 2f) * StartPromptPulseAmplitude;
        startLabelRect.localScale = new Vector3(scale, scale, 1f);
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
        if (settingButton != null)
            settingButton.onClick.RemoveListener(OnSettingClicked);
        if (creditButton != null)
            creditButton.onClick.RemoveListener(OnCreditClicked);

        DestroyRuntimeObject(this.runtimeBackgroundSprite);
        DestroyRuntimeObject(this.runtimeLogoSprite);
        DestroyRuntimeObject(this.runtimeSettingButtonSprite);
        this.runtimeBackgroundSprite = null;
        this.runtimeLogoSprite = null;
        this.runtimeSettingButtonSprite = null;
    }

    private static void DestroyRuntimeObject(Object target)
    {
        if (target == null)
            return;

        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }
}
