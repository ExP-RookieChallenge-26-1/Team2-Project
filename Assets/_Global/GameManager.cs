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

    [SerializeField] private BallStats ballStatsAsset;
    [SerializeField] private PaddleStats paddleStatsAsset;
    [SerializeField] private WorldStats worldStatsAsset;
    [SerializeField] private EnhanceUI enhanceUI;

    [Header("Level System")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int[] requiredExpByLevel = { 10, 20, 35, 50, 70, 100 };

    [Header("Default Upgrade Pool")]
    [SerializeField] private int[] defaultUpgradeIds = { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private float[] defaultUpgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

    public int CurrentLevel => this.currentLevel;
    public int CurrentExp => this.currentExp;

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

        if (this.enhanceUI != null)
            this.enhanceUI.gameObject.SetActive(false);
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

    public void OpenDefaultUpgradeUI()
    {
        OpenUpgradeUI(this.defaultUpgradeIds, this.defaultUpgradeWeights);
    }

    public void OpenUpgradeUI(int[] ids, float[] weights)
    {
        if (this.enhanceUI == null)
        {
            Debug.LogWarning("EnhanceUI is not assigned.");
            return;
        }

        if (ids == null || weights == null || ids.Length == 0 || ids.Length != weights.Length)
        {
            Debug.LogWarning("Upgrade UI open skipped because the card pool is invalid.");
            return;
        }

        this.State.Change(GameStateMachine.State.Enhancement);
        this.enhanceUI.gameObject.SetActive(true);
        this.enhanceUI.ShowCardsByIds(ids, weights);
    }

    public void AddExp(int amount)
    {
        this.currentExp += amount;
        Debug.Log($"경험치 획득: +{amount}, 현재 EXP: {this.currentExp}");

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (this.currentLevel - 1 < this.requiredExpByLevel.Length &&
               this.currentExp >= this.requiredExpByLevel[this.currentLevel - 1])
        {
            this.currentExp -= this.requiredExpByLevel[this.currentLevel - 1];
            this.currentLevel++;

            Debug.Log($"레벨업! 현재 레벨: {this.currentLevel}");
            OpenDefaultUpgradeUI();
        }
    }

    public void OnBallDestroyed()
    {
        this.State.Change(GameStateMachine.State.GameOver);
    }
}