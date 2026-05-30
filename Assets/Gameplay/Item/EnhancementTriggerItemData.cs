using UnityEngine;

[CreateAssetMenu(fileName = "EnhancementTriggerItem", menuName = "ItemData/EnhancementTrigger")]
public class EnhancementTriggerItemData : ItemData
{
    [SerializeField] private int[] upgradeIds = { 1, 2, 3, 4, 5, 6 };
    [SerializeField] private float[] upgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

    public override void Apply()
    {
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem == null)
            return;

        if (this.upgradeIds != null && this.upgradeWeights != null &&
            this.upgradeIds.Length > 0 && this.upgradeIds.Length == this.upgradeWeights.Length)
        {
            levelSystem.OpenUpgradeUI(this.upgradeIds, this.upgradeWeights);
            return;
        }

        // 기본 업그레이드 풀로 열기
        levelSystem.OpenUpgradeUI(new int[] { 1, 2, 3, 4, 5, 6 },
                                 new float[] { 30f, 25f, 20f, 15f, 10f, 20f });
    }
}