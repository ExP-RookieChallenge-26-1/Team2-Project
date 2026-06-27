using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

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
			Terrain,
			Enemy
		}

		public readonly Type type;
		public readonly Bounds bounds;
        public readonly IDamageable damageable;
        public readonly Collider2D damageableCollider;

        public Collision(Type type, Bounds bounds, IDamageable damageable = null, Collider2D damageableCollider = null)
        {
            this.type = type;
            this.bounds = bounds;
            this.damageable = damageable;
            this.damageableCollider = damageableCollider;
        }
	}

	private const bool DefaultAllowLowTerrainPassThrough = true;
	public const float DefaultLowTerrainPassThroughMaxY = -3f;
	private static readonly Vector3Int[] ConnectedTileOffsets =
	{
		Vector3Int.left,
		Vector3Int.right,
		Vector3Int.up,
		Vector3Int.down
	};

	private Ball ball;
	[SerializeField] private Paddle paddle;
	[SerializeField] private bool allowLowTerrainPassThrough = DefaultAllowLowTerrainPassThrough;
	[SerializeField] private float lowTerrainPassThroughMaxY = DefaultLowTerrainPassThroughMaxY;
	private static readonly List<Collider2D> overlapBuffer = new List<Collider2D>();

	public bool AllowLowTerrainPassThrough => this.allowLowTerrainPassThrough;
	public float LowTerrainPassThroughMaxY => this.lowTerrainPassThroughMaxY;

	private IDamageable lastHitDamageable;
	private Collider2D lastHitDamageableCollider;

	private void Awake()
	{
		this.ball = GetComponent<Ball>();
	}

	private IEnumerator PlayAttackVoiceNextFrame()
	{
		yield return null;

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayAttackVoiceSound();
	}

	public void Tick()
	{
		Collision closest;

		UpdateEnemyHitState();
		closest = DetectClosestCollision(
			this.ball.transform.position,
			this.ball.Stats.Radius,
			this.ball.Physics.Velocity,
			this.allowLowTerrainPassThrough,
			this.lowTerrainPassThroughMaxY);
		ApplyCollision(closest);
	}

    private void UpdateEnemyHitState()
    {
        if (this.lastHitDamageable == null || this.lastHitDamageableCollider == null)
            return;

        bool stillTouching = IsTouching(this.lastHitDamageableCollider.bounds, this.ball.transform.position, this.ball.Stats.Radius);
        if (!stillTouching)
        {
            this.lastHitDamageable = null;
            this.lastHitDamageableCollider = null;
        }
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
				AudioManager.Instance.PlayBallHitSound();
				break;

			case Collision.Type.Paddle:
				this.ball.Physics.ApplyPaddleReflectionAlternative(collision.bounds, this.paddle);
				break;

            case Collision.Type.Enemy:
                if (collision.damageable != null && collision.damageable != this.lastHitDamageable)
                {
                    var (damage, isCrit) = CalculateDamage();

                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayAttackSound();

                    StartCoroutine(PlayAttackVoiceNextFrame());

                    collision.damageable.TakeDamage(damage, isCrit);
                    this.ball.Animation.TriggerAttack();
                    Vector2 velocity = this.ball.Physics.Velocity;
                    AttackEffectSpawner.Instance?.Spawn(
                        this.ball.transform.position,
                        isMovingUp: velocity.y >= 0f);
                    this.lastHitDamageable = collision.damageable;
                    this.lastHitDamageableCollider = collision.damageableCollider;
                }
                break;
        }
	}

	// public static Collision DetectClosestCollision(Vector2 pos, float radius)
	// {
	// 	int hitCount;
	// 	Collision closest;
	// 	float closestOverlap;

	// 	overlapBuffer.Clear();
	// 	hitCount = Physics2D.OverlapCircle(pos, radius, ContactFilter2D.noFilter, overlapBuffer);
	// 	closest = new Collision(Collision.Type.None, default);
	// 	closestOverlap = float.MaxValue;

	// 	for (int i = 0; i < hitCount; ++i)
	// 	{
	// 		Collider2D hit;
	// 		Collision collision;
	// 		float overlap;

	// 		hit = overlapBuffer[i];

	// 		if (hit.CompareTag("Wall"))
	// 			collision = new Collision(Collision.Type.Wall, hit.bounds);
	// 		else if (hit.CompareTag("Paddle"))
	// 			collision = new Collision(Collision.Type.Paddle, hit.bounds);
	// 		else if (hit.CompareTag("Terrain"))
	// 		{
	// 			Tilemap tilemap;
	// 			Bounds tileBounds;

	// 			tilemap = hit.GetComponent<Tilemap>();
	// 			if (tilemap == null)
	// 				continue;

	// 			tileBounds = DetectClosestTileBounds(tilemap, pos, radius);
	// 			if (tileBounds.size == Vector3.zero)
	// 				continue;

	// 			collision = new Collision(Collision.Type.Terrain, tileBounds);
	// 		}
	// 		else if (hit.CompareTag("Enemy"))
	// 		{
	// 			Enemy enemy = hit.GetComponent<Enemy>();
	// 			if (enemy == null)
	// 				continue;

	// 			collision = new Collision(Collision.Type.Enemy, hit.bounds, enemy);
	// 		}
	// 		else
	// 			continue;

	// 		overlap = GetMinOverlap(collision.bounds, pos, radius);
	// 		if (overlap < closestOverlap)
	// 		{
	// 			closestOverlap = overlap;
	// 			closest = collision;
	// 		}
	// 	}

	// 	return closest;
	// }

	public static Collision DetectClosestCollision(Vector2 pos, float radius, Vector2 velocity)
	{
		return DetectClosestCollision(
			pos,
			radius,
			velocity,
			DefaultAllowLowTerrainPassThrough,
			DefaultLowTerrainPassThroughMaxY);
	}

	public static Collision DetectClosestCollision(
		Vector2 pos,
		float radius,
		Vector2 velocity,
		bool allowLowTerrainPassThrough,
		float lowTerrainPassThroughMaxY)
	{
		RaycastHit2D[] hits;
		Collision closest;
		float closestOverlap;
		Vector2 direction;
		float distance;

		direction = velocity.normalized;
		distance = velocity.magnitude * Time.deltaTime;

		hits = Physics2D.CircleCastAll(pos, radius, direction, distance);
		closest = new Collision(Collision.Type.None, default);
		closestOverlap = float.MaxValue;

		foreach (RaycastHit2D hit in hits)
		{
			Collision collision;
			float overlap;

			if (hit.collider.CompareTag("Wall"))
				collision = new Collision(Collision.Type.Wall, hit.collider.bounds);
			else if (hit.collider.CompareTag("Paddle"))
				collision = new Collision(Collision.Type.Paddle, hit.collider.bounds);
			else if (hit.collider.CompareTag("Terrain"))
			{
				Tilemap tilemap;
				Bounds tileBounds;

				tilemap = hit.collider.GetComponent<Tilemap>();
				if (tilemap == null)
					continue;

				tileBounds = DetectClosestTileBounds(tilemap, pos, radius);
				if (tileBounds.size == Vector3.zero)
					continue;

				if (ShouldIgnoreLowTerrainCollision(
					tileBounds,
					tileBounds,
					pos,
					radius,
					velocity,
					allowLowTerrainPassThrough,
					lowTerrainPassThroughMaxY))
				{
					Bounds connectedTerrainBounds = DetectConnectedTerrainBounds(tilemap, tileBounds);
					if (ShouldIgnoreLowTerrainCollision(
						tileBounds,
						connectedTerrainBounds,
						pos,
						radius,
						velocity,
						allowLowTerrainPassThrough,
						lowTerrainPassThroughMaxY))
						continue;
				}

				collision = new Collision(Collision.Type.Terrain, tileBounds);
			}
            else if (hit.collider.CompareTag("Enemy"))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable == null)
                    continue;

                collision = new Collision(Collision.Type.Enemy, hit.collider.bounds, damageable, hit.collider);
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

	public static bool ShouldIgnoreLowTerrainCollision(
		Bounds hitTileBounds,
		Bounds connectedTerrainBounds,
		Vector2 pos,
		float radius,
		Vector2 velocity,
		bool isEnabled,
		float passThroughMaxY)
	{
		if (!isEnabled)
			return false;

		if (velocity.y <= 0f)
			return false;

		if (connectedTerrainBounds.size == Vector3.zero)
			connectedTerrainBounds = hitTileBounds;

		if (connectedTerrainBounds.min.y > passThroughMaxY)
			return false;

		return IsBottomFaceCollision(hitTileBounds, pos, radius, velocity);
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
				Bounds tileBounds;
				float overlap;

				cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);

				if (!tilemap.HasTile(cellPos))
					continue;

				tileBounds = GetTileBounds(tilemap, cellPos);

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

	private static Bounds DetectConnectedTerrainBounds(Tilemap tilemap, Bounds startTileBounds)
	{
		Vector3Int startCell;
		Queue<Vector3Int> open;
		HashSet<Vector3Int> visited;
		Bounds connectedBounds;
		bool hasBounds;

		startCell = tilemap.WorldToCell(startTileBounds.center);
		if (!tilemap.HasTile(startCell))
			return startTileBounds;

		open = new Queue<Vector3Int>();
		visited = new HashSet<Vector3Int>();
		connectedBounds = default;
		hasBounds = false;

		open.Enqueue(startCell);
		visited.Add(startCell);

		while (open.Count > 0)
		{
			Vector3Int cell;
			Bounds tileBounds;

			cell = open.Dequeue();
			tileBounds = GetTileBounds(tilemap, cell);
			if (!hasBounds)
			{
				connectedBounds = tileBounds;
				hasBounds = true;
			}
			else
			{
				connectedBounds.Encapsulate(tileBounds);
			}

			for (int i = 0; i < ConnectedTileOffsets.Length; ++i)
			{
				Vector3Int neighbor = cell + ConnectedTileOffsets[i];
				if (visited.Contains(neighbor) || !tilemap.HasTile(neighbor))
					continue;

				visited.Add(neighbor);
				open.Enqueue(neighbor);
			}
		}

		return hasBounds ? connectedBounds : startTileBounds;
	}

	private static Bounds GetTileBounds(Tilemap tilemap, Vector3Int cellPos)
	{
		Vector3 worldPos = tilemap.CellToWorld(cellPos) + tilemap.cellSize / 2f;
		return new Bounds(worldPos, tilemap.cellSize);
	}

	private static bool IsBottomFaceCollision(Bounds bounds, Vector2 pos, float radius, Vector2 velocity)
	{
		float overlapLeft;
		float overlapRight;
		float overlapBottom;
		float overlapTop;
		float minOverlap;
		float epsilon;

		if (velocity.y <= 0f)
			return false;

		overlapLeft = (pos.x + radius) - bounds.min.x;
		overlapRight = bounds.max.x - (pos.x - radius);
		overlapBottom = (pos.y + radius) - bounds.min.y;
		overlapTop = bounds.max.y - (pos.y - radius);
		minOverlap = Mathf.Min(overlapLeft, overlapRight, overlapBottom, overlapTop);
		epsilon = 0.001f;

		return Mathf.Abs(minOverlap - overlapBottom) <= epsilon;
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

	private (int damage, bool isCrit) CalculateDamage()
	{
		float damage = this.ball.Stats.AttackPower;
		bool isCrit = Random.value < this.ball.Stats.CriticalChance;
		if (isCrit)
			damage *= this.ball.Stats.CriticalDamage;
		return (Mathf.Max(1, Mathf.RoundToInt(damage)), isCrit);
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
