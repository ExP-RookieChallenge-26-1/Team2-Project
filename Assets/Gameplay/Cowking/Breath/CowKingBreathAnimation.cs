using System.Collections;
using UnityEngine;

public class CowKingBreathAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float endAnimationDuration = 0.4f;

    private bool isEnding;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayEndAndDestroy()
    {
        if (isEnding)
            return;

        isEnding = true;
        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        if (animator != null)
            animator.SetTrigger("End");

        yield return new WaitForSeconds(endAnimationDuration);

        Destroy(gameObject);
    }
}