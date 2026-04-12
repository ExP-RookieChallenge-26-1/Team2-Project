using UnityEngine;
using UnityEngine.SceneManagement;

public class BallController : MonoBehaviour
{
	[SerializeField] private float speed = 8f;
	Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		rb.linearVelocity = new Vector2(1f, 1f).normalized * speed;
	}

	void Update()
	{
		if (transform.position.y < -7f)
			SceneManager.LoadScene("TitleScene");
	}

	void FixedUpdate()
	{
		rb.linearVelocity = rb.linearVelocity.normalized * speed;
	}
}
