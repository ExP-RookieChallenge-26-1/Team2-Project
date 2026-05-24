using System.Collections.Generic;
using UnityEngine;

public class WorldEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 2;

    private void Awake()
    {
        if (this.spawnPoints == null || this.spawnPoints.Length == 0)
            this.spawnPoints = FindSpawnPoints("EnemySpawnPoints");
    }

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (this.enemyPrefab == null || this.spawnPoints == null || this.spawnPoints.Length == 0)
            return;

        int spawnCount = Random.Range(this.minSpawnCount, this.maxSpawnCount + 1);
        spawnCount = Mathf.Min(spawnCount, this.spawnPoints.Length);

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < this.spawnPoints.Length; ++i)
        {
            if (this.spawnPoints[i] != null)
                availableIndices.Add(i);
        }

        spawnCount = Mathf.Min(spawnCount, availableIndices.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int choice = Random.Range(0, availableIndices.Count);
            int index = availableIndices[choice];
            availableIndices.RemoveAt(choice);

            Transform point = this.spawnPoints[index];
            if (point == null)
                continue;

            GameObject enemy = Instantiate(this.enemyPrefab, point.position, Quaternion.identity);
            enemy.transform.SetParent(transform);
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