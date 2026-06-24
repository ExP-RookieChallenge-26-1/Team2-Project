public class ActiveBuffBadge
{
    public BuffBadgeData Data { get; }
    public float Duration { get; }
    public float RemainingTime { get; set; }
    public bool IsPermanent => Duration <= 0f;

    public ActiveBuffBadge(BuffBadgeData data, float duration)
    {
        Data = data;
        Duration = duration;
        RemainingTime = duration;
    }
}
