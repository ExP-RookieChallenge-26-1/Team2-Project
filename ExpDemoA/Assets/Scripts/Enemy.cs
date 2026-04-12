using UnityEngine;

public class Enemy : MonoBehaviour
{
	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Ball"))
			Destroy(gameObject);
	}
}
