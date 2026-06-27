using UnityEngine;

[CreateAssetMenu(fileName = "EnhancementTriggerItem", menuName = "ItemData/EnhancementTrigger")]
public class EnhancementTriggerItemData : ItemData
{
    private static readonly int[] DefaultBaseUpgradeIds = { 1005, 3010, 2010 };
    private static readonly float[] DefaultBaseUpgradeWeights = { 1f, 1f, 1f };

    [SerializeField] private int[] upgradeIds = CreateDefaultUpgradeIds();
    [SerializeField] private float[] upgradeWeights = CreateDefaultUpgradeWeights();

    public override void Apply()
    {
        Apply(this.upgradeIds, this.upgradeWeights, CardUseContext.None);
    }

    public void Apply(CardUseContext context)
    {
        if (context.Source == CardOfferSource.Item && context.HasProgressionIndex)
        {
            Apply(
                CreateDefaultItemUpgradeIds(context.ProgressionIndex),
                CreateDefaultItemUpgradeWeights(context.ProgressionIndex),
                context);
            return;
        }

        Apply(this.upgradeIds, this.upgradeWeights, context);
    }

    public void Apply(int[] upgradeIds, float[] upgradeWeights)
    {
        Apply(upgradeIds, upgradeWeights, CardUseContext.None);
    }

    public void Apply(int[] upgradeIds, float[] upgradeWeights, CardUseContext context)
    {
        UserLevel userLevel = GameManager.Instance.User.Level;

        if (IsValidUpgradePool(upgradeIds, upgradeWeights))
            userLevel.OpenUpgradeUI(upgradeIds, upgradeWeights, context);
        else
            userLevel.OpenUpgradeUI(CreateDefaultUpgradeIds(), CreateDefaultUpgradeWeights(), context);
    }

    private static bool IsValidUpgradePool(int[] upgradeIds, float[] upgradeWeights)
    {
        if (upgradeIds == null ||
            upgradeWeights == null ||
            upgradeIds.Length == 0 ||
            upgradeIds.Length != upgradeWeights.Length)
            return false;

        for (int i = 0; i < upgradeIds.Length; ++i)
        {
            if (!UserLevel.IsKnownUpgradeCardId(upgradeIds[i]))
                return false;
        }

        return CardIds.HasCompleteScoreBonusWeightShare(upgradeIds, upgradeWeights);
    }

    private static int[] CreateDefaultUpgradeIds()
    {
        System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>(DefaultBaseUpgradeIds);
        System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>(DefaultBaseUpgradeWeights);
        CardIds.AddScoreBonusCards(ids, weights);
        return ids.ToArray();
    }

    private static float[] CreateDefaultUpgradeWeights()
    {
        System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>(DefaultBaseUpgradeIds);
        System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>(DefaultBaseUpgradeWeights);
        CardIds.AddScoreBonusCards(ids, weights);
        return weights.ToArray();
    }

    private static int[] CreateDefaultItemUpgradeIds(int mapIndex)
    {
        System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>();

        ids.Add(CardIds.Attack5);
        ids.Add(GetCriticalDamageCardIdForIndex(mapIndex));
        ids.Add(CardIds.GetCriticalChanceIdForIndex(mapIndex));
        AddItemSkillCardsForMap(mapIndex, ids);

        System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>();
        weights.Add(GetStatWeightForIndex(mapIndex));
        weights.Add(GetCriticalDamageWeightForIndex(mapIndex));
        weights.Add(GetCriticalChanceWeightForIndex(mapIndex));
        AddItemSkillWeightsForMap(mapIndex, weights);
        CardIds.AddScoreBonusCards(ids, weights);

        return ids.ToArray();
    }

    private static float[] CreateDefaultItemUpgradeWeights(int mapIndex)
    {
        System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>();

        weights.Add(GetStatWeightForIndex(mapIndex));
        weights.Add(GetCriticalDamageWeightForIndex(mapIndex));
        weights.Add(GetCriticalChanceWeightForIndex(mapIndex));
        AddItemSkillWeightsForMap(mapIndex, weights);
        System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>();
        ids.Add(CardIds.Attack5);
        ids.Add(GetCriticalDamageCardIdForIndex(mapIndex));
        ids.Add(CardIds.GetCriticalChanceIdForIndex(mapIndex));
        AddItemSkillCardsForMap(mapIndex, ids);
        CardIds.AddScoreBonusCards(ids, weights);

		return weights.ToArray();
    }

    private static int GetCriticalDamageCardIdForIndex(int index)
    {
        int[] criticalDamageByMap =
        {
            3010,
            3010, 3010, 3015, 3020, 3020, 3025, 3030, 3030, 3035, 3035,
            3040, 3040, 3045, 3045, 3050, 3055, 3055, 3060, 3065, 3070,
            3070, 3075, 3075, 3080, 3080, 3085, 3085, 3090, 3095, 3095,
            3100, 3100, 3105, 3110, 3110, 3115, 3115, 3120, 3120, 3125,
            3130, 3130, 3135, 3135, 3140, 3140, 3145, 3145, 3150, 3150
        };

        return criticalDamageByMap[Mathf.Clamp(index, 0, criticalDamageByMap.Length - 1)];
    }

    private static float GetCriticalDamageWeightForIndex(int index)
    {
        return GetStatWeightForIndex(index);
    }

    private static float GetCriticalChanceWeightForIndex(int index)
    {
        return GetStatWeightForIndex(index);
    }

    private static float GetStatWeightForIndex(int index)
    {
        float skillWeightTotal = GetItemSkillWeightTotalForMap(index);
        return skillWeightTotal > 0f ? skillWeightTotal : 1f;
    }

    private static void AddItemSkillCardsForMap(int mapIndex, System.Collections.Generic.List<int> ids)
    {
        if (mapIndex < 1)
            return;

        ids.Add(CardIds.CloneInstant);

        if (mapIndex >= 2)
            ids.Add(CardIds.GiantAutoCooldown);

        if (mapIndex >= 3)
        {
            ids.Add(CardIds.GiantSizeUp);
            ids.Add(CardIds.GiantDurationUp);
            ids.Add(CardIds.CloneCountUp);
            ids.Add(CardIds.GiantInstant);
        }

        if (mapIndex >= 5)
        {
            ids.Add(CardIds.CloneAutoCooldown);
            ids.Add(CardIds.GiantManualCooldown);
        }

        if (mapIndex >= 10)
            ids.Add(CardIds.CloneManualCooldown);
    }

    private static void AddItemSkillWeightsForMap(int mapIndex, System.Collections.Generic.List<float> weights)
    {
        if (mapIndex < 1)
            return;

        weights.Add(GetItemSkillCardWeight(CardIds.CloneInstant));

        if (mapIndex >= 2)
            weights.Add(GetItemSkillCardWeight(CardIds.GiantAutoCooldown));

        if (mapIndex >= 3)
        {
            weights.Add(GetItemSkillCardWeight(CardIds.GiantSizeUp));
            weights.Add(GetItemSkillCardWeight(CardIds.GiantDurationUp));
            weights.Add(GetItemSkillCardWeight(CardIds.CloneCountUp));
            weights.Add(GetItemSkillCardWeight(CardIds.GiantInstant));
        }

        if (mapIndex >= 5)
        {
            weights.Add(GetItemSkillCardWeight(CardIds.CloneAutoCooldown));
            weights.Add(GetItemSkillCardWeight(CardIds.GiantManualCooldown));
        }

        if (mapIndex >= 10)
            weights.Add(GetItemSkillCardWeight(CardIds.CloneManualCooldown));
    }

    private static float GetItemSkillWeightTotalForMap(int mapIndex)
    {
        if (mapIndex < 1)
            return 0f;

        float total = GetItemSkillCardWeight(CardIds.CloneInstant);

        if (mapIndex >= 2)
            total += GetItemSkillCardWeight(CardIds.GiantAutoCooldown);

        if (mapIndex >= 3)
        {
            total += GetItemSkillCardWeight(CardIds.GiantSizeUp);
            total += GetItemSkillCardWeight(CardIds.GiantDurationUp);
            total += GetItemSkillCardWeight(CardIds.CloneCountUp);
            total += GetItemSkillCardWeight(CardIds.GiantInstant);
        }

        if (mapIndex >= 5)
        {
            total += GetItemSkillCardWeight(CardIds.CloneAutoCooldown);
            total += GetItemSkillCardWeight(CardIds.GiantManualCooldown);
        }

        if (mapIndex >= 10)
            total += GetItemSkillCardWeight(CardIds.CloneManualCooldown);

        return total;
    }

    private static float GetItemSkillCardWeight(int cardId)
    {
        return 1f;
    }
}
