#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneArtworkTests
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string BackgroundResourcePath = "TitleSceneArtwork/background";
    private const string LogoResourcePath = "TitleSceneArtwork/logo";

    [Test]
    public void TitleSceneUsesProvidedBackgroundAndTopLeftLogo()
    {
        EditorSceneManager.OpenScene(TitleScenePath);

        Assert.That(Resources.LoadAll(BackgroundResourcePath), Is.Not.Empty);
        Assert.That(Resources.LoadAll(LogoResourcePath), Is.Not.Empty);

        TitleUI titleUI = Object.FindFirstObjectByType<TitleUI>();
        Assert.That(titleUI, Is.Not.Null);
        titleUI.gameObject.SendMessage("ApplyTitleArtwork", SendMessageOptions.RequireReceiver);

        GameObject backgroundObject = GameObject.Find("Canvas/TitleImage");
        Assert.That(backgroundObject, Is.Not.Null);

        RectTransform backgroundRect = backgroundObject.transform as RectTransform;
        Assert.That(backgroundRect, Is.Not.Null);
        Assert.That(backgroundRect.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(backgroundRect.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(backgroundRect.anchoredPosition, Is.EqualTo(Vector2.zero));
        Assert.That(backgroundRect.sizeDelta, Is.EqualTo(Vector2.zero));

        Image backgroundImage = backgroundObject.GetComponent<Image>();
        Assert.That(backgroundImage, Is.Not.Null);
        Assert.That(backgroundImage.sprite, Is.Not.Null);
        Assert.That(backgroundImage.sprite.texture.width, Is.EqualTo(1080));
        Assert.That(backgroundImage.sprite.texture.height, Is.EqualTo(1920));
        Assert.That(backgroundImage.sprite.rect.width, Is.EqualTo(1080));
        Assert.That(backgroundImage.sprite.rect.height, Is.EqualTo(1920));
        Assert.That(backgroundImage.color, Is.EqualTo(Color.white));
        Assert.That(backgroundImage.preserveAspect, Is.False);

        GameObject logoObject = GameObject.Find("Canvas/TitleUI/TitleLogo");
        Assert.That(logoObject, Is.Not.Null);

        RectTransform logoRect = logoObject.transform as RectTransform;
        Assert.That(logoRect, Is.Not.Null);
        Assert.That(logoRect.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(logoRect.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(logoRect.pivot, Is.EqualTo(new Vector2(0f, 1f)));
        Assert.That(logoRect.anchoredPosition, Is.EqualTo(new Vector2(36f, -36f)));
        Assert.That(logoRect.sizeDelta, Is.EqualTo(new Vector2(312f, 637f)));

        Image logoImage = logoObject.GetComponent<Image>();
        Assert.That(logoImage, Is.Not.Null);
        Assert.That(logoImage.sprite, Is.Not.Null);
        Assert.That(logoImage.sprite.texture.width, Is.EqualTo(312));
        Assert.That(logoImage.sprite.texture.height, Is.EqualTo(637));
        Assert.That(logoImage.sprite.rect.width, Is.EqualTo(312));
        Assert.That(logoImage.sprite.rect.height, Is.EqualTo(637));
        Assert.That(logoImage.color, Is.EqualTo(Color.white));
        Assert.That(logoImage.preserveAspect, Is.True);
        Assert.That(logoImage.raycastTarget, Is.False);
    }
}
#endif
