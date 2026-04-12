using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D rb;
    public float speed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 시작 방향
        Vector2 dir = new Vector2(1f, -1f).normalized;
        rb.linearVelocity = dir * speed;
    }

    void Update()
    {
        // 속도 항상 유지
        rb.linearVelocity = rb.linearVelocity.normalized * speed;

        if (Mathf.Abs(rb.linearVelocity.x) < 1f)
        {
            float dir = Random.value < 0.5f ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir, rb.linearVelocity.y).normalized * speed;
        }
    }
}