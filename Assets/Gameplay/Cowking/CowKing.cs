using UnityEngine;

public class CowKing : MonoBehaviour
{
    public enum CowKingState
    {
        Idle,
        Move,
        Attack
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
    private Vector3 targetPosition;
    private GameObject currentBreath;

    private bool isActivated = false;
    private void Start()
    {
        currentHp = maxHp;
  
    }

    private void Update()
    {
        if (!isActivated)
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
        }
    }

    public void ActivateBoss()
    {
        if (isActivated)
            return;

        isActivated = true;
        ChangeState(CowKingState.Idle);
    }

    private void UpdateIdle()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= decisionInterval)
        {
            DecideNextState();
        }
    }

    private void UpdateMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            ChangeState(CowKingState.Idle);
        }
    }

    private void UpdateAttack()
    {
        stateTimer += Time.deltaTime;

        if (currentBreath == null)
        {
            if (stateTimer >= attackReadyTime)
            {
                SpawnBreath();
                stateTimer = 0f;
            }
        }
        else
        {
            if (stateTimer >= GetCurrentBreathDuration())
            {
                EndBreath();
                ChangeState(CowKingState.Idle);
            }
        }
    }

    private float currentBreathDuration;

    private float GetCurrentBreathDuration()
    {
        return currentBreathDuration;
    }

    private void DecideNextState()
    {
        float rand = Random.value;

        if (rand < stateRateMove)
        {
            StartMove();
        }
        else
        {
            StartAttack();
        }
    }

    private void StartMove()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        targetPosition = new Vector3(randomX, randomY, transform.position.z);
        ChangeState(CowKingState.Move);
    }

    private void StartAttack()
    {
        currentBreathDuration = Random.Range(breathDurationMin, breathDurationMax);
        ChangeState(CowKingState.Attack);
    }

    private void SpawnBreath()
    {
        if (breathPrefab == null || breathSpawnPoint == null)
        {
            ChangeState(CowKingState.Idle);
            return;
        }

        currentBreath = Instantiate(breathPrefab, breathSpawnPoint.position, Quaternion.identity, transform);
    }
    private void EndBreath()
    {
        if (currentBreath != null)
        {
            Destroy(currentBreath);
            currentBreath = null;
        }
    }

    private void ChangeState(CowKingState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EndBreath();
        Destroy(gameObject);
    }
}