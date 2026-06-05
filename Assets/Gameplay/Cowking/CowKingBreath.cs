using UnityEngine;

public class CowKingBreath : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private int damage = 1;

    private float damageTimer = 0f;
    private bool isTouchingPaddle = false;

    private void Update()
    {
        if (!isTouchingPaddle)
            return;

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            Debug.Log($"브레스 피격: {damage}");
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