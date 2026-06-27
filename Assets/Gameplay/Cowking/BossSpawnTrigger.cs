using System.Collections;
using UnityEngine;

public class BossSpawnTrigger : MonoBehaviour
{
    private const string EntryStateName = "Cowking_Entry";
    private const string MoveStateName = "CowKing_Move";
    private const string IsMovingParameterName = "IsMoving";

    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private float activateDelay = 1.5f;

    private GameObject spawnedBossObject;
    private CowKing spawnedCowKing;
    private Collider2D spawnedBossCollider;
    private Animator spawnedBossAnimator;
    private bool hasActivated = false;

    private static bool hasSpawnedBossOnce = false;

    public static void ResetSessionState()
    {
        hasSpawnedBossOnce = false;
    }

    private void Start()
    {
        PreSpawnBoss();
    }

    private void Update()
    {
        if (hasActivated || hasSpawnedBossOnce)
            return;

        if (spawnedBossObject == null)
            PreSpawnBoss();

        if (spawnedBossObject == null || bossSpawnPoint == null || Camera.main == null)
            return;

        if (IsBossSpawnPointInActivationZone())
            TrySpawnBoss();
    }

    private void PreSpawnBoss()
    {
        if (spawnedBossObject != null || hasSpawnedBossOnce)
            return;

        if (bossPrefab == null || bossSpawnPoint == null)
            return;

        spawnedBossObject = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        spawnedBossObject.transform.SetParent(transform, true);

        spawnedCowKing = spawnedBossObject.GetComponent<CowKing>();
        spawnedBossCollider = spawnedBossObject.GetComponent<Collider2D>();
        spawnedBossAnimator = spawnedBossObject.GetComponent<Animator>();

        PrepareBossForApproach();
    }

    private bool IsBossSpawnPointInActivationZone()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(bossSpawnPoint.position);

        return viewportPos.z > 0f &&
               viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0.55f && viewportPos.y <= 0.75f;
    }

    private void PrepareBossForApproach()
    {
        if (spawnedBossCollider != null)
            spawnedBossCollider.enabled = false;

        PlayApproachAnimation();
    }

    private void PlayApproachAnimation()
    {
        if (spawnedBossAnimator == null)
            return;

        spawnedBossAnimator.enabled = true;

        if (spawnedBossAnimator.runtimeAnimatorController == null)
            return;

        spawnedBossAnimator.SetBool(IsMovingParameterName, true);
        spawnedBossAnimator.Play(MoveStateName, 0, 0f);
        spawnedBossAnimator.Update(0f);
    }

    private void TrySpawnBoss()
    {
        if (hasActivated || hasSpawnedBossOnce)
            return;

        if (spawnedBossObject == null)
            PreSpawnBoss();

        if (spawnedBossObject == null)
            return;

        hasActivated = true;
        hasSpawnedBossOnce = true;

        DetachBossFromChunk();
        PlayEntryAnimation();

        if (activateDelay <= 0f)
            ActivateBoss();
        else
            StartCoroutine(ActivateBossDelayed());
    }

    private void DetachBossFromChunk()
    {
        if (spawnedBossObject != null)
            spawnedBossObject.transform.SetParent(null, true);
    }

    private void PlayEntryAnimation()
    {
        if (spawnedBossAnimator == null)
            return;

        spawnedBossAnimator.enabled = true;

        if (spawnedBossAnimator.runtimeAnimatorController == null)
            return;

        spawnedBossAnimator.SetBool(IsMovingParameterName, false);
        spawnedBossAnimator.Play(EntryStateName, 0, 0f);
        spawnedBossAnimator.Update(0f);
    }

    private IEnumerator ActivateBossDelayed()
    {
        yield return new WaitForSeconds(activateDelay);

        ActivateBoss();
    }

    private void ActivateBoss()
    {
        if (spawnedBossCollider != null)
            spawnedBossCollider.enabled = true;

        if (spawnedCowKing != null)
            spawnedCowKing.ActivateBoss();
    }
}
