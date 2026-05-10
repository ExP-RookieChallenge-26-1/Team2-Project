using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldSpawner
{
	private readonly World world;
	private readonly GameObject[] chunkPrefabs;
	public GameObject[] Chunks { get; private set; }

	public WorldSpawner(World world, GameObject[] chunkPrefabs)
	{
		this.world = world;
		this.chunkPrefabs = chunkPrefabs;
		this.Chunks = new GameObject[world.Stats.ChunkCount];
	}

	public void Init()
	{
		for (int i = 0; i < this.Chunks.Length; ++i)
		{
			float startY;

			startY = i * this.world.Stats.ChunkHeight;
			this.Chunks[i] = SpawnRandomChunk(new Vector3(0f, startY, 0f));
		}
	}

	public void ReplaceChunk(int index, Vector3 position)
	{
		GameObject.Destroy(this.Chunks[index]);
		this.Chunks[index] = SpawnRandomChunk(position);
	}

	private GameObject SpawnRandomChunk(Vector3 position)
	{
		int randomIndex;
		randomIndex = Random.Range(0, this.chunkPrefabs.Length);
		return GameObject.Instantiate(this.chunkPrefabs[randomIndex], position, Quaternion.identity);
	}
}