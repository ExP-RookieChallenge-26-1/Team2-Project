using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
	public static DamageTextSpawner Instance { get; private set; }

	[SerializeField] private FloatingDamageText prefab;
	[SerializeField] private float spawnRangeX = 0.5f;
	[SerializeField] private float spawnRangeY = 0.5f;

	private void Awake()
	{
		Instance = this;
	}

	public void Spawn(Vector3 position, int damage, Color color)
	{
		float offsetX = Random.Range(-this.spawnRangeX, this.spawnRangeX);
		float offsetY = Random.Range(-this.spawnRangeY, this.spawnRangeY);
		FloatingDamageText instance = Instantiate(
			this.prefab,
			position + new Vector3(offsetX, offsetY, 0f),
			Quaternion.identity
		);
		instance.Initialize(damage, color);
	}
}
