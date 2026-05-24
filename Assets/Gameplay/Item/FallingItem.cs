using UnityEngine;

public class FallingItem : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float destroyY = -7f;

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State.Current != GameStateMachine.State.Playing)
            return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }
}