using UnityEngine;

[RequireComponent(typeof(BallCollision))]
public class Ball : MonoBehaviour
{
	public BallStats Stats { get; private set; }
	public BallPhysics Physics { get; private set; }
	public BallCollision Collision { get; private set; }
	public BallTrajectory Trajectory { get; private set; }
	public GiantSkill GiantSkill { get; private set; }
	public CloneSkill CloneSkill { get; private set; }

	private BallAnimation animation;
	private bool isSpawning = false;
	private float spawnTimer = 0f;
	private const float SpawnDelay = 3f;

	private void Awake()
	{
		this.Collision = GetComponent<BallCollision>();
		this.Trajectory = GetComponent<BallTrajectory>();
		this.Physics = new BallPhysics(this);
		this.GiantSkill = GetComponent<GiantSkill>();
		this.CloneSkill = GetComponent<CloneSkill>();

		Animator animator = GetComponent<Animator>();
		this.animation = new BallAnimation(animator, this);
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.BallStats;
	}

	private void Update()
	{
		if (this.isSpawning)
		{
			TickSpawn();
			return;
		}

		this.Collision.Tick();
		this.Trajectory.Tick();
		this.Physics.Tick();
		this.animation.Tick();
		this.GiantSkill.Tick();
		this.CloneSkill.Tick();
		UpdateScale();
		CheckOutOfBounds();
	}

	public void Spawn()
	{
		this.isSpawning = true;
		this.spawnTimer = 0f;
		this.Physics.SetVelocity(Vector2.zero);
	}

	private void TickSpawn()
	{
		this.spawnTimer += Time.deltaTime;
		if (this.spawnTimer >= SpawnDelay)
		{
			this.isSpawning = false;
			Launch();
		}
	}

	private void Launch()
	{
		// 아래 방향(270°) 기준 ±30° → 240°~300°
		float angleDeg = Random.Range(240f, 300f);
		float angleRad = angleDeg * Mathf.Deg2Rad;
		this.Physics.SetVelocity(
			new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * this.Stats.Speed
		);
	}

	private void UpdateScale()
	{
		float diameter = this.Stats.Radius * 2f;
		transform.localScale = new Vector3(diameter, diameter, 1f);
	}

	private void CheckOutOfBounds()
	{
		if (transform.position.y < -5f)
		{
			Debug.Log($"공이 화면 아래로 떨어짐: {transform.position}");
			Destroy(gameObject);
		}
	}
}
