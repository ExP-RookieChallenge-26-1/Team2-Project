using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public sealed class SettingButtonVisual : MonoBehaviour
{
    private const string SettingButtonSpriteResourcePath = "UI/setting-button-generated";
    private static readonly Vector2 TopRightAnchor = new Vector2(1f, 1f);
    private static readonly Vector2 SettingButtonOffset = new Vector2(-112f, -112f);
    private static readonly Vector2 SettingButtonSize = new Vector2(176f, 176f);

    private static Sprite cachedSprite;

    private void Awake()
    {
        ApplyVisual();
    }

    private void OnEnable()
    {
        ApplyVisual();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += ApplyVisualIfAlive;
#endif
    }

#if UNITY_EDITOR
    private void ApplyVisualIfAlive()
    {
        if (this != null)
            ApplyVisual();
    }
#endif

    private void ApplyVisual()
    {
        if (transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = TopRightAnchor;
            rectTransform.anchorMax = TopRightAnchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = SettingButtonOffset;
            rectTransform.sizeDelta = SettingButtonSize;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        Image image = GetComponent<Image>();
        Sprite sprite = LoadSettingButtonSprite();
        if (sprite != null)
            image.sprite = sprite;

        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        TextMeshProUGUI label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
            return;

        label.text = string.Empty;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f;
        label.fontSizeMax = 36f;
        label.raycastTarget = false;
    }

    private static Sprite LoadSettingButtonSprite()
    {
        if (cachedSprite != null)
            return cachedSprite;

        cachedSprite = Resources.Load<Sprite>(SettingButtonSpriteResourcePath);
        if (cachedSprite == null)
            Debug.LogError($"Setting button sprite not found: Resources/{SettingButtonSpriteResourcePath}");

        return cachedSprite;
    }
}
