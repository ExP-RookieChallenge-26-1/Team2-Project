using UnityEngine;

[CreateAssetMenu(fileName = "EnhancementTriggerItem", menuName = "ItemData/EnhancementTrigger")]
public class EnhancementTriggerItemData : ItemData
{
    [SerializeField] private int[] upgradeIds = { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private float[] upgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

    public override void Apply()
    {
        if (this.upgradeIds != null && this.upgradeWeights != null &&
            this.upgradeIds.Length > 0 && this.upgradeIds.Length == this.upgradeWeights.Length)
        {
            GameManager.Instance.OpenUpgradeUI(this.upgradeIds, this.upgradeWeights);
            return;
        }

        GameManager.Instance.OpenDefaultUpgradeUI();
    }
}