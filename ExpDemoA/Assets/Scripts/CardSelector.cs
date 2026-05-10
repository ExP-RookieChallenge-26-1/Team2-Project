using System.Collections.Generic;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField] private CardDatabase cardDatabase;

    public List<CardData> Pick3ByIds(int[] ids, float[] weights)
    {
        if (ids == null || weights == null || ids.Length != weights.Length)
        {
            Debug.LogError("ids와 weights 길이가 다르거나 null입니다.");
            return new List<CardData>();
        }

        List<CardData> pool = new List<CardData>();
        List<float> poolWeights = new List<float>();

        for (int i = 0; i < ids.Length; i++)
        {
            CardData card = cardDatabase.GetById(ids[i]);
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
        if (names == null || weights == null || names.Length != weights.Length)
        {
            Debug.LogError("names와 weights 길이가 다르거나 null입니다.");
            return new List<CardData>();
        }

        List<CardData> pool = new List<CardData>();
        List<float> poolWeights = new List<float>();

        for (int i = 0; i < names.Length; i++)
        {
            CardData card = cardDatabase.GetByName(names[i]);
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
        List<CardData> result = new List<CardData>();
        int pickCount = Mathf.Min(count, pool.Count);

        for (int i = 0; i < pickCount; i++)
        {
            int selectedIndex = GetWeightedRandomIndex(weights);
            result.Add(pool[selectedIndex]);

            pool.RemoveAt(selectedIndex);
            weights.RemoveAt(selectedIndex);
        }

        return result;
    }

    private int GetWeightedRandomIndex(List<float> weights)
    {
        float totalWeight = 0f;

        for (int i = 0; i < weights.Count; i++)
            totalWeight += weights[i];

        if (totalWeight <= 0f)
            return 0;

        float randomValue = Random.Range(0f, totalWeight);
        float current = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            current += weights[i];
            if (randomValue <= current)
                return i;
        }

        return weights.Count - 1;
    }
}