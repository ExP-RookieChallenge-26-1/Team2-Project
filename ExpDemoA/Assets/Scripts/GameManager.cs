using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Temporary Upgrade Stats")]
    public float attackPower = 10f;
    public float critChance = 5f;
    public float ballSize = 1f;
    public float moveSpeed = 5f;
    public int ballCount = 1;

    [Header("Upgrade UI")]
    [SerializeField] private EnhanceUI enhanceUI;

    [Header("Time Control")]
    [SerializeField] private float slowTimeScale = 0.2f;

    public void OpenUpgradeUI()
    {
        int[] ids = { 1, 2, 3, 4, 5, 6 };
        float[] weights = { 30f, 25f, 20f, 15f, 10f, 20f };

        Time.timeScale = slowTimeScale;
        enhanceUI.gameObject.SetActive(true);
        enhanceUI.ShowCardsByIds(ids, weights);
    }

    public void CloseUpgradeUI()
    {
        Time.timeScale = 1f;
        enhanceUI.gameObject.SetActive(false);
    }

    public void ApplyUpgrade(CardData card)
    {
        if (card == null) return;

        switch (card.upgradeType)
        {
            case UpgradeType.AttackPower:
                attackPower += card.value;
                Debug.Log($"[Upgrade] {card.cardName} 선택 -> 공격력: {attackPower}");
                break;

            case UpgradeType.CritChance:
                critChance += card.value;
                Debug.Log($"[Upgrade] {card.cardName} 선택 -> 치명타: {critChance}");
                break;

            case UpgradeType.BallSize:
                ballSize += card.value;
                Debug.Log($"[Upgrade] {card.cardName} 선택 -> 공 크기: {ballSize}");
                break;

            case UpgradeType.Speed:
                moveSpeed += card.value;
                Debug.Log($"[Upgrade] {card.cardName} 선택 -> 속도: {moveSpeed}");
                break;

            case UpgradeType.BallCount:
                ballCount += Mathf.RoundToInt(card.value);
                Debug.Log($"[Upgrade] {card.cardName} 선택 -> 공 개수: {ballCount}");
                break;
        }

        CloseUpgradeUI();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}