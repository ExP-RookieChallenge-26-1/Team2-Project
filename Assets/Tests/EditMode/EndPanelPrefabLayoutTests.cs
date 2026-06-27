#if UNITY_EDITOR
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class EndPanelPrefabLayoutTests
{
    private static readonly Vector3 PanelScale = new(1f, 1f, 1f);
    private static readonly Vector2 PanelAnchor = new(0.5f, 0.5f);
    private static readonly Vector2 PanelSize = new(648f, 1152f);
    private static readonly Vector2 TitlePosition = new(0f, 180f);
    private static readonly Vector2 TitleSize = new(540f, 300f);
    private static readonly Vector2 ButtonPosition = new(0f, -250f);
    private static readonly Vector2 ButtonSize = new(266.6667f, 100f);
    private static readonly Vector2 ButtonTextPosition = new(0f, 0f);
    private static readonly Vector2 ScoreTextPosition = new(0f, -40f);
    private static readonly Vector2 ScoreTextSize = new(540f, 100f);
    private static readonly Vector3 ButtonScale = new(1f, 1f, 1f);
    private const float ButtonPrefabFontSize = 56.25f;
    private const float RuntimeButtonFontSizeMin = 13.5f;
    private const float RuntimeButtonFontSizeMax = 24.75f;
    private const float TitleFontSizeMin = 125f;
    private const float TitleFontSizeMax = 260f;
    private const float ScoreFontSizeMin = 32f;
    private const float ScoreFontSizeMax = 78f;

    [Test]
    public void GameOverPanelUsesSixtyPercentPanelWithHalfSizeButton()
    {
        AssertEndPanelLayout("Assets/UI/GameOverPanel.prefab");
    }

    [Test]
    public void GameClearPanelUsesSixtyPercentPanelWithHalfSizeButton()
    {
        AssertEndPanelLayout("Assets/UI/GameClearPanel.prefab");
    }

    [Test]
    public void GameSceneEndPanelInstancesUseSixtyPercentCenteredPanelRects()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        AssertPanelRect(GetRect(canvas.transform, "GameOverPanel"));
        AssertPanelRect(GetRect(canvas.transform, "GameClearPanel"));
    }

    [TestCase("Assets/UI/GameOverPanel.prefab")]
    [TestCase("Assets/UI/GameClearPanel.prefab")]
    public void ReturnButtonSizesPanelToSixtyPercentCanvasAndKeepsChildrenWhenEnabled(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, prefabPath);

        GameObject canvasObject = null;
        GameObject instance = null;
        try
        {
            canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1080f, 1920f);

            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform);
            Assert.That(instance, Is.Not.Null);

            instance.SetActive(false);

            RectTransform panel = instance.GetComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = Vector2.zero;

            RectTransform title = GetRect(instance.transform, "TitleText");
            title.sizeDelta = new Vector2(300f, 50f);
            TMP_Text titleText = title.GetComponent<TMP_Text>();
            titleText.text = prefabPath.Contains("Clear") ? "Game Clear!" : "Game Over";
            titleText.fontSizeMin = 18f;
            titleText.fontSizeMax = 72f;

            RectSnapshot authoredButton = RectSnapshot.From(GetRect(instance.transform, "ExitButton"));
            RectTransform buttonTextRect = GetRect(instance.transform, "ExitButton/Text (TMP)");
            TMP_Text buttonText = buttonTextRect.GetComponent<TMP_Text>();
            buttonTextRect.anchoredPosition = Vector2.zero;
            buttonText.fontSizeMin = 12f;
            buttonText.fontSizeMax = 22f;
            buttonText.textWrappingMode = TextWrappingModes.Normal;

            instance.SetActive(true);

            AssertPanelRect(panel);
            AssertTitleText(title, titleText, prefabPath);
            AssertScoreText(GetRect(instance.transform, "ScoreText"));
            AssertButtonText(buttonTextRect, buttonText);
            Assert.That(RectSnapshot.From(GetRect(instance.transform, "ExitButton")), Is.EqualTo(authoredButton));
        }
        finally
        {
            if (instance != null)
                Object.DestroyImmediate(instance);
            if (canvasObject != null)
                Object.DestroyImmediate(canvasObject);
        }
    }

    [TestCase("Assets/UI/GameOverPanel.prefab")]
    [TestCase("Assets/UI/GameClearPanel.prefab")]
    public void ResultPanelShowsScoreBelowTitleWhenEnabled(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, prefabPath);

        GameObject instance = null;
        try
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Assert.That(instance, Is.Not.Null);

            instance.SetActive(false);
            instance.SetActive(true);

            AssertScoreText(GetRect(instance.transform, "ScoreText"));
        }
        finally
        {
            if (instance != null)
                Object.DestroyImmediate(instance);
        }
    }

    private static void AssertEndPanelLayout(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, prefabPath);

        RectTransform panel = prefab.GetComponent<RectTransform>();
        RectTransform title = GetRect(prefab.transform, "TitleText");
        RectTransform button = GetRect(prefab.transform, "ExitButton");
        RectTransform buttonTextRect = GetRect(button.transform, "Text (TMP)");
        TMP_Text titleText = title.GetComponent<TMP_Text>();
        TMP_Text buttonText = buttonTextRect.GetComponent<TMP_Text>();

        AssertPanelRect(panel);
        AssertVector(title.anchoredPosition, TitlePosition);
        AssertTitleText(title, titleText, prefabPath);
        AssertVector(button.anchoredPosition, ButtonPosition);
        AssertVector(button.sizeDelta, ButtonSize);
        AssertVector(button.localScale, ButtonScale);
        Assert.That(buttonText.fontSize, Is.EqualTo(ButtonPrefabFontSize).Within(0.001f));
        Assert.That(GetSerializedFloat(buttonText, "m_fontSizeBase"), Is.EqualTo(ButtonPrefabFontSize).Within(0.001f));
        AssertButtonText(buttonTextRect, buttonText);
    }

    private static void AssertPanelRect(RectTransform panel)
    {
        AssertVector(panel.anchorMin, PanelAnchor);
        AssertVector(panel.anchorMax, PanelAnchor);
        AssertVector(panel.anchoredPosition, Vector2.zero);
        AssertVector(panel.sizeDelta, PanelSize);
        AssertVector(panel.localScale, PanelScale);

        Image image = panel.GetComponent<Image>();
        Assert.That(image, Is.Not.Null);
        Assert.That(image.sprite, Is.Not.Null);
    }

    private static void AssertTitleText(RectTransform title, TMP_Text titleText, string prefabPath)
    {
        Assert.That(titleText, Is.Not.Null);
        AssertVector(title.sizeDelta, TitleSize);
        Assert.That(titleText.text, Is.EqualTo(ExpectedTitleText(prefabPath)));
        Assert.That(titleText.enableAutoSizing, Is.True);
        Assert.That(titleText.fontSizeMin, Is.EqualTo(TitleFontSizeMin).Within(0.001f));
        Assert.That(titleText.fontSizeMax, Is.EqualTo(TitleFontSizeMax).Within(0.001f));
    }

    private static void AssertButtonText(RectTransform buttonTextRect, TMP_Text buttonText)
    {
        AssertVector(buttonTextRect.anchoredPosition, ButtonTextPosition);
        Assert.That(buttonText.text, Is.EqualTo("타이틀 화면으로"));
        Assert.That(buttonText.textWrappingMode, Is.EqualTo(TextWrappingModes.NoWrap));
        Assert.That(buttonText.enableAutoSizing, Is.True);
        Assert.That(buttonText.fontSizeMin, Is.EqualTo(RuntimeButtonFontSizeMin).Within(0.001f));
        Assert.That(buttonText.fontSizeMax, Is.EqualTo(RuntimeButtonFontSizeMax).Within(0.001f));
    }

    private static void AssertScoreText(RectTransform scoreRect)
    {
        TMP_Text scoreText = scoreRect.GetComponent<TMP_Text>();
        Assert.That(scoreText, Is.Not.Null);
        AssertVector(scoreRect.anchoredPosition, ScoreTextPosition);
        AssertVector(scoreRect.sizeDelta, ScoreTextSize);
        Assert.That(scoreText.text, Is.EqualTo("점수: 0"));
        Assert.That(scoreText.alignment, Is.EqualTo(TextAlignmentOptions.Center));
        Assert.That(scoreText.enableAutoSizing, Is.True);
        Assert.That(scoreText.fontSizeMin, Is.EqualTo(ScoreFontSizeMin).Within(0.001f));
        Assert.That(scoreText.fontSizeMax, Is.EqualTo(ScoreFontSizeMax).Within(0.001f));
    }

    private static string ExpectedTitleText(string prefabPath)
    {
        return prefabPath.Contains("Clear") ? "game\nclear" : "game\nover";
    }

    private static RectTransform GetRect(Transform parent, string path)
    {
        Transform target = parent.Find(path);
        Assert.That(target, Is.Not.Null, path);

        RectTransform rect = target as RectTransform;
        Assert.That(rect, Is.Not.Null, path);
        return rect;
    }

    private static TMP_Text GetText(Transform parent, string path)
    {
        Transform target = parent.Find(path);
        Assert.That(target, Is.Not.Null, path);

        TMP_Text text = target.GetComponent<TMP_Text>();
        Assert.That(text, Is.Not.Null, path);
        return text;
    }

    private static float GetSerializedFloat(Object target, string propertyName)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        return property.floatValue;
    }

    private static void AssertVector(Vector2 actual, Vector2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
    }

    private static void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
    }

    private readonly struct RectSnapshot
    {
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 anchoredPosition;
        private readonly Vector2 sizeDelta;
        private readonly Vector3 localScale;

        private RectSnapshot(RectTransform rect)
        {
            this.anchorMin = rect.anchorMin;
            this.anchorMax = rect.anchorMax;
            this.anchoredPosition = rect.anchoredPosition;
            this.sizeDelta = rect.sizeDelta;
            this.localScale = rect.localScale;
        }

        public static RectSnapshot From(RectTransform rect)
        {
            Assert.That(rect, Is.Not.Null);
            return new RectSnapshot(rect);
        }

        public override bool Equals(object obj)
        {
            return obj is RectSnapshot other &&
                ApproximatelyEqual(this.anchorMin, other.anchorMin) &&
                ApproximatelyEqual(this.anchorMax, other.anchorMax) &&
                ApproximatelyEqual(this.anchoredPosition, other.anchoredPosition) &&
                ApproximatelyEqual(this.sizeDelta, other.sizeDelta) &&
                ApproximatelyEqual(this.localScale, other.localScale);
        }

        public override int GetHashCode()
        {
            return this.anchorMin.GetHashCode();
        }

        private static bool ApproximatelyEqual(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.001f &&
                Mathf.Abs(left.y - right.y) <= 0.001f;
        }

        private static bool ApproximatelyEqual(Vector3 left, Vector3 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.001f &&
                Mathf.Abs(left.y - right.y) <= 0.001f &&
                Mathf.Abs(left.z - right.z) <= 0.001f;
        }
    }
}
#endif
