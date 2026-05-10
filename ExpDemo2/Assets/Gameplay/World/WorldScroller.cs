using UnityEngine;

public class WorldScroller
{
	private readonly World world;

	public WorldScroller(World world)
	{
		this.world = world;
	}

	public void Tick(float deltaTime)
	{
		GameObject[] chunks;
		float chunkHeight;

		if (deltaTime <= 0f)
			return;

		chunks = this.world.Spawner.Chunks;
		chunkHeight = this.world.Stats.ChunkHeight;

		for (int i = 0; i < chunks.Length; ++i)
		{
			chunks[i].transform.position += deltaTime * this.world.Stats.ScrollSpeed * Vector3.down;

			if (chunks[i].transform.position.y < -chunkHeight)
				this.world.Spawner.ReplaceChunk(i, new Vector3(0f, GetHighestChunkY(chunks) + chunkHeight, 0f));
		}
	}

	private float GetHighestChunkY(GameObject[] chunks)
	{
		float highestY;

		highestY = float.MinValue;
		for (int i = 0; i < chunks.Length; ++i)
		{
			if (chunks[i].transform.position.y > highestY)
				highestY = chunks[i].transform.position.y;
		}

		return highestY;
	}
}