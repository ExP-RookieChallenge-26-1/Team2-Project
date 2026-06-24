using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnData", menuName = "Spawn/EnemySpawnData")]
public class EnemySpawnData : EnemyStats
{
    [field: SerializeField] public GameObject EnemyPrefab { get; private set; }
}
