#if UNITY_EDITOR
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotPrefabTests
{
    private const string PrefabPath = "Assets/UI/Enhancement/Prefabs/CardSlot.prefab";
    private const float DescriptionFontSizeMin = 18f;

    [Test]
    public void CardSlotPrefabDisplaysCardIconAsFullCardArt()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        RectTransform slotRect = prefab.GetComponent<RectTransform>();
        Image slotBackground = prefab.GetComponent<Image>();
        Transform iconTransform = prefab.transform.Find("Icon");
        Transform titleTransform = prefab.transform.Find("TitleText");
        Transform descriptionTransform = prefab.transform.Find("DescriptionText");
        Assert.That(slotBackground, Is.Not.Null);
        Assert.That(iconTransform, Is.Not.Null);
        Assert.That(titleTransform, Is.Not.Null);
        Assert.That(descriptionTransform, Is.Not.Null);

        Image icon = iconTransform.GetComponent<Image>();
        RectTransform iconRect = iconTransform.GetComponent<RectTransform>();
        TMP_Text titleText = titleTransform.GetComponent<TMP_Text>();
        TMP_Text descriptionText = descriptionTransform.GetComponent<TMP_Text>();
        RectTransform titleRect = titleTransform.GetComponent<RectTransform>();
        RectTransform descriptionRect = descriptionTransform.GetComponent<RectTransform>();
        Assert.That(icon, Is.Not.Null);
        Assert.That(iconRect, Is.Not.Null);
        Assert.That(titleText, Is.Not.Null);
        Assert.That(descriptionText, Is.Not.Null);
        Assert.That(titleRect, Is.Not.Null);
        Assert.That(descriptionRect, Is.Not.Null);
        Assert.That(titleTransform.gameObject.activeSelf, Is.False);
        Assert.That(descriptionTransform.gameObject.activeSelf, Is.True);

        Assert.That(slotBackground.color.a, Is.EqualTo(0f).Within(0.001f));
        Assert.That(iconTransform.GetSiblingIndex(), Is.EqualTo(0));
        Assert.That(titleTransform.GetSiblingIndex(), Is.GreaterThan(iconTransform.GetSiblingIndex()));
        Assert.That(descriptionTransform.GetSiblingIndex(), Is.GreaterThan(iconTransform.GetSiblingIndex()));
        Assert.That(icon.preserveAspect, Is.True);
        Assert.That(iconRect.sizeDelta.x, Is.EqualTo(slotRect.sizeDelta.x).Within(0.001f));
        Assert.That(iconRect.sizeDelta.y, Is.EqualTo(slotRect.sizeDelta.y).Within(0.001f));
        AssertReadableDarkText(descriptionText.color);
        Assert.That(descriptionRect.anchorMin.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.anchorMin.y, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.anchorMax.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.anchorMax.y, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.pivot.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.pivot.y, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(descriptionRect.anchoredPosition.y, Is.EqualTo(-140f).Within(0.001f));
        Assert.That(descriptionRect.sizeDelta.x, Is.EqualTo(220f).Within(0.001f));
        Assert.That(descriptionRect.sizeDelta.y, Is.EqualTo(60f).Within(0.001f));
        Assert.That(descriptionText.horizontalAlignment, Is.EqualTo(HorizontalAlignmentOptions.Center));
        Assert.That(descriptionText.verticalAlignment, Is.EqualTo(VerticalAlignmentOptions.Middle));
        Assert.That(descriptionText.fontSize, Is.GreaterThanOrEqualTo(38f));
        Assert.That(descriptionText.enableAutoSizing, Is.True);
        Assert.That(descriptionText.fontSizeMin, Is.LessThanOrEqualTo(DescriptionFontSizeMin));
        Assert.That(descriptionText.overflowMode, Is.EqualTo(TextOverflowModes.Truncate));
    }

    [Test]
    public void CardSlotDescriptionTextShrinksToFitDescriptionBox()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        GameObject canvasObject = new("CardSlotTextFitCanvas", typeof(Canvas));
        GameObject instance = Object.Instantiate(prefab, canvasObject.transform);

        try
        {
            RectTransform descriptionRect = instance.transform.Find("DescriptionText").GetComponent<RectTransform>();
            TMP_Text descriptionText = descriptionRect.GetComponent<TMP_Text>();
            Assert.That(descriptionRect, Is.Not.Null);
            Assert.That(descriptionText, Is.Not.Null);

            descriptionText.text = "자동 시전되는 거대화의 쿨타임이 12.5초 감소합니다.";
            Canvas.ForceUpdateCanvases();
            descriptionText.ForceMeshUpdate(true, true);

            Assert.That(descriptionText.enableAutoSizing, Is.True);
            Assert.That(descriptionText.fontSizeMin, Is.LessThanOrEqualTo(DescriptionFontSizeMin));
            Assert.That(descriptionText.overflowMode, Is.EqualTo(TextOverflowModes.Truncate));
            Assert.That(descriptionText.isTextOverflowing, Is.False);
            Assert.That(descriptionText.renderedWidth, Is.LessThanOrEqualTo(descriptionRect.rect.width + 0.001f));
            Assert.That(descriptionText.renderedHeight, Is.LessThanOrEqualTo(descriptionRect.rect.height + 0.001f));
        }
        finally
        {
            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(canvasObject);
        }
    }

    private static void AssertReadableDarkText(Color color)
    {
        Assert.That(color.a, Is.EqualTo(1f).Within(0.001f));
        Assert.That(color.r, Is.LessThan(0.35f));
        Assert.That(color.g, Is.LessThan(0.35f));
        Assert.That(color.b, Is.LessThan(0.35f));
    }
}
#endif
