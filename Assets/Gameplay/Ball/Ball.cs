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

	public BallAnimation Animation { get; private set; }
	private bool isSpawning = false;
	private float spawnTimer = 0f;
	private const float SpawnDelay = 3f;
	public bool IsRespawning => this.isSpawning;

	private bool isGameOverFalling = false;
	[SerializeField] private float gameOverFallSpeed = 5f;
	[SerializeField] private float gameOverSpinSpeed = 360f;

	private void Awake()
	{
		this.Collision = GetComponent<BallCollision>();
		this.Trajectory = GetComponent<BallTrajectory>();
		this.Physics = new BallPhysics(this);
		this.GiantSkill = GetComponent<GiantSkill>();
		this.CloneSkill = GetComponent<CloneSkill>();
		this.Stats = GameManager.Instance.BallStats;

		this.Animation = new BallAnimation(this);
		SetSkillStats(
			this.Stats.Skill1HasManualTrigger,
			this.Stats.Skill1HasAutoTrigger,
			this.Stats.Skill1ManualCooldown,
			this.Stats.Skill1AutoCooldown,
			this.Stats.Skill2HasManualTrigger,
			this.Stats.Skill2HasAutoTrigger,
			this.Stats.Skill2ManualCooldown,
			this.Stats.Skill2AutoCooldown,
			this.Stats.Skill1Duration,
			this.Stats.Skill2Duration,
			this.Stats.Skill1SizeLevel,
			this.Stats.Skill2CloneLevel);
	}

	private void Update()
	{
		if (this.isGameOverFalling)
		{
			TickGameOverFall();
			return;
		}

		if (this.isSpawning)
		{
			TickSpawn();
			return;
		}

		this.Collision.Tick();
		this.Trajectory.Tick();
		this.Physics.Tick();
		this.Animation.Tick();
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
		this.Animation.SetRespawning(true);
	}

	public void LaunchImmediately()
	{
		this.isSpawning = false;
		this.spawnTimer = 0f;
		this.Animation.SetRespawning(false);
		Launch();
	}

	public void StartGameOverFall()
	{
		this.isGameOverFalling = true;
		this.Physics.SetVelocity(Vector2.zero);
		this.Animation.SetGameOver();
	}

	public void SetSkillTriggerSettings(
		bool skill1HasManualTrigger,
		bool skill1HasAutoTrigger,
		bool skill2HasManualTrigger,
		bool skill2HasAutoTrigger)
	{
		if (this.GiantSkill != null)
			this.GiantSkill.SetTriggerSettings(skill1HasManualTrigger, skill1HasAutoTrigger);

		if (this.CloneSkill != null)
			this.CloneSkill.SetTriggerSettings(skill2HasManualTrigger, skill2HasAutoTrigger);
	}

	public void SetSkillCooldowns(float skill1ManualCooldown, float skill1AutoCooldown, float skill2ManualCooldown, float skill2AutoCooldown)
	{
		if (this.GiantSkill != null)
		{
			this.GiantSkill.SetManualCooldown(skill1ManualCooldown);
			this.GiantSkill.SetAutoCooldown(skill1AutoCooldown);
		}

		if (this.CloneSkill != null)
		{
			this.CloneSkill.SetManualCooldown(skill2ManualCooldown);
			this.CloneSkill.SetAutoCooldown(skill2AutoCooldown);
		}
	}

	public void SetSkillStats(
		bool skill1HasManualTrigger,
		bool skill1HasAutoTrigger,
		float skill1ManualCooldown,
		float skill1AutoCooldown,
		bool skill2HasManualTrigger,
		bool skill2HasAutoTrigger,
		float skill2ManualCooldown,
		float skill2AutoCooldown,
		float skill1Duration,
		float skill2Duration,
		int skill1SizeLevel,
		int skill2CloneLevel)
	{
		SetSkillCooldowns(
			skill1ManualCooldown,
			skill1AutoCooldown,
			skill2ManualCooldown,
			skill2AutoCooldown);
		SetSkillTriggerSettings(
			skill1HasManualTrigger,
			skill1HasAutoTrigger,
			skill2HasManualTrigger,
			skill2HasAutoTrigger);

		if (this.GiantSkill != null)
		{
			this.GiantSkill.SetDuration(skill1Duration);
			this.GiantSkill.SetSizeLevel(skill1SizeLevel);
		}

		if (this.CloneSkill != null)
		{
			this.CloneSkill.SetDuration(skill2Duration);
			this.CloneSkill.SetCloneLevel(skill2CloneLevel);
		}
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
		this.Animation.SetRespawning(false);
		// 아래 방향(270°) 기준 ±30° → 240°~300°
		float angleDeg = Random.Range(240f, 300f);
		float angleRad = angleDeg * Mathf.Deg2Rad;
		this.Physics.SetVelocity(
			new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * this.Stats.Speed
		);
	}

	private void TickGameOverFall()
	{
		float dt = Time.unscaledDeltaTime;
		transform.position += Vector3.down * this.gameOverFallSpeed * dt;
		transform.Rotate(0f, 0f, this.gameOverSpinSpeed * dt);
		if (transform.position.y < -10f)
			Destroy(gameObject);
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
