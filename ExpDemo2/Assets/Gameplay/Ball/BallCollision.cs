using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Ball))]
public class BallCollision : MonoBehaviour
{
	private Ball ball;
	[SerializeField] private Paddle paddle;
	[SerializeField] private World world;

	private void Awake()
	{
		this.ball = GetComponent<Ball>();
	}
	public void Tick()
	{
		CheckCollision();
		CheckTerrainCollision();
	}

	private void CheckCollision()
	{
		Collider2D[] hits;
		float radius;
		
		radius = this.ball.Stats.radius;
		hits = Physics2D.OverlapCircleAll(this.ball.transform.position, radius);
		
		foreach (Collider2D hit in hits)
		{
			if (hit.CompareTag("Wall"))
				this.ball.Physics.ResolveWallCollision(hit.bounds);
			else if (hit.CompareTag("Paddle"))
				this.ball.Physics.ResolvePaddleCollision(hit.bounds, this.paddle);
		}
	}

	private void CheckTerrainCollision()
	{
		Tilemap[] tilemaps;

		tilemaps = this.world.Spawner.GetActiveTilemaps();

		foreach (Tilemap tilemap in tilemaps)
		{
			int checkRange;
			Vector3Int centerCell;
			float radius;

			if (tilemap == null)
				continue;
			
			radius = this.ball.Stats.radius;
			checkRange = Mathf.CeilToInt(radius / tilemap.cellSize.x) + 1;
			centerCell = tilemap.WorldToCell(ball.transform.position);

			for (int x = -checkRange; x <= checkRange; ++x)
			{
				for (int y = -checkRange; y <= checkRange; ++y)
				{
					Vector3Int cellPos;
					Vector3 worldPos;
					Bounds tileBounds;

					cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);

					if (!tilemap.HasTile(cellPos))
						continue;
					
					worldPos = tilemap.CellToWorld(cellPos) + tilemap.cellSize / 2f;
					tileBounds = new Bounds(worldPos, tilemap.cellSize);

					if (IsTouching(tileBounds))
						this.ball.Physics.ResolveWallCollision(tileBounds);
				}
			}
		}
	}

	private bool IsTouching(Bounds bounds)
	{
		Vector2 pos;
		Vector2 closest;
		Vector2 dist;
		float radius;

		pos = this.ball.transform.position;
		radius = this.ball.Stats.radius;
		closest.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
		closest.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
		dist = pos - closest;

		return dist.sqrMagnitude <= radius * radius;
	}
}