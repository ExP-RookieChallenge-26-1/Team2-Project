using System.Collections;
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
    [SerializeField] private GameObject gameOverPanel;
    [Header("Enhancement Time Slow")]
    [SerializeField] private float enhancementInitialTimeScale = 0.2f;
    [SerializeField] private float enhancementSlowdownRate = 2f;
    [SerializeField] private float enhancementMinimumTimeScale = 0.001f;

    private bool isEnhancementTimeSlowActive;
    private float enhancementElapsedSeconds;

    public Paddle Paddle => this.paddle;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
        ResetSessionState();
		Instance = this;

		this.State = GetComponent<GameStateMachine>();
		this.Input = new InputState();
		this.BallStats = Instantiate(this.ballStatsAsset);
		this.PaddleStats = Instantiate(this.paddleStatsAsset);
		this.WorldStats = Instantiate(this.worldStatsAsset);
		this.User = FindFirstObjectByType<User>();
		this.paddle = FindFirstObjectByType<Paddle>();
		if (GetComponent<ScoreManager>() == null)
			gameObject.AddComponent<ScoreManager>();
	}

    public static void ResetSessionState()
    {
        Time.timeScale = 1f;
        ScoreManager.ResetSessionScore();
        BossSpawnTrigger.ResetSessionState();
    }

	private void Start()
	{
		this.State.OnChanged += OnGameStateChanged;

        this.User = FindFirstObjectByType<User>();
        this.paddle = FindFirstObjectByType<Paddle>();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameBgm();
        TriggerInitialSpawn();

	}

	private void Update()
	{
		this.Input.Tick();
        TickEnhancementTimeSlow();
		CheckBallState();
	}

	private void OnDestroy()
	{
		if (this.State != null)
			this.State.OnChanged -= OnGameStateChanged;

		DestroyRuntimeObject(this.BallStats);
		DestroyRuntimeObject(this.PaddleStats);
		DestroyRuntimeObject(this.WorldStats);
		this.BallStats = null;
		this.PaddleStats = null;
		this.WorldStats = null;

		if (Instance == this)
			Instance = null;
	}

	private void OnGameStateChanged(GameStateMachine.State newState)
	{
        switch (newState)
		{
			case GameStateMachine.State.Playing:
                StopEnhancementTimeSlow();
				Time.timeScale = 1f;
				break;
			case GameStateMachine.State.Enhancement:
                StartEnhancementTimeSlow();
				break;
            case GameStateMachine.State.GameOver:
                StopEnhancementTimeSlow();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayGameOverSound();

                foreach (var ball in FindObjectsByType<Ball>(FindObjectsSortMode.None))
                    ball.StartGameOverFall();
                Time.timeScale = 0f;
                StartCoroutine(ShowGameOverPanelWhenBallsGone());
                break;
	        }
		}

    private void StartEnhancementTimeSlow()
    {
        this.enhancementElapsedSeconds = 0f;
        this.isEnhancementTimeSlowActive = true;
        Time.timeScale = CalculateEnhancementTimeScale(
            this.enhancementElapsedSeconds,
            this.enhancementInitialTimeScale,
            this.enhancementSlowdownRate,
            this.enhancementMinimumTimeScale);
    }

    private void StopEnhancementTimeSlow()
    {
        this.isEnhancementTimeSlowActive = false;
        this.enhancementElapsedSeconds = 0f;
    }

    private void TickEnhancementTimeSlow()
    {
        if (!this.isEnhancementTimeSlowActive || this.State.Current != GameStateMachine.State.Enhancement)
            return;

        if (Time.timeScale <= 0f)
            return;

        this.enhancementElapsedSeconds += Time.unscaledDeltaTime;
        Time.timeScale = CalculateEnhancementTimeScale(
            this.enhancementElapsedSeconds,
            this.enhancementInitialTimeScale,
            this.enhancementSlowdownRate,
            this.enhancementMinimumTimeScale);
    }

    public static float CalculateEnhancementTimeScale(float elapsedSeconds, float initialScale, float slowdownRate, float minimumScale)
    {
        float safeInitialScale = Mathf.Max(0f, initialScale);
        float safeMinimumScale = Mathf.Min(Mathf.Max(0f, minimumScale), safeInitialScale);
        float safeElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
        float safeSlowdownRate = Mathf.Max(0f, slowdownRate);

        float denominator = 1f + safeElapsedSeconds * safeSlowdownRate;
        float scale = denominator > 0f ? safeInitialScale / denominator : safeInitialScale;
        return Mathf.Max(safeMinimumScale, scale);
    }

	private void CheckBallState()
	{
		if (this.State.Current != GameStateMachine.State.Playing)
			return;

		if (FindObjectsByType<Ball>(FindObjectsSortMode.None).Length > 0)
			return;

		TriggerSpawn();
	}

    private void TriggerInitialSpawn()
    {
        Ball ball = CreateBallAtPaddle();
        if (ball == null)
            return;

        ball.LaunchImmediately();
    }

    private void TriggerSpawn(bool playSound = true)
    {
        Ball ball = CreateBallAtPaddle();
        if (ball == null)
            return;

        ball.Spawn();

        if (playSound && AudioManager.Instance != null)
            AudioManager.Instance.PlayRespawnSound();
    }

    private Ball CreateBallAtPaddle()
    {
        if (this.paddle == null)
        {
            Debug.LogError("Paddle not found!");
            return null;
        }

        float centerX = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f)).x;
        Vector3 spawnPos = new Vector3(centerX, this.paddle.transform.position.y + 1f, 0f);

        return Instantiate(this.ballPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator ShowGameOverPanelWhenBallsGone()
    {
        yield return new WaitUntil(() => FindObjectsByType<Ball>(FindObjectsSortMode.None).Length == 0);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void OnBossDefeated()
    {
        Debug.Log("게임 클리어");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameClearSound();

        if (gameClearPanel != null)
            gameClearPanel.SetActive(true);

        StopEnhancementTimeSlow();
        Time.timeScale = 0f;
    }
    public void GoToTitle()
    {
        ResetSessionState();
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopBgm();
        SceneManager.LoadScene("TitleScene");
    }

	private static void DestroyRuntimeObject(Object target)
	{
		if (target == null)
			return;

		if (Application.isPlaying)
			Destroy(target);
		else
			DestroyImmediate(target);
	}
}

public sealed class ScoreManager : MonoBehaviour
{
	public static ScoreManager Instance { get; private set; }

	private static int sessionScore;

	public int CurrentScore => sessionScore;
	public static int SessionScore => sessionScore;

	private void Awake()
	{
		Instance = this;
		ResetSessionScore();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	public static void ResetSessionScore()
	{
		sessionScore = 0;
	}

	public static void AddScoreToSession(int amount)
	{
		sessionScore += Mathf.Max(0, amount);
	}

	public static void AddDamageScoreToSession(int previousHp, int currentHp, int maxHp)
	{
		AddScoreToSession(CalculateDamageScoreDelta(previousHp, currentHp, maxHp));
	}

	public static int CalculateLostHealthScore(int currentHp, int maxHp)
	{
		if (maxHp <= 0)
			return 0;

		int clampedCurrent = Mathf.Clamp(currentHp, 0, maxHp);
		float lostRatio = (maxHp - clampedCurrent) / (float)maxHp;
		return Mathf.FloorToInt(lostRatio * 100f);
	}

	public static int CalculateDamageScoreDelta(int previousHp, int currentHp, int maxHp)
	{
		int previousScore = CalculateLostHealthScore(previousHp, maxHp);
		int currentScore = CalculateLostHealthScore(currentHp, maxHp);
		return Mathf.Max(0, currentScore - previousScore);
	}

	public static string FormatScoreText()
	{
		return $"점수: {sessionScore}";
	}
}
