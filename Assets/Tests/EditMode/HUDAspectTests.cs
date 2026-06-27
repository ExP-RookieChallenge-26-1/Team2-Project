#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class HUDAspectTests
{
    private const float SkillX = -70f;
    private const float Skill1Y = -490f;
    private const float Skill2Y = -350f;
    private static readonly Vector2 LevelTextPosition = new(-144f, 84f);
    private static readonly Vector2 LevelTextSize = new(170f, 54f);

    [Test]
    public void GameSceneHudFrameKeepsSpriteAspectRatio()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        Transform frameTransform = canvas.transform.Find("HUD/PlayerStatus/Frame");
        Assert.That(frameTransform, Is.Not.Null);

        RectTransform frameRect = frameTransform as RectTransform;
        Image frameImage = frameTransform.GetComponent<Image>();
        Assert.That(frameRect, Is.Not.Null);
        Assert.That(frameImage, Is.Not.Null);
        Assert.That(frameImage.sprite, Is.Not.Null);

        float spriteAspect = frameImage.sprite.rect.width / frameImage.sprite.rect.height;
        float rectAspect = frameRect.rect.width / frameRect.rect.height;

        Assert.That(frameImage.preserveAspect, Is.True);
        Assert.That(rectAspect, Is.EqualTo(spriteAspect).Within(0.001f));
    }

    [Test]
    public void GameSceneHudBarBackgroundUsesResolvedSpriteAndAlignsWithFrameSource()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        RectTransform frame = GetRect(canvas.transform, "HUD/PlayerStatus/Frame");
        RectTransform barBackground = GetRect(canvas.transform, "HUD/PlayerStatus/BarBackground");
        Image frameImage = frame.GetComponent<Image>();
        Image barBackgroundImage = barBackground.GetComponent<Image>();
        Assert.That(frameImage, Is.Not.Null);
        Assert.That(barBackgroundImage, Is.Not.Null);
        Assert.That(frameImage.sprite, Is.Not.Null);
        Assert.That(barBackgroundImage.sprite, Is.Not.Null);

        AssertTopCenterAnchor(barBackground);

        float sourceScale = frame.rect.width / frameImage.sprite.rect.width;
        float frameSourceCenterY = frameImage.sprite.rect.y + frameImage.sprite.rect.height * 0.5f;
        float backgroundSourceCenterY =
            barBackgroundImage.sprite.rect.y + barBackgroundImage.sprite.rect.height * 0.5f;
        Vector2 expectedSize = new(
            barBackgroundImage.sprite.rect.width * sourceScale,
            barBackgroundImage.sprite.rect.height * sourceScale);
        Vector2 expectedPosition = new(
            frame.anchoredPosition.x,
            frame.anchoredPosition.y + (backgroundSourceCenterY - frameSourceCenterY) * sourceScale);

        AssertSize(barBackground, expectedSize);
        AssertVector(barBackground.anchoredPosition, expectedPosition);
    }

    [Test]
    public void GameSceneHudKeepsRightSideSkillButtonsVisible()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        HUD hud = Object.FindFirstObjectByType<HUD>();
        Assert.That(hud, Is.Not.Null);
        InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

        RectTransform frame = GetRect(canvas.transform, "HUD/PlayerStatus/Frame");
        AssertTopCenterAnchor(frame);
        AssertVector(frame.anchoredPosition, new Vector2(0f, -200f));

        RectTransform skill1Button = GetRect(canvas.transform, "HUD/PlayerStatus/Skill1Button");
        RectTransform skill2Button = GetRect(canvas.transform, "HUD/PlayerStatus/Skill2Button");
        Assert.That(skill1Button.gameObject.activeInHierarchy, Is.True);
        Assert.That(skill2Button.gameObject.activeInHierarchy, Is.True);
        AssertRightCenterAnchor(skill1Button);
        AssertRightCenterAnchor(skill2Button);
        AssertVector(skill1Button.anchoredPosition, new Vector2(SkillX, Skill1Y));
        AssertVector(skill2Button.anchoredPosition, new Vector2(SkillX, Skill2Y));
        AssertSize(skill1Button, new Vector2(120f, 120f));
        AssertSize(skill2Button, new Vector2(120f, 120f));
    }

    [Test]
    public void GameSceneHudCreatesLevelTextInFrameSlot()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        HUD hud = Object.FindFirstObjectByType<HUD>();
        Assert.That(hud, Is.Not.Null);
        InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

        RectTransform levelTextRect = GetRect(canvas.transform, "HUD/PlayerStatus/Frame/LevelText");
        TextMeshProUGUI levelText = levelTextRect.GetComponent<TextMeshProUGUI>();
        Assert.That(levelText, Is.Not.Null);
        AssertCenterAnchor(levelTextRect);
        AssertVector(levelTextRect.anchoredPosition, LevelTextPosition);
        AssertSize(levelTextRect, LevelTextSize);
        Assert.That(levelText.alignment, Is.EqualTo(TextAlignmentOptions.Center));
        Assert.That(levelText.text, Is.EqualTo("Lv. 1"));
    }

    [Test]
    public void HudLevelTextDisplaysObservedUserLevel()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject userObject = new("User");
        GameObject hudObject = new("HUD", typeof(RectTransform));
        try
        {
            UserLevel userLevel = userObject.AddComponent<UserLevel>();
            SetPrivateField(userLevel, "currentLevel", 12);

            RectTransform frame = CreateRect("Frame", hudObject.transform);
            HUD hud = hudObject.AddComponent<HUD>();
            InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

            TextMeshProUGUI levelText = frame.Find("LevelText").GetComponent<TextMeshProUGUI>();
            Assert.That(levelText.text, Is.EqualTo("Lv. 12"));
        }
        finally
        {
            Object.DestroyImmediate(hudObject);
            Object.DestroyImmediate(userObject);
        }
    }

    private static RectTransform GetRect(Transform parent, string path)
    {
        Transform target = parent.Find(path);
        Assert.That(target, Is.Not.Null, path);

        RectTransform rect = target as RectTransform;
        Assert.That(rect, Is.Not.Null, path);
        return rect;
    }

    private static void AssertTopCenterAnchor(RectTransform rect)
    {
        AssertVector(rect.anchorMin, new Vector2(0.5f, 1f));
        AssertVector(rect.anchorMax, new Vector2(0.5f, 1f));
    }

    private static void AssertCenterAnchor(RectTransform rect)
    {
        AssertVector(rect.anchorMin, new Vector2(0.5f, 0.5f));
        AssertVector(rect.anchorMax, new Vector2(0.5f, 0.5f));
    }

    private static void AssertRightCenterAnchor(RectTransform rect)
    {
        AssertVector(rect.anchorMin, new Vector2(1f, 0.5f));
        AssertVector(rect.anchorMax, new Vector2(1f, 0.5f));
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, null);
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }

    private static void AssertSize(RectTransform rect, Vector2 expected)
    {
        AssertVector(rect.sizeDelta, expected);
    }

    private static void AssertVector(Vector2 actual, Vector2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
    }
}
#endif
