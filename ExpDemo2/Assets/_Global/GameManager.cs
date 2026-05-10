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

	[SerializeField] private BallStats ballStatsAsset;
	[SerializeField] private PaddleStats paddleStatsAsset;
	[SerializeField] private WorldStats worldStatsAsset;

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
		this.BallStats = Instantiate(this.ballStatsAsset);
		this.PaddleStats = Instantiate(this.paddleStatsAsset);
		this.WorldStats = Instantiate(this.worldStatsAsset);
	}

	private void Update()
	{
		this.Input.Tick();
	}
}
