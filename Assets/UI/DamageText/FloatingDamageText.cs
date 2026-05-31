using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
	[SerializeField] private TextMeshPro text;
	[SerializeField] private float floatSpeed = 1f;
	[SerializeField] private float duration = 1f;

	private float elapsed;

	public void Initialize(int damage, Color color)
	{
		this.text.text = damage.ToString();
		this.text.color = color;
	}

	private void Update()
	{
		this.elapsed += Time.unscaledDeltaTime;
		transform.position += Vector3.up * this.floatSpeed * Time.unscaledDeltaTime;
		this.text.alpha = 1f - (this.elapsed / this.duration);
		if (this.elapsed >= this.duration)
			Destroy(gameObject);
	}
}
