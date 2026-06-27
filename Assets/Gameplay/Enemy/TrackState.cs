using UnityEngine;

public class TrackState : EnemyBaseState
{
	public override void Enter(Enemy enemy)
	{
		enemy.SetMoveAnimation(true);
	}

	public override void Exit(Enemy enemy)
	{
		enemy.SetMoveAnimation(false);
	}

	public override void Tick(Enemy enemy)
	{
		if (!enemy.TryGetPaddleDirection(out bool isLeft))
			return;

		enemy.SetHeading(isLeft ? Enemy.Heading.Left : Enemy.Heading.Right);

		bool canMove = isLeft
			? EnemyMovementValidator.CanMoveLeft(enemy)
			: EnemyMovementValidator.CanMoveRight(enemy);

		if (canMove)
			Move(enemy, isLeft);
	}

	private void Move(Enemy enemy, bool isLeft)
	{
		float dir = isLeft ? -1f : 1f;
		Vector3 pos = enemy.transform.position;
		pos.x = Mathf.Clamp(
			pos.x + dir * enemy.Stats.TrackSpeed * Time.deltaTime,
			enemy.MoveRange.min,
			enemy.MoveRange.max
		);
		enemy.transform.position = pos;
	}
}
