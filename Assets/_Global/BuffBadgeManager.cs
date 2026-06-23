using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffBadgeManager : MonoBehaviour
{
    public static BuffBadgeManager Instance { get; private set; }

    private readonly List<ActiveBuffBadge> activeBadges = new();

    public event Action<ActiveBuffBadge> OnBadgeAttached;
    public event Action<ActiveBuffBadge> OnBadgeDetached;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        for (int i = activeBadges.Count - 1; i >= 0; i--)
        {
            ActiveBuffBadge badge = activeBadges[i];
            if (badge.IsPermanent) continue;

            badge.RemainingTime -= Time.deltaTime;
            if (badge.RemainingTime <= 0f)
                Detach(badge);
        }
    }

    public void Attach(BuffBadgeData data, float duration = 0f)
    {
        if (data == null)
        {
            Debug.LogWarning("BuffBadgeData is null.");
            return;
        }

        ActiveBuffBadge badge = new ActiveBuffBadge(data, duration);
        activeBadges.Add(badge);
        OnBadgeAttached?.Invoke(badge);
    }

    private void Detach(ActiveBuffBadge badge)
    {
        if (!activeBadges.Remove(badge)) return;
        OnBadgeDetached?.Invoke(badge);
    }
}
