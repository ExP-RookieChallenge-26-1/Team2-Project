using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum PickupTargetType
    {
        Ball,
        Paddle,
        Both
    }

    [SerializeField] private ItemData itemData;
    [SerializeField] private PickupTargetType pickupTargetType = PickupTargetType.Ball;

    private Ball cachedBall;
    private Paddle cachedPaddle;
    private Collider2D cachedCollider;
    private Renderer cachedRenderer;
    private Collider2D paddleCollider;
    private bool isConsumed;

    private void Awake()
    {
        this.cachedCollider = GetComponent<Collider2D>();
        this.cachedRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        this.cachedBall = Object.FindFirstObjectByType<Ball>();
        this.cachedPaddle = Object.FindFirstObjectByType<Paddle>();

        if (this.cachedPaddle != null)
            this.paddleCollider = this.cachedPaddle.GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (this.isConsumed || this.itemData == null)
            return;

        if (GameManager.Instance == null || GameManager.Instance.State.Current != GameStateMachine.State.Playing)
            return;

        Bounds bounds = GetPickupBounds();
        if (bounds.size == Vector3.zero)
            return;

        if (CanBePickedByBall(bounds) || CanBePickedByPaddle(bounds))
            Collect();
    }

    private Bounds GetPickupBounds()
    {
        if (this.cachedCollider != null)
            return this.cachedCollider.bounds;
        if (this.cachedRenderer != null)
            return this.cachedRenderer.bounds;
        return default;
    }

    private bool CanBePickedByBall(Bounds itemBounds)
    {
        if (this.pickupTargetType != PickupTargetType.Ball &&
            this.pickupTargetType != PickupTargetType.Both)
            return false;

        if (this.cachedBall == null)
            return false;

        return BallCollision.IsTouching(itemBounds, this.cachedBall.transform.position, this.cachedBall.Stats.Radius);
    }

    private bool CanBePickedByPaddle(Bounds itemBounds)
    {
        if (this.pickupTargetType != PickupTargetType.Paddle &&
            this.pickupTargetType != PickupTargetType.Both)
            return false;

        if (this.paddleCollider == null)
            return false;

        return itemBounds.Intersects(this.paddleCollider.bounds);
    }

    private void Collect()
    {
        this.isConsumed = true;
        Debug.Log($"아이템 획득: {this.itemData.name}");
        this.itemData.Apply();
        Destroy(gameObject);
    }
}