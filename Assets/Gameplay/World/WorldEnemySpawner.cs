using UnityEngine;

public class WorldEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 2;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        spawnCount = Mathf.Min(spawnCount, spawnPoints.Length);

        bool[] used = new bool[spawnPoints.Length];

        for (int i = 0; i < spawnCount; i++)
        {
            int index = GetRandomUnusedIndex(used);
            if (index == -1)
                break;

            Transform point = spawnPoints[index];
            GameObject enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);

            enemy.transform.SetParent(transform);
            used[index] = true;
        }
    }

    private int GetRandomUnusedIndex(bool[] used)
    {
        int tryCount = 100;

        while (tryCount-- > 0)
        {
            int index = Random.Range(0, used.Length);
            if (!used[index])
                return index;
        }

        return -1;
    }
}
