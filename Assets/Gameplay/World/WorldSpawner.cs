using UnityEngine;

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
        GameObject chunk;

        randomIndex = Random.Range(0, this.chunkPrefabs.Length);
        chunk = GameObject.Instantiate(this.chunkPrefabs[randomIndex], position, Quaternion.identity);
        ConfigureChunk(chunk);
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