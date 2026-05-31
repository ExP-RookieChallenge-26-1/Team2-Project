public class AttackState : EnemyBaseState
{
	private bool hasAttacked;

	public override void Enter(Enemy enemy)
	{
		hasAttacked = false;
		enemy.MarkAttacked();
		OnAttackAnimation(enemy);
	}

	public override void Tick(Enemy enemy)
	{
		if (hasAttacked) return;
		hasAttacked = true;
		GameManager.Instance.User.Health.TakeDamage(enemy.Stats.AttackDamage);
		enemy.ChangeState(enemy.IdleState);
	}

	private void OnAttackAnimation(Enemy enemy) { }
}
