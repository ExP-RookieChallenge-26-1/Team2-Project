using UnityEngine;

public class Paddle : MonoBehaviour
{
	public PaddleStats Stats { get; private set; }
	public PaddlePhysics Physics { get; private set; }

	private void Awake()
	{
		this.Physics = new PaddlePhysics(this);
	}

	private void Start()
	{
		this.Stats = GameManager.Instance.PaddleStats;
	}

	private void Update()
	{
		this.Physics.Tick();
	}
}