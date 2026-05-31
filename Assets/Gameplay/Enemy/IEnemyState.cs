public interface IEnemyState
{
	void Enter(Enemy enemy);
	void Exit(Enemy enemy);
	void Tick(Enemy enemy);
}
