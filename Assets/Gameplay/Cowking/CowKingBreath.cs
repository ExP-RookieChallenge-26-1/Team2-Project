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
            Debug.Log($"브레스 피격: {damage}");

            //TODO: 플레이어 체력 시스템 연결
            // GameManager.Instance.TakePlayerDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Paddle"))
            return;

        isTouchingPaddle = true;
        damageTimer = 0f;

        Debug.Log($"브레스 첫 피격: {damage}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Paddle"))
            return;

        isTouchingPaddle = false;
        damageTimer = 0f;
    }
}