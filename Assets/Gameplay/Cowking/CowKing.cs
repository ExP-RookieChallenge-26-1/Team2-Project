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
    private float breathTimer = 0f;

    private Vector3 targetPosition;
    private GameObject currentBreath;

    private bool isDead = false;
    private bool isActivated = false;
    private bool isBreathing = false;
    private bool canTakeDamage = false;
    private Animator animator;
    private void Start()
    {
        currentHp = maxHp;
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
                break;

            case CowKingState.Die:
                break;
        }
    }

    public void ActivateBoss()
    {
        if (isActivated)
            return;

        isActivated = true;
        canTakeDamage = true;
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
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            SetMoveAnimation(false);
            ChangeState(CowKingState.Idle);
        }
    }

    private void UpdateAttack()
    {
        if (!isBreathing)
        {
            stateTimer += Time.deltaTime;

            if (stateTimer >= attackReadyTime)
                SpawnBreath();
        }
        else
        {
            breathTimer += Time.deltaTime;

            if (breathTimer >= currentBreathDuration)
            {
                EndBreath();
                ChangeState(CowKingState.Idle);
            }
        }
    }

    private void DecideNextState()
    {
        float rand = Random.value;

        if (rand < stateRateMove)
            StartMove();
        else
            StartAttack();
    }

    private void StartMove()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        targetPosition = new Vector3(randomX, randomY, transform.position.z);
        SetMoveAnimation(true);
        ChangeState(CowKingState.Move);
    }

    private void StartAttack()
    {
        currentBreathDuration = Random.Range(breathDurationMin, breathDurationMax);

        SetMoveAnimation(false);
        PlayAttackReadyAnimation();

        isBreathing = false;
        breathTimer = 0f;
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

        currentBreath = Instantiate(breathPrefab, breathSpawnPoint.position, Quaternion.identity);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossBreathSound();

        isBreathing = true;
        breathTimer = 0f;
    }

    private void EndBreath()
    {
        if (currentBreath != null)
        {
            Destroy(currentBreath);
            currentBreath = null;
        }

        isBreathing = false;
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
        currentHp -= damage;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossHitSound();

        Debug.Log($"우마왕 피격, 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        PlayDamagedAnimation();

        if (currentState != CowKingState.Attack)
        {
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
            AudioManager.Instance.PlayBossDieSound();

        EndBreath();
        ChangeState(CowKingState.Die);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(DieSequence());
    }

    private System.Collections.IEnumerator DieSequence()
    {
        Debug.Log("우마왕 die 시작");

        yield return new WaitForSeconds(1f);

        float fallSpeed = 2.5f;
        float rotateSpeed = 360f;
        float bottomY = -7f;

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


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void SetMoveAnimation(bool isMoving)
    {
        if (animator != null)
            animator.SetBool("IsMoving", isMoving);
    }

    private void PlayAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    private void PlayDamagedAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Damaged");
    }

    private void PlayAttackReadyAnimation()
    {
        if (animator != null)
            animator.SetTrigger("AttackReady");
    }
    private void OnDestroy()
    {
        EndBreath();
    }
}