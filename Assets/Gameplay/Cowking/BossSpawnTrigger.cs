using System.Collections;
using UnityEngine;

public class BossSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private float activateDelay = 1.5f;

    private bool hasSpawned = false;

    private static bool hasSpawnedBossOnce = false;

    private void Update()
    {
        if (hasSpawned || hasSpawnedBossOnce)
            return;

        if (bossPrefab == null || bossSpawnPoint == null || Camera.main == null)
            return;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(bossSpawnPoint.position);

        bool isVisibleZone =
            viewportPos.z > 0f &&
            viewportPos.x >= 0f && viewportPos.x <= 1f &&
            viewportPos.y >= 0.55f && viewportPos.y <= 1f;

        if (isVisibleZone)
        {
            TrySpawnBoss();
        }
    }

    private void TrySpawnBoss()
    {
        if (hasSpawned || hasSpawnedBossOnce)
            return;

        if (bossPrefab == null || bossSpawnPoint == null || Camera.main == null)
            return;

        Vector3 spawnPos = bossSpawnPoint.position;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(spawnPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.25f, 0.75f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0.55f, 0.75f);

        spawnPos = Camera.main.ViewportToWorldPoint(viewportPos);
        spawnPos.z = 0f;

        GameObject bossObject = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        CowKing cowKing = bossObject.GetComponent<CowKing>();
        Collider2D bossCollider = bossObject.GetComponent<Collider2D>();

        if (bossCollider != null)
            bossCollider.enabled = false;

        hasSpawned = true;
        hasSpawnedBossOnce = true;

        if (cowKing != null)
            StartCoroutine(ActivateBossDelayed(cowKing, bossCollider));
    }

    private IEnumerator ActivateBossDelayed(CowKing cowKing, Collider2D bossCollider)
    {
        yield return new WaitForSeconds(activateDelay);

        if (bossCollider != null)
            bossCollider.enabled = true;

        cowKing.ActivateBoss();
    }
}