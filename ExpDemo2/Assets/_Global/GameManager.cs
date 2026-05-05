using UnityEngine;

[RequireComponent(typeof(GameStateMachine))]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	public GameStateMachine State { get; private set; }
	public InputState Input { get; private set; }
	public BallStats BallStats { get; private set; }
	public PaddleStats PaddleStats { get; private set; }
	public WorldStats WorldStats { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		this.State = GetComponent<GameStateMachine>();
		this.Input = new InputState();
		this.BallStats = new BallStats();
		this.PaddleStats = new PaddleStats();
		this.WorldStats = new WorldStats();
	}

	private void Update()
	{
		this.Input.Tick();
	}
}
