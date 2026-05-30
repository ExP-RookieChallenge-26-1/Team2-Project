using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameStateMachine))]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	public GameStateMachine State { get; private set; }
	public InputState Input { get; private set; }
	public BallStats BallStats { get; private set; }
	public PaddleStats PaddleStats { get; private set; }
	public WorldStats WorldStats { get; private set; }
	public LevelSystem LevelSystem { get; private set; }

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

	private void Start()
	{
		this.State.OnChanged += OnGameStateChanged;
		this.LevelSystem = FindFirstObjectByType<LevelSystem>();
	}

	private void Update()
	{
		this.Input.Tick();
	}

	private void OnDestroy()
	{
		if (this.State != null)
			this.State.OnChanged -= OnGameStateChanged;
	}

	private void OnGameStateChanged(GameStateMachine.State newState)
	{
		switch (newState)
		{
			case GameStateMachine.State.Playing:
				Time.timeScale = 1f;
				break;
			case GameStateMachine.State.Enhancement:
				Time.timeScale = 0.2f;
				break;
			case GameStateMachine.State.GameOver:
				Time.timeScale = 1f;
				SceneManager.LoadScene("TitleScene");
				break;
		}
	}

	public void OnBallDestroyed()
	{
		if (FindObjectsByType<Ball>(FindObjectsSortMode.None).Length == 0)
			this.State.Change(GameStateMachine.State.GameOver);
	}
}