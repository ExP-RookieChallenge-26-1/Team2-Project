using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffBadgeBarUI : MonoBehaviour
{
    [SerializeField] private BuffBadgeUI badgePrefab;

    private readonly Dictionary<ActiveBuffBadge, BuffBadgeUI> activeUI = new();
    private readonly List<ActiveBuffBadge> activeBadges = new();

    private void Awake()
    {
        ConfigureLayout();
    }

    private void OnEnable()
    {
        ConfigureLayout();
    }

    private void Start()
    {
        ConfigureLayout();

        if (BuffBadgeManager.Instance == null)
            return;

        BuffBadgeManager.Instance.OnBadgeAttached += OnBadgeAttached;
        BuffBadgeManager.Instance.OnBadgeDetached += OnBadgeDetached;
    }

    private void OnDestroy()
    {
        if (BuffBadgeManager.Instance == null) return;
        BuffBadgeManager.Instance.OnBadgeAttached -= OnBadgeAttached;
        BuffBadgeManager.Instance.OnBadgeDetached -= OnBadgeDetached;
    }

    private void OnBadgeAttached(ActiveBuffBadge badge)
    {
        ConfigureLayout();

        if (badgePrefab == null)
            return;

        BuffBadgeUI ui = Instantiate(badgePrefab, transform);
        ui.Setup(badge);
        activeUI.Add(badge, ui);
        activeBadges.Add(badge);
        LayoutBadges();
    }

    private void OnBadgeDetached(ActiveBuffBadge badge)
    {
        if (!activeUI.TryGetValue(badge, out BuffBadgeUI ui)) return;
        activeUI.Remove(badge);
        activeBadges.Remove(badge);
        Destroy(ui.gameObject);
        LayoutBadges();
    }

    private void OnRectTransformDimensionsChange()
    {
        LayoutBadges();
    }

    private void ConfigureLayout()
    {
        HorizontalLayoutGroup horizontal = GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null)
            horizontal.enabled = false;

        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        if (grid != null)
            grid.enabled = false;

        LayoutBadges();
    }

    private void LayoutBadges()
    {
        int columns = GetColumnCount();

        for (int i = 0; i < activeBadges.Count; i++)
        {
            if (!activeUI.TryGetValue(activeBadges[i], out BuffBadgeUI ui))
                continue;

            if (ui.transform is not RectTransform rect)
                continue;

            int column = i % columns;
            int row = i / columns;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = BuffBadgeUI.BadgeSize;
            rect.anchoredPosition = new Vector2(
                column * BuffBadgeUI.BadgeSize.x,
                -row * BuffBadgeUI.BadgeSize.y);
        }
    }

    private int GetColumnCount()
    {
        float width = 0f;
        if (transform is RectTransform rect)
        {
            width = rect.rect.width;
            if (width <= 0f)
                width = rect.sizeDelta.x;
        }

        return Mathf.Max(1, Mathf.FloorToInt(width / BuffBadgeUI.BadgeSize.x));
    }
}
