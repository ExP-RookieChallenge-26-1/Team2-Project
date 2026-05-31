public static class EnemyMovementValidator
{
	public static bool CanMoveLeft(Enemy enemy)
	{
		return enemy.transform.position.x > enemy.MoveRange.min;
	}

	public static bool CanMoveRight(Enemy enemy)
	{
		return enemy.transform.position.x < enemy.MoveRange.max;
	}
}
