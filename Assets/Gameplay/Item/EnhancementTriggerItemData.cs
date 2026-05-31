using UnityEngine;

[CreateAssetMenu(fileName = "EnhancementTriggerItem", menuName = "ItemData/EnhancementTrigger")]
public class EnhancementTriggerItemData : ItemData
{
    [SerializeField] private int[] upgradeIds = { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private float[] upgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

    public override void Apply()
    {
        UserLevel userLevel = GameManager.Instance.User.Level;

        if (this.upgradeIds != null && this.upgradeWeights != null &&
            this.upgradeIds.Length > 0 && this.upgradeIds.Length == this.upgradeWeights.Length)
        {
            userLevel.OpenUpgradeUI(this.upgradeIds, this.upgradeWeights);
            return;
        }

        userLevel.OpenUpgradeUI(new int[] { 1, 2, 3, 4, 5, 6 },
                                new float[] { 30f, 25f, 20f, 15f, 10f, 20f });
    }
}
