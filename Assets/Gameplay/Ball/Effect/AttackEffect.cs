using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public float ScrollSpeed { get; set; }
    public float DestroyDelay { get; set; }

    private void Start()
    {
        Destroy(gameObject, this.DestroyDelay);
    }

    private void Update()
    {
        transform.position += Vector3.down * this.ScrollSpeed * Time.deltaTime;
    }
}
