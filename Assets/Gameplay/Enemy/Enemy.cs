using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour
{
	public enum Heading { Left, Right }

	[SerializeField] private EnemyStats statsAsset;

	public EnemyStats Stats { get; private set; }

	// min = left end X, max = right end X
	public (float min, float max) MoveRange { get; private set; }

	private IEnemyState currentState;
	public IdleState IdleState { get; private set; }
	public MoveState MoveLeftState { get; private set; }
	public MoveState MoveRightState { get; private set; }
	public TrackState TrackState { get; private set; }
	public AttackState AttackState { get; private set; }

	public bool HasAttacked { get; private set; }
	public void MarkAttacked() => this.HasAttacked = true;

	private int currentHp;
	private bool isDead;
	private Heading heading = Heading.Right;
	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		this.Stats = Instantiate(this.statsAsset);
		this.spriteRenderer = GetComponent<SpriteRenderer>();

		InitializeStates();
	}

	private void Start()
	{
		this.currentHp = this.Stats.MaxHp;

		Tilemap tilemap = transform.parent.GetComponentInChildren<Tilemap>();
		MeasureMoveRange(tilemap);

		ChangeState(this.IdleState);
	}

	private void Update()
	{
		CheckTrackTransition();
		this.currentState?.Tick(this);
	}

	private void CheckTrackTransition()
	{
		if (this.currentState == this.TrackState || this.currentState == this.AttackState)
			return;
		if (this.HasAttacked)
			return;
		Debug.Log($"[Enemy] Y={transform.position.y:F2}, Threshold={this.Stats.TrackYThreshold:F2}, willTrack={transform.position.y < this.Stats.TrackYThreshold}");
		if (transform.position.y < this.Stats.TrackYThreshold)
			ChangeState(this.TrackState);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (this.currentState != this.TrackState) return;
		if (!other.CompareTag("Paddle")) return;
		ChangeState(this.AttackState);
	}

	private void InitializeStates()
	{
		this.IdleState = new IdleState();
		this.MoveLeftState = new MoveState();
		this.MoveRightState = new MoveState();
		this.TrackState = new TrackState();
		this.AttackState = new AttackState();
	}

	public void ChangeState(IEnemyState newState)
	{
		if (newState == null || this.currentState == newState) return;
		this.currentState?.Exit(this);
		this.currentState = newState;
		this.currentState.Enter(this);
	}

	private void MeasureMoveRange(Tilemap tilemap)
	{
		if (tilemap == null)
		{
			this.MoveRange = (transform.position.x - 5f, transform.position.x + 5f);
			return;
		}

		// 처음 만나는 타일 Y까지 최대 3칸 탐색
		Vector3Int cell = tilemap.WorldToCell(transform.position);
		for (int i = 0; i < 3 && !tilemap.HasTile(cell); i++)
			cell += Vector3Int.down;

		Vector3Int leftCell = cell;
		while (tilemap.HasTile(leftCell + Vector3Int.left))
			leftCell += Vector3Int.left;

		Vector3Int rightCell = cell;
		while (tilemap.HasTile(rightCell + Vector3Int.right))
			rightCell += Vector3Int.right;

		this.MoveRange = (
			min: tilemap.CellToWorld(leftCell).x,
			max: tilemap.CellToWorld(rightCell).x + tilemap.cellSize.x
		);
		Debug.Log($"[Enemy] MoveRange measured: ({this.MoveRange.min:F2}, {this.MoveRange.max:F2}), spawnX={transform.position.x:F2}, startCell={cell}");
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Vector3 left  = new Vector3(this.MoveRange.min, transform.position.y, 0f);
		Vector3 right = new Vector3(this.MoveRange.max, transform.position.y, 0f);
		Gizmos.DrawLine(left, right);
		Gizmos.DrawSphere(left,  0.1f);
		Gizmos.DrawSphere(right, 0.1f);
	}

	public void SetHeading(Heading newHeading)
	{
		if (this.heading == newHeading) return;
		this.heading = newHeading;
		this.spriteRenderer.flipX = (newHeading == Heading.Left);
	}

	public void TakeDamage(int damage, bool isCrit = false)
	{
		if (this.isDead) return;
		damage = Mathf.Max(1, damage);
		this.currentHp -= damage;
		Color color = isCrit ? Color.yellow : Color.white;
		DamageTextSpawner.Instance.Spawn(transform.position, damage, color);
		if (this.currentHp <= 0) Die();
	}

	private void Die()
	{
		this.isDead = true;
		GameManager.Instance.User.Level.AddExp(this.Stats.ExpReward);
		Destroy(gameObject);
	}
}
