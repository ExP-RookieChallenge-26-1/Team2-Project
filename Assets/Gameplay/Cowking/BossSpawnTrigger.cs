using System.Collections;
using UnityEngine;

public class BossSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private float activateDelay = 1.5f;

    private bool hasSpawned = false;

    private void Update()
    {
        if (hasSpawned)
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
        if (hasSpawned)
            return;

        Vector3 spawnPos = bossSpawnPoint.position;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(spawnPos);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.25f, 0.75f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0.65f, 0.85f);

        spawnPos = Camera.main.ViewportToWorldPoint(viewportPos);
        spawnPos.z = 0f;

        GameObject bossObject = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        CowKing cowKing = bossObject.GetComponent<CowKing>();

        hasSpawned = true;

        if (cowKing != null)
            StartCoroutine(ActivateBossDelayed(cowKing));
    }

    private IEnumerator ActivateBossDelayed(CowKing cowKing)
    {
        yield return new WaitForSeconds(activateDelay);
        cowKing.ActivateBoss();
    }
}