using UnityEngine;

public class IdleState : EnemyBaseState
{
	private float decisionTimer;

	public override void Enter(Enemy enemy)
	{
		decisionTimer = 0f;
	}

	public override void Tick(Enemy enemy)
	{
		decisionTimer += Time.deltaTime;

		if (decisionTimer >= enemy.Stats.IdleDecisionInterval)
		{
			decisionTimer = 0f;
			DecideNextState(enemy);
		}
	}

	private void DecideNextState(Enemy enemy)
	{
		float random = Random.value;

		// idle 유지: 1 - StateChangeRate 확률
		if (random >= enemy.Stats.StateChangeRate)
			return;

		// move_left: StateChangeRate / 2 확률
		if (random < enemy.Stats.StateChangeRate / 2f)
		{
			if (EnemyMovementValidator.CanMoveLeft(enemy))
				enemy.ChangeState(enemy.MoveLeftState);
			// 불가능하면 Idle 유지
		}
		else
		{
			// move_right: StateChangeRate / 2 확률
			if (EnemyMovementValidator.CanMoveRight(enemy))
				enemy.ChangeState(enemy.MoveRightState);
			// 불가능하면 Idle 유지
		}
	}
}
