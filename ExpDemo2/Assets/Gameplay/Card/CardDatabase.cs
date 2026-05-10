using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Game/CardDatabase")]
public class CardDatabase : ScriptableObject
{
	[SerializeField] private List<CardData> cards = new List<CardData>();

	private Dictionary<int, CardData> idMap;
	private Dictionary<string, CardData> nameMap;

	private void BuildMap()
	{
		this.idMap = new Dictionary<int, CardData>();
		this.nameMap = new Dictionary<string, CardData>();

		foreach (CardData card in cards)
		{
			if (!card)
				continue;
			
			if (!idMap.ContainsKey(card.Id))
				idMap.Add(card.Id, card);
			else
				Debug.LogWarning($"Duplicate Id found: {card.Id}");
			
			if (!string.IsNullOrEmpty(card.CardName))
			{
				if (!this.nameMap.ContainsKey(card.CardName))
					this.nameMap.Add(card.CardName, card);
				else
					Debug.LogWarning($"Duplicate card name found: {card.CardName}");
			}
		}
	}

	public CardData GetById(int id)
	{
		if (idMap == null)
			BuildMap();

		idMap.TryGetValue(id, out CardData card);
		return card;
	}

	public CardData GetByName(string cardName)
	{
		if (this.nameMap == null)
			BuildMap();
		
		nameMap.TryGetValue(cardName, out CardData card);
		return card;
	}
}
