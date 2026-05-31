public abstract class EnemyBaseState : IEnemyState
{
	public virtual void Enter(Enemy enemy) { }
	public virtual void Exit(Enemy enemy) { }
	public virtual void Tick(Enemy enemy) { }
}
