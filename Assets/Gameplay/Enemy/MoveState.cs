using UnityEngine;

public class MoveState : EnemyBaseState
{
	private float decisionTimer;

	public override void Enter(Enemy enemy)
	{
		decisionTimer = 0f;
		enemy.SetHeading(this == enemy.MoveLeftState
			? Enemy.Heading.Left
			: Enemy.Heading.Right);
	}

	public override void Tick(Enemy enemy)
	{
		// 범위 벗어나면 즉시 Idle 전환
		bool canContinue = (this == enemy.MoveLeftState)
			? EnemyMovementValidator.CanMoveLeft(enemy)
			: EnemyMovementValidator.CanMoveRight(enemy);

		if (!canContinue)
		{
			enemy.ChangeState(enemy.IdleState);
			return;
		}

		Move(enemy);

		// 0.1초마다 Idle 전환 여부 판정
		decisionTimer += Time.deltaTime;
		if (decisionTimer >= enemy.Stats.MoveDecisionInterval)
		{
			decisionTimer = 0f;
			DecideNextState(enemy);
		}
	}

	private void Move(Enemy enemy)
	{
		float dir = (this == enemy.MoveLeftState) ? -1f : 1f;
		Vector3 pos = enemy.transform.position;
		pos.x = Mathf.Clamp(
			pos.x + dir * enemy.Stats.MoveSpeed * Time.deltaTime,
			enemy.MoveRange.min,
			enemy.MoveRange.max
		);
		enemy.transform.position = pos;
	}

	private void DecideNextState(Enemy enemy)
	{
		// 1 - StateChangeRate 확률로 Idle 전환
		if (Random.value >= enemy.Stats.StateChangeRate)
			enemy.ChangeState(enemy.IdleState);
	}
}
