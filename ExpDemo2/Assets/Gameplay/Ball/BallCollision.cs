using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Ball))]
public class BallCollision : MonoBehaviour
{
	public readonly struct Collision
	{
		public enum Type
		{
			None,
			Wall,
			Paddle,
			Terrain
		}
		public readonly Type type;
		public readonly Bounds bounds;

		public Collision(Type type, Bounds bounds)
		{
			this.type = type;
			this.bounds = bounds;
		}
	}

	private Ball ball;
	[SerializeField] private Paddle paddle;
	private static readonly List<Collider2D> overlapBuffer = new List<Collider2D>();

	private void Awake()
	{
		this.ball = GetComponent<Ball>();
	}
	public void Tick()
	{
		Collision closest;

		closest = DetectClosestCollision(this.ball.transform.position, this.ball.Stats.Radius);
		ApplyCollision(closest);
	}

	private void ApplyCollision(Collision collision)
	{
		switch (collision.type)
		{
			case Collision.Type.None:
				return;
			case Collision.Type.Wall:
			case Collision.Type.Terrain:
				this.ball.Physics.ApplyWallReflection(collision.bounds);
				break;
			case Collision.Type.Paddle:
				this.ball.Physics.ApplyPaddleReflection(collision.bounds, this.paddle);
				break;
		}
	}
	public static Collision DetectClosestCollision(Vector2 pos, float radius)
	{
		int hitCount;
		Collision closest;
		float closestOverlap;

		overlapBuffer.Clear();
		hitCount = Physics2D.OverlapCircle(pos, radius, ContactFilter2D.noFilter, overlapBuffer);
		closest = new Collision(Collision.Type.None, default);
		closestOverlap = float.MaxValue;

		for (int i = 0; i < hitCount; ++i)
		{
			Collider2D hit;
			Collision collision;
			float overlap;

			hit = overlapBuffer[i];

			if (hit.CompareTag("Wall"))
				collision = new Collision(Collision.Type.Wall, hit.bounds);
			else if (hit.CompareTag("Paddle"))
				collision = new Collision(Collision.Type.Paddle, hit.bounds);
			else if (hit.CompareTag("Terrain"))
			{
				Tilemap tilemap;
				Bounds tileBounds;

				tilemap = hit.GetComponent<Tilemap>();
				if (tilemap == null)
					continue;
				
				tileBounds = DetectClosestTileBounds(tilemap, pos, radius);
				if (tileBounds.size == Vector3.zero)
					continue;
				
				collision = new Collision(Collision.Type.Terrain, tileBounds);
			}
			else
				continue;

			overlap = GetMinOverlap(collision.bounds, pos, radius);
			if (overlap < closestOverlap)
			{
				closestOverlap = overlap;
				closest = collision;
			}
		}

		return closest;
	}

	public static Bounds DetectClosestTileBounds(Tilemap tilemap, Vector2 pos, float radius)
	{
		int checkRange;
		Vector3Int centerCell;
		Bounds closest;
		float closestOverlap;

		checkRange = Mathf.CeilToInt(radius / tilemap.cellSize.x) + 1;
		centerCell = tilemap.WorldToCell(pos);
		closest = default;
		closestOverlap = float.MaxValue;

		for (int x = -checkRange; x <= checkRange; ++x)
		{
			for (int y = -checkRange; y <= checkRange; ++y)
			{
				Vector3Int cellPos;
				Vector3 worldPos;
				Bounds tileBounds;
				float overlap;

				cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
				
				if (!tilemap.HasTile(cellPos))
					continue;

				worldPos = tilemap.CellToWorld(cellPos) + tilemap.cellSize / 2f;
				tileBounds = new Bounds(worldPos, tilemap.cellSize);

				if (!IsTouching(tileBounds, pos, radius))
					continue;
				
				overlap = GetMinOverlap(tileBounds, pos, radius);

				if (overlap < closestOverlap)
				{
					closestOverlap = overlap;
					closest = tileBounds;
				}
			}
		}

		return closest;
	}

	public static float GetMinOverlap(Bounds bounds, Vector2 pos, float radius)
	{
		float overlapLeft;
		float overlapRight;
		float overlapBottom;
		float overlapTop;

		overlapLeft = (pos.x + radius) - bounds.min.x;
		overlapRight = bounds.max.x - (pos.x - radius);
		overlapBottom = (pos.y + radius) - bounds.min.y;
		overlapTop = bounds.max.y - (pos.y - radius);

		return Mathf.Min(overlapLeft, overlapRight, overlapBottom, overlapTop);
	}

	public static bool IsTouching(Bounds bounds, Vector2 pos, float radius)
	{
		Vector2 closest;
		Vector2 dist;

		closest.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
		closest.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
		dist = pos - closest;

		return dist.sqrMagnitude <= radius * radius;
	}
}