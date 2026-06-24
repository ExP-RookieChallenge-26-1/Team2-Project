using UnityEngine;

public class CowKingBreath : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float fallbackLifetime = 6f;

    private float damageTimer = 0f;
    private bool isTouchingPaddle = false;

    private void Start()
    {
        Destroy(gameObject, fallbackLifetime);
    }

    private void Update()
    {
        if (!isTouchingPaddle)
            return;

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Paddle"))
            return;

        isTouchingPaddle = true;
        damageTimer = 0f;

        ApplyDamage();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Paddle"))
            return;

        isTouchingPaddle = false;
        damageTimer = 0f;
    }

    private void ApplyDamage()
    {
        if (GameManager.Instance == null || GameManager.Instance.User == null)
            return;

        GameManager.Instance.User.Health.TakeDamage(damage);

        Debug.Log($"브레스 피격: {damage}");
    }
}