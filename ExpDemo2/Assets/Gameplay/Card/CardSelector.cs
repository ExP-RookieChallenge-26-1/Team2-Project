using System.Collections.Generic;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
	[SerializeField] private CardDatabase cardDatabase;

	public List<CardData> Pick3ByIds(int[] ids, float[] weights)
	{
		List<CardData> pool;
		List<float> poolWeights;

		if (ids == null || weights == null || ids.Length != weights.Length)
		{
			Debug.LogError("Ids and weights are null or different length.");
			return new List<CardData>();
		}

		pool = new List<CardData>();
		poolWeights = new List<float>();

		for (int i = 0; i < ids.Length; ++i)
		{
			CardData card;
			card = this.cardDatabase.GetById(ids[i]);

			if (card != null && weights[i] > 0f)
			{
				pool.Add(card);
				poolWeights.Add(weights[i]);
			}
		}

		return PickUnique(pool, poolWeights, 3);
	}

	public List<CardData> Pick3ByNames(string[] names, float[] weights)
	{
		List<CardData> pool;
		List<float> poolWeights;

		if (names == null || weights == null || names.Length != weights.Length)
		{
			Debug.LogError("Names and weights are null or different length.");
			return new List<CardData>();
		}

		pool = new List<CardData>();
		poolWeights = new List<float>();

		for (int i = 0; i < names.Length; ++i)
		{
			CardData card;
			card = this.cardDatabase.GetByName(names[i]);

			if (card != null && weights[i] > 0f)
			{
				pool.Add(card);
				poolWeights.Add(weights[i]);
			}
		}

		return PickUnique(pool, poolWeights, 3);
	}

	private List<CardData> PickUnique(List<CardData> pool, List<float> weights, int count)
	{
		List<CardData> result;
		int pickCount;

		result = new List<CardData>();
		pickCount = Mathf.Min(count, pool.Count);

		for (int i = 0; i < pickCount; ++i)
		{
			int selectedIndex;

			selectedIndex = GetWeightedRandomIndex(weights);
			result.Add(pool[selectedIndex]);

			pool.RemoveAt(selectedIndex);
			weights.RemoveAt(selectedIndex);
		}

		return result;
	}

	private int GetWeightedRandomIndex(List<float> weights)
	{
		float totalWeight;
		float randomValue;
		float current;

		totalWeight = 0f;
		for (int i = 0; i < weights.Count; ++i)
			totalWeight += weights[i];
		
		if (totalWeight <= 0f)
			return 0;
		
		randomValue = Random.Range(0f, totalWeight);
		current = 0f;

		for (int i = 0; i < weights.Count; ++i)
		{
			current += weights[i];
			if (randomValue <= current)
				return i;
		}

		return weights.Count - 1;
	}
}