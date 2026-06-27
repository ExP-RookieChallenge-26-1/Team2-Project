using UnityEngine;

public class CowKingBreath : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float fallbackLifetime = 6f;

    private float damageTimer;
    private bool wasOverlappingPaddle;
    private Collider2D breathCollider;
    private Transform followTarget;
    private Paddle cachedPaddle;
    private User cachedUser;
    private UserHealth cachedUserHealth;

    private void Awake()
    {
        breathCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        Destroy(gameObject, fallbackLifetime);
    }

    private void Update()
    {
        AlignToSpawnPoint();

        bool isOverlappingPaddle = IsPaddleOverlappingBreath();
        if (!isOverlappingPaddle)
        {
            wasOverlappingPaddle = false;
            damageTimer = 0f;
            return;
        }

        if (!wasOverlappingPaddle)
        {
            wasOverlappingPaddle = true;
            damageTimer = 0f;
            ApplyDamage();
            return;
        }

        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval)
            return;

        damageTimer = 0f;
        ApplyDamage();
    }

    private void LateUpdate()
    {
        AlignToSpawnPoint();
    }

    public void AttachToSpawnPoint(Transform spawnPoint)
    {
        followTarget = spawnPoint;
        AlignToSpawnPoint();
    }

    private void AlignToSpawnPoint()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position;
    }

    private bool IsPaddleOverlappingBreath()
    {
        Paddle paddle = GetPaddle();
        if (paddle == null)
            return false;

        if (breathCollider == null)
            breathCollider = GetComponent<Collider2D>();

        Collider2D paddleCollider = paddle.GetComponent<Collider2D>();
        if (breathCollider == null || paddleCollider == null)
            return false;
        if (!breathCollider.enabled || !paddleCollider.enabled)
            return false;

        Physics2D.SyncTransforms();
        return breathCollider.Distance(paddleCollider).isOverlapped ||
            breathCollider.bounds.Intersects(paddleCollider.bounds) ||
            IsPaddleBoundsOverlappingBreath(paddleCollider) ||
            IsPaddleTransformInsideBreathColumn(paddle);
    }

    private bool IsPaddleBoundsOverlappingBreath(Collider2D paddleCollider)
    {
        Bounds bounds = paddleCollider.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;
        Vector2 center = bounds.center;

        return IsWorldPointInsideBreathPolygon(center) ||
            IsWorldPointInsideBreathPolygon(new Vector2(min.x, min.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(min.x, max.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(max.x, min.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(max.x, max.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(center.x, min.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(center.x, max.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(min.x, center.y)) ||
            IsWorldPointInsideBreathPolygon(new Vector2(max.x, center.y));
    }

    private bool IsPaddleTransformInsideBreathColumn(Paddle paddle)
    {
        Vector2 localPosition = transform.InverseTransformPoint(paddle.transform.position);
        return Mathf.Abs(localPosition.x) <= 1.35f &&
            localPosition.y >= -11.6f &&
            localPosition.y <= 11.2f;
    }

    private bool IsWorldPointInsideBreathPolygon(Vector2 worldPoint)
    {
        if (breathCollider is not PolygonCollider2D polygonCollider)
            return false;

        Vector2 localPoint = transform.InverseTransformPoint(worldPoint);
        for (int pathIndex = 0; pathIndex < polygonCollider.pathCount; pathIndex++)
        {
            if (ContainsPointInPolygon(localPoint, polygonCollider.GetPath(pathIndex)))
                return true;
        }

        return false;
    }

    private static bool ContainsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;

        for (int current = 0, previous = polygon.Length - 1; current < polygon.Length; previous = current++)
        {
            Vector2 currentPoint = polygon[current];
            Vector2 previousPoint = polygon[previous];

            bool crossesY = currentPoint.y > point.y != previousPoint.y > point.y;
            if (!crossesY)
                continue;

            float intersectionX = (previousPoint.x - currentPoint.x) *
                (point.y - currentPoint.y) /
                (previousPoint.y - currentPoint.y) +
                currentPoint.x;

            if (point.x < intersectionX)
                inside = !inside;
        }

        return inside;
    }

    private Paddle GetPaddle()
    {
        if (GameManager.Instance != null && GameManager.Instance.Paddle != null)
        {
            cachedPaddle = GameManager.Instance.Paddle;
            return cachedPaddle;
        }

        if (cachedPaddle == null)
            cachedPaddle = FindFirstObjectByType<Paddle>();

        return cachedPaddle;
    }

    private User GetUser()
    {
        if (GameManager.Instance != null && GameManager.Instance.User != null)
        {
            cachedUser = GameManager.Instance.User;
            return cachedUser;
        }

        if (cachedUser == null)
            cachedUser = FindFirstObjectByType<User>();

        return cachedUser;
    }

    private UserHealth GetUserHealth()
    {
        User user = GetUser();
        if (user != null && user.Health != null)
        {
            cachedUserHealth = user.Health;
            return cachedUserHealth;
        }

        if (cachedUserHealth == null)
            cachedUserHealth = FindFirstObjectByType<UserHealth>();

        return cachedUserHealth;
    }

    private void ApplyDamage()
    {
        UserHealth userHealth = GetUserHealth();
        if (userHealth == null)
            return;

        userHealth.TakeDamage(damage);
        Debug.Log($"브레스 피격: {damage}");
    }
}
