using UnityEngine;

[CreateAssetMenu(fileName = "EnhancementTriggerItem", menuName = "ItemData/EnhancementTrigger")]
public class EnhancementTriggerItemData : ItemData
{
    [SerializeField] private int[] upgradeIds = { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private float[] upgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

    public override void Apply()
    {
        Apply(this.upgradeIds, this.upgradeWeights);
    }

    public void Apply(int[] upgradeIds, float[] upgradeWeights)
    {
        UserLevel userLevel = GameManager.Instance.User.Level;

        if (IsValidUpgradePool(upgradeIds, upgradeWeights))
            userLevel.OpenUpgradeUI(upgradeIds, upgradeWeights);
        else
            userLevel.OpenUpgradeUI(new int[] { 1, 2, 3, 4, 5, 6 },
                                    new float[] { 30f, 25f, 20f, 15f, 10f, 20f });
    }

    private static bool IsValidUpgradePool(int[] upgradeIds, float[] upgradeWeights)
    {
        return upgradeIds != null &&
               upgradeWeights != null &&
               upgradeIds.Length > 0 &&
               upgradeIds.Length == upgradeWeights.Length;
    }
}
