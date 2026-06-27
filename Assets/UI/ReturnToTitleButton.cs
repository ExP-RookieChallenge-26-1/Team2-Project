using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ReturnToTitleButton : MonoBehaviour
{
    private const string TitleSceneName = "TitleScene";
    private const float ResultPanelCanvasCoverage = 0.6f;
    private const float ButtonLabelFontSizeMin = 13.5f;
    private const float ButtonLabelFontSizeMax = 24.75f;
    private const float TitleFontSizeMin = 125f;
    private const float TitleFontSizeMax = 260f;
    private const float ScoreFontSizeMin = 32f;
    private const float ScoreFontSizeMax = 78f;
    private const string ScoreTextObjectName = "ScoreText";

    private static readonly Color InkColor = new Color(0.16f, 0.1f, 0.08f);
    private static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 FallbackPanelSize = new Vector2(648f, 1152f);
    private static readonly Vector2 ButtonLabelPosition = new Vector2(0f, 0f);
    private static readonly Vector2 TitleTextPosition = new Vector2(0f, 180f);
    private static readonly Vector2 TitleTextSize = new Vector2(540f, 300f);
    private static readonly Vector2 ScoreTextPosition = new Vector2(0f, -40f);
    private static readonly Vector2 ScoreTextSize = new Vector2(540f, 100f);

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        ApplyResultPanelLayout();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveListener(ReturnToTitle);
        button.onClick.AddListener(ReturnToTitle);
        ApplyResultPanelLayout();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(ReturnToTitle);
    }

    private void ReturnToTitle()
    {
        GameManager.ResetSessionState();
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopBgm();
        SceneManager.LoadScene(TitleSceneName);
    }

    private void ApplyResultPanelLayout()
    {
        RectTransform buttonRect = transform as RectTransform;
        if (buttonRect == null)
            return;

        RectTransform panelRect = buttonRect.parent as RectTransform;
        if (panelRect == null)
            return;

        ApplyPanelFrameRect(panelRect);

        Image panelImage = panelRect.GetComponent<Image>();
        if (panelImage != null)
            panelImage.preserveAspect = true;

        Image buttonImage = GetComponent<Image>();
        if (buttonImage != null)
            buttonImage.preserveAspect = true;

        TextMeshProUGUI buttonLabel = GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonLabel != null)
        {
            buttonLabel.text = "타이틀 화면으로";
            buttonLabel.color = Color.white;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            buttonLabel.textWrappingMode = TextWrappingModes.NoWrap;
            buttonLabel.enableAutoSizing = true;
            buttonLabel.fontSize = ButtonLabelFontSizeMax;
            buttonLabel.fontSizeMin = ButtonLabelFontSizeMin;
            buttonLabel.fontSizeMax = ButtonLabelFontSizeMax;

            if (buttonLabel.transform is RectTransform labelRect)
                labelRect.anchoredPosition = ButtonLabelPosition;
        }

        Transform titleTransform = panelRect.Find("TitleText");
        TextMeshProUGUI titleText = titleTransform != null ? titleTransform.GetComponent<TextMeshProUGUI>() : null;
        if (titleTransform is RectTransform titleRect && titleText != null)
        {
            titleRect.anchoredPosition = TitleTextPosition;
            titleRect.sizeDelta = TitleTextSize;
            titleText.text = panelRect.name.Contains("Clear") ? "game\nclear" : "game\nover";
            titleText.color = InkColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSize = TitleFontSizeMax;
            titleText.fontSizeMin = TitleFontSizeMin;
            titleText.fontSizeMax = TitleFontSizeMax;
        }

        ApplyScoreText(panelRect);
    }

    private static void ApplyScoreText(RectTransform panelRect)
    {
        TextMeshProUGUI scoreText = GetOrCreateScoreText(panelRect);
        RectTransform scoreRect = scoreText.transform as RectTransform;

        scoreRect.anchorMin = CenterAnchor;
        scoreRect.anchorMax = CenterAnchor;
        scoreRect.anchoredPosition = ScoreTextPosition;
        scoreRect.sizeDelta = ScoreTextSize;
        scoreText.text = ScoreManager.FormatScoreText();
        scoreText.color = InkColor;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.enableAutoSizing = true;
        scoreText.fontSize = ScoreFontSizeMax;
        scoreText.fontSizeMin = ScoreFontSizeMin;
        scoreText.fontSizeMax = ScoreFontSizeMax;
        scoreText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static TextMeshProUGUI GetOrCreateScoreText(RectTransform panelRect)
    {
        Transform existing = panelRect.Find(ScoreTextObjectName);
        if (existing != null && existing.TryGetComponent(out TextMeshProUGUI existingText))
            return existingText;

        GameObject scoreObject = new GameObject(ScoreTextObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        scoreObject.transform.SetParent(panelRect, false);
        return scoreObject.GetComponent<TextMeshProUGUI>();
    }

    private static void ApplyPanelFrameRect(RectTransform panelRect)
    {
        panelRect.anchorMin = CenterAnchor;
        panelRect.anchorMax = CenterAnchor;
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = CalculatePanelSize(panelRect);
    }

    private static Vector2 CalculatePanelSize(RectTransform panelRect)
    {
        RectTransform canvasRect = FindParentCanvasRect(panelRect);
        if (canvasRect == null)
            return FallbackPanelSize;

        Rect rect = canvasRect.rect;
        if (rect.width <= 0f || rect.height <= 0f)
            return FallbackPanelSize;

        return new Vector2(rect.width * ResultPanelCanvasCoverage, rect.height * ResultPanelCanvasCoverage);
    }

    private static RectTransform FindParentCanvasRect(Transform transform)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.TryGetComponent(out Canvas _))
                return current as RectTransform;

            current = current.parent;
        }

        return null;
    }
}
