using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    public int id;
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;
}