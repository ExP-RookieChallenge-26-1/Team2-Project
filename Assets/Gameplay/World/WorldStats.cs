using UnityEngine;

[CreateAssetMenu(fileName = "WorldStats", menuName = "Stats/WorldStats")]
public class WorldStats : ScriptableObject
{
	[field: SerializeField] public float ScrollSpeed { get; private set; }
	[field: SerializeField] public float ChunkHeight { get; private set; }
	[field: SerializeField] public int ChunkCount { get; private set; }
}