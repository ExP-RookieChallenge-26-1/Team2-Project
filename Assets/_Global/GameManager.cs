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
	public User User { get; private set; }

	[SerializeField] private BallStats ballStatsAsset;
	[SerializeField] private PaddleStats paddleStatsAsset;
	[SerializeField] private WorldStats worldStatsAsset;
	[SerializeField] private Ball ballPrefab;
	[SerializeField] private Paddle paddle;
    [SerializeField] private GameObject gameClearPanel;
    public Paddle Paddle => this.paddle;

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
		this.User = FindFirstObjectByType<User>();
		this.paddle = FindFirstObjectByType<Paddle>();
		TriggerSpawn();
	}

	private void Update()
	{
		this.Input.Tick();
		CheckBallState();
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

	private void CheckBallState()
	{
		if (this.State.Current != GameStateMachine.State.Playing)
			return;

		if (FindObjectsByType<Ball>(FindObjectsSortMode.None).Length > 0)
			return;

		this.User.Health.TakeDamage(1);

		if (this.User.Health.CurrentHp > 0)
			TriggerSpawn();
	}

	private void TriggerSpawn()
	{
		if (this.paddle == null)
		{
			Debug.LogError("Paddle not found!");
			return;
		}

		float centerX = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).x;
		Vector3 spawnPos = new Vector3(centerX, this.paddle.transform.position.y + 1f, 0f);
		Ball ball = Instantiate(this.ballPrefab, spawnPos, Quaternion.identity);
		ball.Spawn();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayRespawnSound();
    }

    public void OnBossDefeated()
    {
        Debug.Log("게임 클리어");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameClearSound();

        if (gameClearPanel != null)
            gameClearPanel.SetActive(true);

        Time.timeScale = 0f;
    }
    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}
