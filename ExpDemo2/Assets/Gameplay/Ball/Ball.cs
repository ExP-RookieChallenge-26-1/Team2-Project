using UnityEngine;

[RequireComponent(typeof(BallCollision))]
public class Ball : MonoBehaviour
{
	public BallStats Stats { get; private set; }
	public BallPhysics Physics { get; private set; }
	public BallCollision Collision { get; private set; }

	private void Awake()
	{
		this.Collision = GetComponent<BallCollision>();
		this.Physics = new BallPhysics(this);
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.BallStats;
		this.Physics.Launch();
	}

	private void Update()
	{
		this.Collision.Tick();
		this.Physics.Tick(Time.deltaTime);
		CheckGameOver();
	}

	private void CheckGameOver()
	{
		if (transform.position.y < -6f)
			GameManager.Instance.State.Change(GameState.GameOver);
	}
}
