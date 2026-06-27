using UnityEngine;

public class FallingItemSpawner : MonoBehaviour
{
    private const int DefaultMapDropSettingCount = 51;

    [System.Serializable]
    private class MapDropSettings
    {
        [SerializeField] private int mapIndex;
        [SerializeField] private float enhancementDropTime;
        [SerializeField] private float enhancementX;
        [SerializeField] private ItemCardPool enhancementCardPool;
        [SerializeField] private float attackDropTime;
        [SerializeField] private float attackX;

        public int MapIndex => this.mapIndex;
        public float EnhancementDropTime => this.enhancementDropTime;
        public float EnhancementX => this.enhancementX;
        public ItemCardPool EnhancementCardPool => this.enhancementCardPool;
        public float AttackDropTime => this.attackDropTime;
        public float AttackX => this.attackX;

        public MapDropSettings(int mapIndex, float enhancementDropTime, float enhancementX, ItemCardPool enhancementCardPool, float attackDropTime, float attackX)
        {
            this.mapIndex = mapIndex;
            this.enhancementDropTime = enhancementDropTime;
            this.enhancementX = enhancementX;
            this.enhancementCardPool = enhancementCardPool;
            this.attackDropTime = attackDropTime;
            this.attackX = attackX;
        }
    }

    private struct PendingDrop
    {
        public GameObject Prefab;
        public Transform ChunkTransform;
        public int MapIndex;
        public float Delay;
        public float Elapsed;
        public float X;
        public ItemCardPool CardPool;
        public bool UseCardContext;
    }

    [SerializeField] private GameObject enhancementTicketPrefab;
    [SerializeField] private GameObject attackUpPrefab;
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private MapDropSettings[] mapDropSettings = CreateDefaultMapDropSettings();

    private readonly System.Collections.Generic.List<PendingDrop> pendingDrops = new System.Collections.Generic.List<PendingDrop>();

    private void Awake()
    {
        EnsureDefaultDropSettings();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureDefaultDropSettings();
    }
#endif

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State.Current != GameStateMachine.State.Playing)
            return;

        TickPendingDrops(Time.deltaTime);
    }

    public void ScheduleMapDrops(int mapIndex, Transform chunkTransform)
    {
        if (chunkTransform == null)
            return;

        EnsureDefaultDropSettings();

        MapDropSettings settings = GetMapDropSettings(mapIndex);
        if (settings == null)
            return;

        ScheduleDrop(
            this.enhancementTicketPrefab,
            chunkTransform,
            mapIndex,
            settings.EnhancementDropTime,
            settings.EnhancementX,
            settings.EnhancementCardPool,
            true);
    }

    private void TickPendingDrops(float deltaTime)
    {
        for (int i = this.pendingDrops.Count - 1; i >= 0; --i)
        {
            PendingDrop drop = this.pendingDrops[i];
            if (drop.ChunkTransform == null)
            {
                this.pendingDrops.RemoveAt(i);
                continue;
            }

            drop.Elapsed += deltaTime;
            if (drop.Elapsed < drop.Delay)
            {
                this.pendingDrops[i] = drop;
                continue;
            }

            SpawnItem(drop);
            this.pendingDrops.RemoveAt(i);
        }
    }

    private void ScheduleDrop(
        GameObject prefab,
        Transform chunkTransform,
        int mapIndex,
        float delay,
        float x,
        ItemCardPool cardPool,
        bool useCardContext)
    {
        if (prefab == null)
            return;

        this.pendingDrops.Add(new PendingDrop
        {
            Prefab = prefab,
            ChunkTransform = chunkTransform,
            MapIndex = mapIndex,
            Delay = Mathf.Max(0f, delay),
            X = x,
            CardPool = cardPool,
            UseCardContext = useCardContext
        });
    }

    private void SpawnItem(PendingDrop drop)
    {
        Vector3 spawnPos = new Vector3(drop.X, drop.ChunkTransform.position.y + this.spawnY, 0f);
        GameObject item = Instantiate(drop.Prefab, spawnPos, Quaternion.identity);

        if (drop.UseCardContext)
            ApplyCardPool(item, drop.CardPool, drop.MapIndex);
    }

    private void ApplyCardPool(GameObject item, ItemCardPool cardPool, int mapIndex)
    {
        ItemPickup itemPickup = item.GetComponent<ItemPickup>();

        if (itemPickup == null)
            return;

        CardUseContext context = new CardUseContext(CardOfferSource.Item, mapIndex);
        if (cardPool != null)
            itemPickup.SetCardPool(cardPool.CardIds, cardPool.CardWeights, context);
        else
            itemPickup.SetCardContext(context);
    }

    private MapDropSettings GetMapDropSettings(int mapIndex)
    {
        if (this.mapDropSettings == null || this.mapDropSettings.Length == 0)
            return null;

        for (int i = 0; i < this.mapDropSettings.Length; ++i)
        {
            if (this.mapDropSettings[i] != null && this.mapDropSettings[i].MapIndex == mapIndex)
                return this.mapDropSettings[i];
        }

        return this.mapDropSettings[Mathf.Clamp(mapIndex, 0, this.mapDropSettings.Length - 1)];
    }

    private void EnsureDefaultDropSettings()
    {
        if (AreValidMapDropSettings(this.mapDropSettings))
            return;

        this.mapDropSettings = CreateDefaultMapDropSettings();
    }

    private static bool AreValidMapDropSettings(MapDropSettings[] settings)
    {
        if (settings == null || settings.Length != DefaultMapDropSettingCount)
            return false;

        for (int i = 0; i < settings.Length; ++i)
        {
            if (settings[i] == null || settings[i].MapIndex != i)
                return false;

            if (!IsValidItemCardPool(settings[i].EnhancementCardPool))
                return false;

            int compressedItemIndex = GetCompressedItemIndex(i);
            if (!AreItemCardPoolsEqual(settings[i].EnhancementCardPool, CreateItemCardPool(compressedItemIndex)))
                return false;
        }

        return true;
    }

    private static bool AreItemCardPoolsEqual(ItemCardPool left, ItemCardPool right)
    {
        if (left == null || right == null ||
            left.CardIds == null || right.CardIds == null ||
            left.CardWeights == null || right.CardWeights == null ||
            left.CardIds.Length != right.CardIds.Length ||
            left.CardWeights.Length != right.CardWeights.Length)
            return false;

        for (int i = 0; i < left.CardIds.Length; ++i)
        {
            if (left.CardIds[i] != right.CardIds[i])
                return false;
        }

        for (int i = 0; i < left.CardWeights.Length; ++i)
        {
            if (Mathf.Abs(left.CardWeights[i] - right.CardWeights[i]) > 0.0001f)
                return false;
        }

        return true;
    }

    private static bool IsValidItemCardPool(ItemCardPool cardPool)
    {
        if (cardPool == null ||
            cardPool.CardIds == null ||
            cardPool.CardWeights == null ||
            cardPool.CardIds.Length == 0 ||
            cardPool.CardIds.Length != cardPool.CardWeights.Length)
            return false;

        for (int i = 0; i < cardPool.CardIds.Length; ++i)
        {
            if (!UserLevel.IsKnownUpgradeCardId(cardPool.CardIds[i]))
                return false;
        }

        return CardIds.HasCompleteScoreBonusWeightShare(cardPool.CardIds, cardPool.CardWeights);
    }

    private static MapDropSettings[] CreateDefaultMapDropSettings()
    {
        MapDropSettings[] settings = new MapDropSettings[DefaultMapDropSettingCount];
        for (int mapIndex = 0; mapIndex < settings.Length; ++mapIndex)
        {
            int compressedItemIndex = GetCompressedItemIndex(mapIndex);
            settings[mapIndex] = new MapDropSettings(
                mapIndex,
                GetEnhancementDropTime(compressedItemIndex),
                GetEnhancementDropX(compressedItemIndex),
                CreateItemCardPool(compressedItemIndex),
                GetAttackDropTime(compressedItemIndex),
                GetAttackDropX(compressedItemIndex));
        }

        return settings;
    }

    private static int GetCompressedItemIndex(int mapIndex)
    {
        if (mapIndex <= 0)
            return 0;

        if (mapIndex == 3)
            return 3;

        if (mapIndex == 1)
            return 2;

        if (mapIndex == 2)
            return 5;

        if (mapIndex >= 4 && mapIndex <= 25)
            return mapIndex * 2 - 1;

        return 50;
    }

    private static float GetEnhancementDropTime(int mapIndex)
    {
        return 5f + mapIndex % 4;
    }

    private static float GetAttackDropTime(int mapIndex)
    {
        return 11f + mapIndex % 5;
    }

    private static float GetEnhancementDropX(int mapIndex)
    {
        return -2.25f + mapIndex % 4 * 1.5f;
    }

    private static float GetAttackDropX(int mapIndex)
    {
        return 2.25f - mapIndex % 4 * 1.5f;
    }

    private static ItemCardPool CreateItemCardPool(int mapIndex)
    {
        System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>();
        System.Collections.Generic.List<float> weights = new System.Collections.Generic.List<float>();

        ids.Add(CardIds.Attack5);
        weights.Add(GetStatWeightForIndex(mapIndex));

        ids.Add(GetCriticalDamageCardIdForIndex(mapIndex));
        weights.Add(GetCriticalDamageWeightForIndex(mapIndex));

        ids.Add(CardIds.GetCriticalChanceIdForIndex(mapIndex));
        weights.Add(GetCriticalChanceWeightForIndex(mapIndex));

        AddItemSkillCardsForMap(mapIndex, ids, weights);
        CardIds.AddScoreBonusCards(ids, weights);

        return new ItemCardPool(ids.ToArray(), weights.ToArray());
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

    private static void AddItemSkillCardsForMap(
        int mapIndex,
        System.Collections.Generic.List<int> ids,
        System.Collections.Generic.List<float> weights)
    {
        if (mapIndex < 1)
            return;

        ids.Add(CardIds.CloneInstant);
        weights.Add(GetItemSkillCardWeight(CardIds.CloneInstant));

        if (mapIndex >= 2)
        {
            ids.Add(CardIds.GiantAutoCooldown);
            weights.Add(GetItemSkillCardWeight(CardIds.GiantAutoCooldown));
        }

        if (mapIndex >= 3)
        {
            ids.Add(CardIds.GiantSizeUp);
            weights.Add(GetItemSkillCardWeight(CardIds.GiantSizeUp));
            ids.Add(CardIds.GiantDurationUp);
            weights.Add(GetItemSkillCardWeight(CardIds.GiantDurationUp));
            ids.Add(CardIds.CloneCountUp);
            weights.Add(GetItemSkillCardWeight(CardIds.CloneCountUp));
            ids.Add(CardIds.GiantInstant);
            weights.Add(GetItemSkillCardWeight(CardIds.GiantInstant));
        }

        if (mapIndex >= 5)
        {
            ids.Add(CardIds.CloneAutoCooldown);
            weights.Add(GetItemSkillCardWeight(CardIds.CloneAutoCooldown));
            ids.Add(CardIds.GiantManualCooldown);
            weights.Add(GetItemSkillCardWeight(CardIds.GiantManualCooldown));
        }

        if (mapIndex >= 10)
        {
            ids.Add(CardIds.CloneManualCooldown);
            weights.Add(GetItemSkillCardWeight(CardIds.CloneManualCooldown));
        }
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
