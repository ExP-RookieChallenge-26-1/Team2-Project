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

        GameObject chunkPrefab = this.chunkPrefabs[this.nextChunkIndex];
        int mapIndex = GetChunkMapIndex(chunkPrefab, this.nextChunkIndex);
        GameObject chunk = GameObject.Instantiate(chunkPrefab, position, Quaternion.identity);
        ConfigureChunk(chunk, mapIndex);

        this.nextChunkIndex++;

        if (this.nextChunkIndex >= this.chunkPrefabs.Length)
            this.nextChunkIndex = 0;

        return chunk;
    }

    private void ConfigureChunk(GameObject chunk, int mapIndex)
    {
        if (chunk == null)
            return;

        EnsureTilemapAspectCropper(chunk);
        EnsureWorldItemSpawner(chunk, mapIndex);
        ScheduleFallingItems(chunk, mapIndex);
    }

    private static void EnsureTilemapAspectCropper(GameObject chunk)
    {
        TilemapAspectCropper cropper = chunk.GetComponent<TilemapAspectCropper>();
        if (cropper == null)
            cropper = chunk.AddComponent<TilemapAspectCropper>();

        cropper.Configure(Camera.main);
    }

    private void ScheduleFallingItems(GameObject chunk, int mapIndex)
    {
        FallingItemSpawner fallingItemSpawner = Object.FindFirstObjectByType<FallingItemSpawner>();
        if (fallingItemSpawner == null)
            return;

        fallingItemSpawner.ScheduleMapDrops(mapIndex, chunk.transform);
    }

    private void EnsureWorldItemSpawner(GameObject chunk, int mapIndex)
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

        itemSpawner.Configure(itemPrefabs, mapIndex);
    }

    private static int GetChunkMapIndex(GameObject chunkPrefab, int fallbackIndex)
    {
        if (chunkPrefab == null || string.IsNullOrEmpty(chunkPrefab.name))
            return fallbackIndex;

        string name = chunkPrefab.name;
        int end = name.Length - 1;
        while (end >= 0 && !char.IsDigit(name[end]))
            end--;

        if (end < 0)
            return fallbackIndex;

        int start = end;
        while (start >= 0 && char.IsDigit(name[start]))
            start--;

        if (int.TryParse(name.Substring(start + 1, end - start), out int mapIndex))
            return mapIndex;

        return fallbackIndex;
    }
}
