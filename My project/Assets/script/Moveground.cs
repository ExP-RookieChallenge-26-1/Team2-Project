using UnityEngine;

public class Moveground : MonoBehaviour
{
    public float speed = 1f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 15f);
    }

    void FixedUpdate()
    {
        Vector2 newPos = rb.position + Vector2.down * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}
