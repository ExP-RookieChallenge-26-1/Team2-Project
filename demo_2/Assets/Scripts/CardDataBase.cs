using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Game/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> cards = new List<CardData>();

    private Dictionary<int, CardData> idMap;
    private Dictionary<string, CardData> nameMap;

    public void Init()
    {
        idMap = new Dictionary<int, CardData>();
        nameMap = new Dictionary<string, CardData>();

        foreach (var card in cards)
        {
            if (card == null) continue;

            if (!idMap.ContainsKey(card.id))
                idMap.Add(card.id, card);
            else
                Debug.LogWarning($"중복 id 발견: {card.id}");

            if (!string.IsNullOrEmpty(card.cardName))
            {
                if (!nameMap.ContainsKey(card.cardName))
                    nameMap.Add(card.cardName, card);
                else
                    Debug.LogWarning($"중복 카드명 발견: {card.cardName}");
            }
        }
    }

    public CardData GetById(int id)
    {
        if (idMap == null) Init();
        idMap.TryGetValue(id, out var card);
        return card;
    }

    public CardData GetByName(string cardName)
    {
        if (nameMap == null) Init();
        nameMap.TryGetValue(cardName, out var card);
        return card;
    }
}