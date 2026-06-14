using UnityEngine;

public class WorldSpawner
{
    private readonly World world;
    private readonly GameObject[] chunkPrefabs;
    public GameObject[] Chunks { get; private set; }

    private int nextChunkIndex;

    public WorldSpawner(World world, GameObject[] chunkPrefabs)
    {
        this.world = world;
        this.chunkPrefabs = chunkPrefabs;
        this.Chunks = new GameObject[world.Stats.ChunkCount];
        this.nextChunkIndex = 0;
    }

    public void Init()
    {
        for (int i = 0; i < this.Chunks.Length; ++i)
        {
            float startY = i * this.world.Stats.ChunkHeight;
            this.Chunks[i] = SpawnNextChunk(new Vector3(0f, startY, 0f));
        }
    }

    public void ReplaceChunk(int index, Vector3 position)
    {
        GameObject.Destroy(this.Chunks[index]);
        this.Chunks[index] = SpawnNextChunk(position);
    }

    private GameObject SpawnNextChunk(Vector3 position)
    {
        if (this.chunkPrefabs == null || this.chunkPrefabs.Length == 0)
        {
            Debug.LogError("Chunk Prefabs is empty.");
            return null;
        }

        GameObject chunk = GameObject.Instantiate(this.chunkPrefabs[this.nextChunkIndex], position, Quaternion.identity);
        ConfigureChunk(chunk);

        this.nextChunkIndex++;

        if (this.nextChunkIndex >= this.chunkPrefabs.Length)
            this.nextChunkIndex = 0;

        return chunk;
    }

    private void ConfigureChunk(GameObject chunk)
    {
        if (chunk == null)
            return;

        EnsureWorldItemSpawner(chunk);
    }

    private void EnsureWorldItemSpawner(GameObject chunk)
    {
        Transform itemSpawnRoot;
        WorldItemSpawner itemSpawner;
        GameObject[] itemPrefabs;

        itemSpawnRoot = chunk.transform.Find("ItemSpawnPoints");
        if (itemSpawnRoot == null)
            return;

        itemPrefabs = this.world.ItemPrefabs;
        if (itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        itemSpawner = chunk.GetComponent<WorldItemSpawner>();
        if (itemSpawner == null)
            itemSpawner = chunk.AddComponent<WorldItemSpawner>();

        itemSpawner.Configure(itemPrefabs);
    }
}