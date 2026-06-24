using System.Collections.Generic;
using UnityEngine;

public class BuffBadgeBarUI : MonoBehaviour
{
    [SerializeField] private BuffBadgeUI badgePrefab;

    private readonly Dictionary<ActiveBuffBadge, BuffBadgeUI> activeUI = new();

    private void Start()
    {
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
        BuffBadgeUI ui = Instantiate(badgePrefab, transform);
        ui.Setup(badge);
        activeUI.Add(badge, ui);
    }

    private void OnBadgeDetached(ActiveBuffBadge badge)
    {
        if (!activeUI.TryGetValue(badge, out BuffBadgeUI ui)) return;
        activeUI.Remove(badge);
        Destroy(ui.gameObject);
    }
}
