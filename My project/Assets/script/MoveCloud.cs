using UnityEngine;

public class MoveCloud : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)
            );

            float targetX = mousePos.x;

            float newX = Mathf.Lerp(transform.position.x, targetX, speed * Time.deltaTime);

            float clampedX = Mathf.Clamp(newX, -2.5f, 2.5f);

            transform.position = new Vector3(clampedX, transform.position.y, 0f);
        }
    }
}