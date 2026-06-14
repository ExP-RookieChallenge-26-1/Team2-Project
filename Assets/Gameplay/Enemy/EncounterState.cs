using UnityEngine;

public class EncounterState : EnemyBaseState
{
    private float timer;

    public override void Enter(Enemy enemy)
    {
        timer = 0f;
        enemy.SetMoveAnimation(false);
        enemy.PlayEncounterAnimation();
    }

    public override void Tick(Enemy enemy)
    {
        timer += Time.deltaTime;

        if (timer >= enemy.EncounterDuration)
            enemy.ChangeState(enemy.TrackState);
    }

    public override void Exit(Enemy enemy)
    {
    }
}