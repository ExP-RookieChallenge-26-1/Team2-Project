using UnityEngine;

public class FallingItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private ItemCardPool[] itemCardPools;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minX = -2.5f;
    [SerializeField] private float maxX = 2.5f;
    [SerializeField] private float spawnY = 6f;

    private float timer;

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State.Current != GameStateMachine.State.Playing)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnItem();
        }
    }

    private void SpawnItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        int itemIndex = Random.Range(0, itemPrefabs.Length);
        GameObject prefab = itemPrefabs[itemIndex];
        if (prefab == null)
            return;

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        GameObject item = Instantiate(prefab, spawnPos, Quaternion.identity);
        ApplyCardPool(item, itemIndex);
    }

    private void ApplyCardPool(GameObject item, int itemIndex)
    {
        ItemPickup itemPickup = item.GetComponent<ItemPickup>();
        ItemCardPool cardPool = GetItemCardPool(itemIndex);

        if (itemPickup == null || cardPool == null)
            return;

        itemPickup.SetCardPool(cardPool.CardIds, cardPool.CardWeights);
    }

    private ItemCardPool GetItemCardPool(int index)
    {
        if (this.itemCardPools == null || index < 0 || index >= this.itemCardPools.Length)
            return null;

        return this.itemCardPools[index];
    }
}
