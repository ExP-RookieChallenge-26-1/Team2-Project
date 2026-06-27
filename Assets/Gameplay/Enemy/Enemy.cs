using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour, IDamageable
{
	private const int GroundSearchHorizontalRadius = 1;
	private const int GroundSearchVerticalRadius = 24;
	private const float GroundSearchAboveSpawnToleranceCells = 0.25f;
	private const float DefaultGroundVisualInset = 0.06f;
	private const float DefaultMovementFootprintHalfWidth = 0.5f;

	public enum Heading { Left, Right }

	[SerializeField] private EnemyStats statsAsset;
    [SerializeField] private float encounterDuration = 0.8f;
    [SerializeField, Min(0f)] private float groundVisualInset = DefaultGroundVisualInset;
    [SerializeField, Min(0f)] private float movementFootprintHalfWidth = DefaultMovementFootprintHalfWidth;

    public EnemyStats Stats { get; private set; }

	// min = left end X, max = right end X
	public (float min, float max) MoveRange { get; private set; }

	private IEnemyState currentState;
	public IdleState IdleState { get; private set; }
	public MoveState MoveLeftState { get; private set; }
	public MoveState MoveRightState { get; private set; }
	public TrackState TrackState { get; private set; }
	public AttackState AttackState { get; private set; }

    public EncounterState EncounterState { get; private set; }

    public bool HasAttacked { get; private set; }
	public void MarkAttacked() => this.HasAttacked = true;
    public float EncounterDuration => this.encounterDuration;

    private int currentHp;
	private bool isDead;
    private Heading heading = Heading.Left;
	private SpriteRenderer spriteRenderer;
    private Animator animator;
    private MonsterHealthBar healthBar;
    private MobDamageOverlay damageOverlay;

    private void Awake()
	{
		this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.animator = GetComponent<Animator>();
        this.healthBar = GetComponent<MonsterHealthBar>();
        if (this.healthBar == null)
            this.healthBar = gameObject.AddComponent<MonsterHealthBar>();
        this.healthBar.SetHideAtZeroHealth(false);
        this.damageOverlay = GetComponent<MobDamageOverlay>();
        if (this.damageOverlay == null)
            this.damageOverlay = gameObject.AddComponent<MobDamageOverlay>();

        InitializeStates();
	}

	public void Initialize(EnemyStats statsAsset)
	{
		if (statsAsset == null)
			return;

		DestroyRuntimeObject(this.Stats);
		this.statsAsset = statsAsset;
		this.Stats = Instantiate(statsAsset);
	}

	private void OnDestroy()
	{
		DestroyRuntimeObject(this.Stats);
		this.Stats = null;
	}

	private void Start()
	{
		if (this.Stats == null)
			Initialize(this.statsAsset);

		if (this.Stats == null)
		{
			Debug.LogError("Enemy stats asset is missing.", this);
			this.enabled = false;
			return;
		}

		this.currentHp = this.Stats.MaxHp;
        this.healthBar.Configure(new Vector2(1.15f, 0.2f), 0.18f);
        this.healthBar.SetHealth(this.currentHp, this.Stats.MaxHp);

		Tilemap tilemap = transform.parent != null
			? transform.parent.GetComponentInChildren<Tilemap>()
			: null;
		SnapToGround(tilemap);
		MeasureMoveRange(tilemap);

		ChangeState(this.IdleState);
	}

	private void Update()
	{
		if (this.isDead)
			return;

		CheckTrackTransition();
		this.currentState?.Tick(this);
	}

	private void CheckTrackTransition()
	{
		if (this.isDead)
			return;
		if (this.currentState == this.TrackState || this.currentState == this.AttackState)
			return;
		if (this.HasAttacked)
			return;
        if (transform.position.y < this.Stats.TrackYThreshold)
        {
            FacePaddle();
            ChangeState(this.EncounterState);
        }
    }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (this.isDead) return;
		if (this.currentState != this.TrackState) return;
		if (!other.CompareTag("Paddle")) return;
		ChangeState(this.AttackState);
	}

    public bool IsPaddleOverlappingAttackCollider()
    {
        if (GameManager.Instance == null || GameManager.Instance.Paddle == null)
            return false;

        Collider2D enemyCollider = GetComponent<Collider2D>();
        Collider2D paddleCollider = GameManager.Instance.Paddle.GetComponent<Collider2D>();
        if (enemyCollider == null || paddleCollider == null)
            return false;
        if (!enemyCollider.enabled || !paddleCollider.enabled)
            return false;

        Physics2D.SyncTransforms();
        return enemyCollider.Distance(paddleCollider).isOverlapped;
    }

	private void InitializeStates()
	{
        this.IdleState = new IdleState();
        this.MoveLeftState = new MoveState();
        this.MoveRightState = new MoveState();
        this.EncounterState = new EncounterState();
        this.TrackState = new TrackState();
        this.AttackState = new AttackState();
    }

	public void ChangeState(IEnemyState newState)
	{
		if (this.isDead || newState == null || this.currentState == newState) return;
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

		if (!TryFindGroundCell(tilemap, out Vector3Int cell))
		{
			this.MoveRange = (transform.position.x, transform.position.x);
			return;
		}

		Vector3Int leftCell = cell;
		while (IsTopSurfaceTile(tilemap, leftCell + Vector3Int.left))
			leftCell += Vector3Int.left;

		Vector3Int rightCell = cell;
		while (IsTopSurfaceTile(tilemap, rightCell + Vector3Int.right))
			rightCell += Vector3Int.right;

		float leftEdge = tilemap.CellToWorld(leftCell).x;
		float rightEdge = tilemap.CellToWorld(rightCell).x + tilemap.cellSize.x;
		float halfWidth = GetMovementFootprintHalfWidth();
		float min = leftEdge + halfWidth;
		float max = rightEdge - halfWidth;

		if (min > max)
		{
			float center = (leftEdge + rightEdge) * 0.5f;
			min = center;
			max = center;
		}

		this.MoveRange = (min, max);
		ClampToMoveRange();
	}

	private void SnapToGround(Tilemap tilemap)
	{
		if (tilemap == null || !TryFindGroundCell(tilemap, out Vector3Int groundCell))
			return;

		float groundTopY = GetCellTopY(tilemap, groundCell);
		float bottomY = GetMovementBounds().min.y;
		Vector3 position = transform.position;
		position.y += groundTopY - bottomY - this.groundVisualInset;
		transform.position = position;
		Physics2D.SyncTransforms();
	}

	private bool TryFindGroundCell(Tilemap tilemap, out Vector3Int groundCell)
	{
		Vector3Int startCell = tilemap.WorldToCell(transform.position);
		Vector3 probePosition = transform.position;
		float highestAllowedTopY = probePosition.y + tilemap.cellSize.y * GroundSearchAboveSpawnToleranceCells;
		float bestScore = float.PositiveInfinity;
		groundCell = default;

		for (int xOffset = -GroundSearchHorizontalRadius; xOffset <= GroundSearchHorizontalRadius; ++xOffset)
		{
			for (int yOffset = -GroundSearchVerticalRadius; yOffset <= GroundSearchVerticalRadius; ++yOffset)
			{
				Vector3Int candidate = new Vector3Int(
					startCell.x + xOffset,
					startCell.y + yOffset,
					startCell.z);
				if (!IsTopSurfaceTile(tilemap, candidate))
					continue;

				Vector3 topCenter = GetCellTopCenter(tilemap, candidate);
				if (topCenter.y > highestAllowedTopY)
					continue;

				float score = (topCenter - probePosition).sqrMagnitude;
				if (score >= bestScore)
					continue;

				bestScore = score;
				groundCell = candidate;
			}
		}

		return bestScore < float.PositiveInfinity;
	}

	private static bool IsTopSurfaceTile(Tilemap tilemap, Vector3Int cell)
	{
		return tilemap.HasTile(cell) && !tilemap.HasTile(cell + Vector3Int.up);
	}

	private static Vector3 GetCellTopCenter(Tilemap tilemap, Vector3Int cell)
	{
		Vector3 topCenter = tilemap.GetCellCenterWorld(cell);
		topCenter.y = GetCellTopY(tilemap, cell);
		return topCenter;
	}

	private static float GetCellTopY(Tilemap tilemap, Vector3Int cell)
	{
		return tilemap.CellToWorld(cell + Vector3Int.up).y;
	}

	private void ClampToMoveRange()
	{
		Vector3 position = transform.position;
		position.x = Mathf.Clamp(position.x, this.MoveRange.min, this.MoveRange.max);
		transform.position = position;
		Physics2D.SyncTransforms();
	}

	private float GetMovementFootprintHalfWidth()
	{
		float configuredHalfWidth = Mathf.Max(0f, this.movementFootprintHalfWidth);
		float boundsHalfWidth = GetMovementBounds().extents.x;
		return Mathf.Max(configuredHalfWidth, boundsHalfWidth);
	}

	private Bounds GetMovementBounds()
	{
		Collider2D collider = GetComponent<Collider2D>();
		if (collider != null)
		{
			Physics2D.SyncTransforms();
			return collider.bounds;
		}

		if (this.spriteRenderer != null)
			return this.spriteRenderer.bounds;

		return new Bounds(transform.position, Vector3.zero);
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
		this.spriteRenderer.flipX = (newHeading == Heading.Right);
	}

    public bool TryGetPaddleDirection(out bool isLeft)
    {
        isLeft = false;

        if (GameManager.Instance == null || GameManager.Instance.Paddle == null)
            return false;

        isLeft = GameManager.Instance.Paddle.transform.position.x < transform.position.x;
        return true;
    }

    public bool FacePaddle()
    {
        if (!TryGetPaddleDirection(out bool isLeft))
            return false;

        SetHeading(isLeft ? Heading.Left : Heading.Right);
        return true;
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (this.isDead) return;

        damage = Mathf.Max(1, damage);
        int previousHp = this.currentHp;
        this.currentHp = Mathf.Max(0, this.currentHp - damage);
        ScoreManager.AddDamageScoreToSession(previousHp, this.currentHp, this.Stats.MaxHp);
        this.healthBar.SetHealth(this.currentHp, this.Stats.MaxHp);

        Color color = isCrit ? Color.yellow : Color.white;
        DamageTextSpawner.Instance?.Spawn(transform.position, damage, color);
        this.damageOverlay?.Play();

        if (this.currentHp <= 0)
        {
            Die();
        }
        else
        {
            PlayDamagedAnimation();
        }
    }

    private void Die()
    {
        if (this.isDead) return;

        this.isDead = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMobDieSound();

        this.currentState = null;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        PlayDieAnimation();
        GameManager.Instance.User.Level.AddExp(this.Stats.ExpReward);
        StartCoroutine(DieSequence());
    }

    private System.Collections.IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    public void SetMoveAnimation(bool isMoving)
    {
        if (this.animator != null)
            this.animator.SetBool("IsMoving", isMoving);
    }

    public void PlayEncounterAnimation()
    {
        if (this.isDead) return;

        if (this.animator != null)
            this.animator.SetTrigger("Encounter");
    }

    public void PlayAttackAnimation()
    {
        if (this.isDead) return;

        if (this.animator != null)
            this.animator.SetTrigger("Attack");
    }

    public void OnAttackAnimationFinished()
    {
        if (this.currentState == this.AttackState)
            this.AttackState.CompleteAttack(this);
    }

    public void PlayDamagedAnimation()
    {
        if (this.isDead) return;

        if (this.animator != null)
            this.animator.SetTrigger("Damaged");
    }

	    public void PlayDieAnimation()
	    {
	        if (this.animator == null)
	            return;

        this.animator.SetBool("IsMoving", false);
        this.animator.ResetTrigger("Encounter");
        this.animator.ResetTrigger("Attack");
	        this.animator.ResetTrigger("Damaged");
	        this.animator.SetTrigger("Die");
	    }

	private static void DestroyRuntimeObject(Object target)
	{
		if (target == null)
			return;

		if (Application.isPlaying)
			Destroy(target);
		else
			DestroyImmediate(target);
	}
}
