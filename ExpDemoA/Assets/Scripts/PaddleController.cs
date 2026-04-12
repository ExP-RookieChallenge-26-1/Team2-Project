using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : MonoBehaviour
{
	[SerializeField] private float minX = -7f;
	[SerializeField] private float maxX = 7f;
	
	void Update()
	{
		Vector2 mousePos;
		Vector3 world;
		float x;

		mousePos = Mouse.current.position.ReadValue();
		world = Camera.main.ScreenToWorldPoint(mousePos);
		x = Mathf.Clamp(world.x, minX, maxX);
		transform.position = new Vector3(x, transform.position.y, 0);
	}
}
