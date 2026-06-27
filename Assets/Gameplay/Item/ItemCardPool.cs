using UnityEngine;

[System.Serializable]
public class ItemCardPool
{
    [SerializeField] private int[] cardIds;
    [SerializeField] private float[] cardWeights;

    public int[] CardIds => this.cardIds;
    public float[] CardWeights => this.cardWeights;

    public ItemCardPool() { }

    public ItemCardPool(int[] cardIds, float[] cardWeights)
    {
        this.cardIds = cardIds;
        this.cardWeights = cardWeights;
    }
}
