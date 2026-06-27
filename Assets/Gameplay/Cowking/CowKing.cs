using UnityEngine;

public class CowKing : MonoBehaviour, IDamageable
{
    public enum CowKingState
    {
        Idle,
        Move,
        Attack,
        Damaged,
        Die
    }

    [Header("Stats")]
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int attackPower = 1;

    [Header("State Rates")]
    [Range(0f, 1f)]
    [SerializeField] private float stateRateMove = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float stateRateAttack = 0.5f;

    [Header("Timing")]
    [SerializeField] private float decisionInterval = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackReadyTime = 2f;
    [SerializeField] private float breathDurationMin = 2f;
    [SerializeField] private float breathDurationMax = 5f;

    [Header("Move Range")]
    [SerializeField] private float minX = -2.5f;
    [SerializeField] private float maxX = 2.5f;
    [SerializeField] private float minY = 2.5f;
    [SerializeField] private float maxY = 4.5f;

    [Header("Breath")]
    [SerializeField] private GameObject breathPrefab;
    [SerializeField] private Transform breathSpawnPoint;

    private int currentHp;
    private CowKingState currentState;
    private float stateTimer;
    private float currentBreathDuration;
    private float breathTimer;
    private Vector3 targetPosition;
    private GameObject currentBreath;
    private bool isDead;
    private bool isActivated;
    private bool isBreathing;
    private bool canTakeDamage;
    private Animator animator;
    private MonsterHealthBar healthBar;
    private MobDamageOverlay damageOverlay;

    public int AttackPower => attackPower;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHp = maxHp;

        healthBar = GetComponent<MonsterHealthBar>();
        if (healthBar == null)
            healthBar = gameObject.AddComponent<MonsterHealthBar>();

        damageOverlay = GetComponent<MobDamageOverlay>();
        if (damageOverlay == null)
            damageOverlay = gameObject.AddComponent<MobDamageOverlay>();

        healthBar.Configure(new Vector2(2.6f, 0.45f), 0.05f, MonsterHealthBar.VerticalAnchor.Bottom);
        healthBar.Hide();
    }

    private void Update()
    {
        if (!isActivated || isDead)
            return;

        switch (currentState)
        {
            case CowKingState.Idle:
                UpdateIdle();
                break;
            case CowKingState.Move:
                UpdateMove();
                break;
            case CowKingState.Attack:
                UpdateAttack();
                break;
            case CowKingState.Damaged:
            case CowKingState.Die:
                break;
        }
    }

    public void ActivateBoss()
    {
        if (isActivated || isDead)
            return;

        isActivated = true;
        canTakeDamage = true;
        healthBar.SetHealth(currentHp, maxHp);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossBgm();

        ChangeState(CowKingState.Idle);
    }

    private void UpdateIdle()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= decisionInterval)
            DecideNextState();
    }

    private void UpdateMove()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) >= 0.05f)
            return;

        SetMoveAnimation(false);
        ChangeState(CowKingState.Idle);
    }

    private void UpdateAttack()
    {
        if (!isBreathing)
        {
            stateTimer += Time.deltaTime;

            if (stateTimer >= attackReadyTime)
                SpawnBreath();

            return;
        }

        breathTimer += Time.deltaTime;

        if (breathTimer < currentBreathDuration)
            return;

        EndBreath();
        ChangeState(CowKingState.Idle);
    }

    private void DecideNextState()
    {
        float moveWeight = Mathf.Max(0f, stateRateMove);
        float attackWeight = Mathf.Max(0f, stateRateAttack);
        float totalWeight = moveWeight + attackWeight;

        if (totalWeight <= 0f || Random.value < moveWeight / totalWeight)
            StartMove();
        else
            StartAttack();
    }

    private void StartMove()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossVoiceSound();

        targetPosition = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            transform.position.z);

        SetMoveAnimation(true);
        ChangeState(CowKingState.Move);
    }

    private void StartAttack()
    {
        currentBreathDuration = Random.Range(breathDurationMin, breathDurationMax);
        isBreathing = false;
        breathTimer = 0f;

        SetMoveAnimation(false);
        SetBreathAnimation(false);
        PlayAttackReadyAnimation();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossAttackReadySound();

        ChangeState(CowKingState.Attack);
    }

    private void SpawnBreath()
    {
        if (currentState != CowKingState.Attack)
            return;

        if (breathPrefab == null || breathSpawnPoint == null)
        {
            ChangeState(CowKingState.Idle);
            return;
        }

        if (currentBreath != null)
            EndBreath();

        SetBreathAnimation(true);
        PlayAttackAnimation();

        currentBreath = Instantiate(breathPrefab, breathSpawnPoint.position, Quaternion.identity);
        CowKingBreath breath = currentBreath.GetComponent<CowKingBreath>();
        if (breath != null)
            breath.AttachToSpawnPoint(breathSpawnPoint);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossAttackLoopSound();

        isBreathing = true;
        breathTimer = 0f;
    }

    private void EndBreath()
    {
        if (currentBreath != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBossAttackOutloopSound();

            CowKingBreathAnimation breathAnimation = currentBreath.GetComponent<CowKingBreathAnimation>();
            if (breathAnimation != null)
                breathAnimation.PlayEndAndDestroy();
            else
                Destroy(currentBreath);

            currentBreath = null;
        }

        isBreathing = false;
        SetBreathAnimation(false);
        breathTimer = 0f;
    }

    private void ChangeState(CowKingState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    private System.Collections.IEnumerator DamagedRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (!isDead && currentState == CowKingState.Damaged)
            ChangeState(CowKingState.Idle);
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead || !canTakeDamage)
            return;

        damage = Mathf.Max(1, damage);
        int previousHp = currentHp;
        currentHp = Mathf.Max(0, currentHp - damage);
        ScoreManager.AddDamageScoreToSession(previousHp, currentHp, maxHp);
        healthBar.SetHealth(currentHp, maxHp);

        Debug.Log($"우마왕 피격, 남은 체력: {currentHp}");
        Color color = isCrit ? Color.yellow : Color.white;
        DamageTextSpawner.Instance?.Spawn(transform.position, damage, color);
        damageOverlay?.Play();

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        if (currentState != CowKingState.Attack)
        {
            PlayDamagedAnimation();
            ChangeState(CowKingState.Damaged);
            StartCoroutine(DamagedRoutine());
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        canTakeDamage = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossDieSound();
            AudioManager.Instance.StopBgm();
        }

        EndBreath();
        ChangeState(CowKingState.Die);

        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null)
            bossCollider.enabled = false;

        PlayDieAnimation();
        StartCoroutine(DieSequence());
    }

    private System.Collections.IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(1f);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossFallSound();

        const float fallSpeed = 2.5f;
        const float rotateSpeed = 360f;
        const float bottomY = -7f;

        while (transform.position.y > bottomY)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
            yield return null;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnBossDefeated();

        Destroy(gameObject);
    }

    public void PlayBossEntrySfx()
    {
        AudioManager.Instance?.PlayBossEntrySound();
    }

    private void SetMoveAnimation(bool isMoving)
    {
        if (animator != null)
            animator.SetBool("IsMoving", isMoving);
    }

    private void SetBreathAnimation(bool breathing)
    {
        if (animator != null)
            animator.SetBool("IsBreathing", breathing);
    }

    private void PlayAttackAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("AttackReady");
        animator.SetTrigger("Attack");
        animator.Update(0f);
    }

    private void PlayAttackReadyAnimation()
    {
        if (animator != null)
            animator.SetTrigger("AttackReady");
    }

    private void PlayDamagedAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Damaged");
    }

    private void PlayDieAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    private void OnDestroy()
    {
        EndBreath();
    }
}
