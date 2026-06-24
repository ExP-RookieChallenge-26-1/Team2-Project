using UnityEngine;
using UnityEngine.UI;

public class BuffBadgeUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void Setup(ActiveBuffBadge badge)
    {
        icon.sprite = badge.Data.Icon;
    }
}
