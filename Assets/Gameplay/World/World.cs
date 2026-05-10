using UnityEngine;

public class World : MonoBehaviour
{
	[SerializeField] private GameObject[] chunkPrefabs;

	public WorldStats Stats { get; private set; }
	public WorldScroller Scroller { get; private set; }
	public WorldSpawner Spawner { get; private set; }

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