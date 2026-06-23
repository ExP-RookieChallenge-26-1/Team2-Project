using UnityEngine;

[CreateAssetMenu(fileName = "BuffBadgeData", menuName = "Buff/BuffBadgeData")]
public class BuffBadgeData : ScriptableObject
{
    [field: SerializeField] public string BadgeName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
}
