using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldEnemySpawner : MonoBehaviour
{
    [FormerlySerializedAs("enemyPrefab")]
    [SerializeField] private GameObject fallbackEnemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemySpawnData[] enemySpawnDataList;
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
        if (this.spawnPoints == null || this.spawnPoints.Length == 0)
            return;

        int spawnCount = Random.Range(this.minSpawnCount, this.maxSpawnCount + 1);

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < this.spawnPoints.Length; ++i)
        {
            if (this.spawnPoints[i] != null && HasEnemyData(i))
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

            EnemySpawnData spawnData = GetEnemySpawnData(index);
            GameObject enemyPrefab = spawnData != null && spawnData.EnemyPrefab != null
                ? spawnData.EnemyPrefab
                : this.fallbackEnemyPrefab;
            if (enemyPrefab == null)
                continue;

            GameObject enemyObject = Instantiate(enemyPrefab, point.position, Quaternion.identity);
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null && spawnData != null)
                enemy.Initialize(spawnData);

            enemyObject.transform.SetParent(transform);
        }
    }

    private bool HasEnemyData(int index)
    {
        EnemySpawnData spawnData = GetEnemySpawnData(index);
        return (spawnData != null && spawnData.EnemyPrefab != null) || this.fallbackEnemyPrefab != null;
    }

    private EnemySpawnData GetEnemySpawnData(int index)
    {
        if (this.enemySpawnDataList == null || index < 0 || index >= this.enemySpawnDataList.Length)
            return null;

        return this.enemySpawnDataList[index];
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
