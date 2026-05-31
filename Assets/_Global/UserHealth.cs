using UnityEngine;

public class UserHealth : MonoBehaviour
{
	[SerializeField] private int maxHp = 3;

	public int MaxHp => this.maxHp;
	public int CurrentHp { get; private set; }

	private void Awake()
	{
		this.CurrentHp = this.maxHp;
	}

	public void TakeDamage(int damage)
	{
		this.CurrentHp -= damage;
		DamageTextSpawner.Instance.Spawn(GameManager.Instance.Paddle.transform.position, damage, Color.red);
		if (this.CurrentHp <= 0)
		{
			this.CurrentHp = 0;
			GameManager.Instance.State.Change(GameStateMachine.State.GameOver);
		}
	}
}
