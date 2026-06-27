using UnityEngine;
using UnityEngine.UI;

public class BuffBadgeUI : MonoBehaviour
{
    public static readonly Vector2 BadgeSize = new(66.6667f, 66.6667f);

    [SerializeField] private Image icon;

    private void Awake()
    {
        ApplyBadgeSize();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyBadgeSize();
    }
#endif

    public void Setup(ActiveBuffBadge badge)
    {
        ApplyBadgeSize();

        if (icon == null)
            return;

        icon.sprite = badge.Icon;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
    }

    private void ApplyBadgeSize()
    {
        if (transform is RectTransform rect)
            rect.sizeDelta = BadgeSize;
    }
}
