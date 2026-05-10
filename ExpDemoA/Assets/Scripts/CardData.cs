using UnityEngine;

public enum UpgradeType
{
    AttackPower,
    CritChance,
    BallSize,
    Speed,
    BallCount
}

[CreateAssetMenu(fileName = "CardData", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    public int id;
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;

    public UpgradeType upgradeType;
    public float value;
}