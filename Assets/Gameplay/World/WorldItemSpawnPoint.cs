using System.Collections.Generic;
using UnityEngine;

public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private ItemCardPool[] itemCardPools;
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 1;
    [SerializeField] private int mapIndex = -1;

    public void Configure(GameObject[] configuredItemPrefabs)
    {
        Configure(configuredItemPrefabs, this.mapIndex);
    }

    public void Configure(GameObject[] configuredItemPrefabs, int configuredMapIndex)
    {
        if (configuredItemPrefabs != null && configuredItemPrefabs.Length > 0)
            this.itemPrefabs = configuredItemPrefabs;

        this.mapIndex = configuredMapIndex;
    }

    private void Awake()
    {
        if (this.spawnPoints == null || this.spawnPoints.Length == 0)
            this.spawnPoints = FindSpawnPoints("ItemSpawnPoints");
    }

    private void Start()
    {
        SpawnItems();
    }

    private void SpawnItems()
    {
        if (this.itemPrefabs == null || this.itemPrefabs.Length == 0)
            return;

        if (this.spawnPoints == null || this.spawnPoints.Length == 0)
            return;

        int spawnCount = Random.Range(this.minSpawnCount, this.maxSpawnCount + 1);

        List<int> validPointIndices = new List<int>();
        for (int i = 0; i < this.spawnPoints.Length; ++i)
        {
            if (this.spawnPoints[i] != null)
                validPointIndices.Add(i);
        }

        spawnCount = Mathf.Min(spawnCount, validPointIndices.Count);

        for (int i = 0; i < spawnCount; ++i)
        {
            int selectedIndex = Random.Range(0, validPointIndices.Count);
            int pointIndex = validPointIndices[selectedIndex];
            Transform point = this.spawnPoints[pointIndex];
            validPointIndices.RemoveAt(selectedIndex);

            GameObject itemPrefab = this.itemPrefabs[Random.Range(0, this.itemPrefabs.Length)];
            if (itemPrefab == null)
                continue;

            GameObject item = Instantiate(itemPrefab, point.position, Quaternion.identity);
            ApplyCardPool(item, pointIndex);
            item.transform.SetParent(transform);
        }
    }

    private void ApplyCardPool(GameObject item, int spawnPointIndex)
    {
        ItemPickup itemPickup = item.GetComponent<ItemPickup>();
        ItemCardPool cardPool = GetItemCardPool(spawnPointIndex);

        if (itemPickup == null)
            return;

        CardUseContext context = new CardUseContext(CardOfferSource.Item, this.mapIndex);
        itemPickup.SetCardContext(context);

        if (cardPool == null)
            return;

        itemPickup.SetCardPool(cardPool.CardIds, cardPool.CardWeights, context);
    }

    private ItemCardPool GetItemCardPool(int index)
    {
        if (this.itemCardPools == null || index < 0 || index >= this.itemCardPools.Length)
            return null;

        return this.itemCardPools[index];
    }

    private Transform[] FindSpawnPoints(string rootName)
    {
        Transform root = transform.Find(rootName);
        if (root == null)
            return System.Array.Empty<Transform>();

        List<Transform> points = new List<Transform>();
        for (int i = 0; i < root.childCount; ++i)
            points.Add(root.GetChild(i));

        return points.ToArray();
    }
}
