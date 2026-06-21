using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour, IDamageable
{
	public enum Heading { Left, Right }

	[SerializeField] private EnemyStats statsAsset;
    [SerializeField] private float encounterDuration = 0.8f;

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
	private Heading heading = Heading.Right;
	private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
	{
		this.Stats = Instantiate(this.statsAsset);
		this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.animator = GetComponent<Animator>();

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
        if (transform.position.y < this.Stats.TrackYThreshold)
            ChangeState(this.EncounterState);
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
        this.EncounterState = new EncounterState();
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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyHitSound();

        Color color = isCrit ? Color.yellow : Color.white;
        DamageTextSpawner.Instance.Spawn(transform.position, damage, color);

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
        if (this.animator != null)
            this.animator.SetTrigger("Encounter");
    }

    public void PlayAttackAnimation()
    {
        if (this.animator != null)
            this.animator.SetTrigger("Attack");
    }

    public void PlayDamagedAnimation()
    {
        if (this.animator != null)
            this.animator.SetTrigger("Damaged");
    }

    public void PlayDieAnimation()
    {
        if (this.animator != null)
            this.animator.SetTrigger("Die");
    }
}
