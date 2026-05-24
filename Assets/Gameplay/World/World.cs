using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private GameObject[] itemPrefabs;

    public WorldStats Stats { get; private set; }
    public WorldScroller Scroller { get; private set; }
    public WorldSpawner Spawner { get; private set; }
    public GameObject[] ItemPrefabs => this.itemPrefabs;

    private void Start()
    {
        this.Stats = GameManager.Instance.WorldStats;
        this.Spawner = new WorldSpawner(this, this.chunkPrefabs);
        this.Scroller = new WorldScroller(this);
        this.Spawner.Init();
    }

    private void Update()
    {
        this.Scroller.Tick(Time.deltaTime);
    }
}