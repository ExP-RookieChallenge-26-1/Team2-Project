using UnityEngine;

[RequireComponent(typeof(BallCollision))]
public class Ball : MonoBehaviour
{
	public BallStats Stats { get; private set; }
	public BallPhysics Physics { get; private set; }
	public BallCollision Collision { get; private set; }
	public BallTrajectory Trajectory { get; private set; }
	public GiantSkill GiantSKill { get; private set; }

	private void Awake()
	{
		this.Collision = GetComponent<BallCollision>();
		this.Trajectory = GetComponent<BallTrajectory>();
		this.Physics = new BallPhysics(this);
		this.GiantSKill = GetComponent<GiantSkill>();
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.BallStats;
		this.Physics.Launch();
	}

	private void Update()
	{
		this.Collision.Tick();
		this.Trajectory.Tick();
		this.Physics.Tick();
		this.GiantSKill.Tick();
		UpdateScale();
		CheckGameOver();
	}

	private void UpdateScale()
	{
		float diameter;

		diameter = this.Stats.Radius * 2f;
		transform.localScale = new Vector3(diameter, diameter, 1f);
	}

	private void CheckGameOver()
	{
		if (transform.position.y < -6f)
			GameManager.Instance.State.Change(GameStateMachine.State.GameOver);
	}
}
