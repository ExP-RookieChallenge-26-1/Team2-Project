using UnityEngine;

public class TrackState : EnemyBaseState
{
	public override void Enter(Enemy enemy)
	{
		Debug.Log($"[Enemy] → TrackState");
		OnEncounterAnimation(enemy);
	}

	public override void Tick(Enemy enemy)
	{
		float paddleX = GameManager.Instance.Paddle.transform.position.x;
		bool isLeft = paddleX < enemy.transform.position.x;

		bool canMove = isLeft
			? EnemyMovementValidator.CanMoveLeft(enemy)
			: EnemyMovementValidator.CanMoveRight(enemy);

		if (canMove)
		{
			Move(enemy, isLeft);
			OnMoveAnimation(enemy);
		}
		else
		{
			OnIdleAnimation(enemy);
		}
	}

	private void Move(Enemy enemy, bool isLeft)
	{
		float dir = isLeft ? -1f : 1f;
		enemy.SetHeading(isLeft ? Enemy.Heading.Left : Enemy.Heading.Right);
		Vector3 pos = enemy.transform.position;
		pos.x = Mathf.Clamp(
			pos.x + dir * enemy.Stats.TrackSpeed * Time.deltaTime,
			enemy.MoveRange.min,
			enemy.MoveRange.max
		);
		enemy.transform.position = pos;
	}

	private void OnEncounterAnimation(Enemy enemy) { }
	private void OnMoveAnimation(Enemy enemy) { }
	private void OnIdleAnimation(Enemy enemy) { }
}
