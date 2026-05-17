using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHp = 1;
    [SerializeField] private int expReward = 5;

    private int currentHp;

    private void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"몬스터 피격, 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"몬스터 처치, 경험치 +{expReward}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExp(expReward);
        }

        Destroy(gameObject);
    }
}
