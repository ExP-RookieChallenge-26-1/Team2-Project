using UnityEngine;

public class ActiveBuffBadge
{
    public BuffBadgeData Data { get; }
    public string BadgeName { get; }
    public Sprite Icon { get; }
    public float Duration { get; }
    public float RemainingTime { get; set; }
    public bool IsPermanent => Duration <= 0f;

    public ActiveBuffBadge(BuffBadgeData data, float duration)
        : this(data, data != null ? data.Icon : null, data != null ? data.BadgeName : string.Empty, duration)
    {
    }

    public ActiveBuffBadge(Sprite icon, string badgeName, float duration)
        : this(null, icon, badgeName, duration)
    {
    }

    private ActiveBuffBadge(BuffBadgeData data, Sprite icon, string badgeName, float duration)
    {
        Data = data;
        Icon = icon;
        BadgeName = badgeName;
        Duration = duration;
        RemainingTime = duration;
    }
}
