using System.Collections.Generic;
using UnityEngine;

public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 1;

    public void Configure(GameObject[] configuredItemPrefabs)
    {
        if (configuredItemPrefabs != null && configuredItemPrefabs.Length > 0)
            this.itemPrefabs = configuredItemPrefabs;
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

        List<Transform> validPoints = new List<Transform>();
        for (int i = 0; i < this.spawnPoints.Length; ++i)
        {
            if (this.spawnPoints[i] != null)
                validPoints.Add(this.spawnPoints[i]);
        }

        spawnCount = Mathf.Min(spawnCount, validPoints.Count);

        for (int i = 0; i < spawnCount; ++i)
        {
            int pointIndex = Random.Range(0, validPoints.Count);
            Transform point = validPoints[pointIndex];
            validPoints.RemoveAt(pointIndex);

            GameObject itemPrefab = this.itemPrefabs[Random.Range(0, this.itemPrefabs.Length)];
            if (itemPrefab == null)
                continue;

            GameObject item = Instantiate(itemPrefab, point.position, Quaternion.identity);
            item.transform.SetParent(transform);
        }
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