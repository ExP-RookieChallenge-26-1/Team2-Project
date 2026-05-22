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

	private void Awake()
	{
		this.Collision = GetComponent<BallCollision>();
		this.Trajectory = GetComponent<BallTrajectory>();
		this.Physics = new BallPhysics(this);
		this.GiantSkill = GetComponent<GiantSkill>();
		this.CloneSkill = GetComponent<CloneSkill>();
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.BallStats;

		float speed = this.Stats.Speed;
		if (this.Physics.Velocity == Vector2.zero)
			this.Physics.SetVelocity(new Vector2(speed * 0.5f, speed * 0.866f));
	}

	private void Update()
	{
		this.Collision.Tick();
		this.Trajectory.Tick();
		this.Physics.Tick();
		this.GiantSkill.Tick();
		this.CloneSkill.Tick();
		UpdateScale();
		CheckOutOfBounds();
	}

	private void UpdateScale()
	{
		float diameter;

		diameter = this.Stats.Radius * 2f;
		transform.localScale = new Vector3(diameter, diameter, 1f);
	}

	private void CheckOutOfBounds()
	{
		if (transform.position.y < -6f)
			Destroy(gameObject);
	}

	private void OnDestroy()
	{
		GameManager.Instance.OnBallDestroyed();
	}
}
