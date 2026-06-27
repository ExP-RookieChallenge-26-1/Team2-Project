public class AttackState : EnemyBaseState
{
    private bool hasAttacked;

    public override void Enter(Enemy enemy)
    {
        hasAttacked = false;
        enemy.MarkAttacked();
        enemy.FacePaddle();
        enemy.SetMoveAnimation(false);
        enemy.PlayAttackAnimation();
    }

    public override void Tick(Enemy enemy)
    {
        enemy.FacePaddle();
    }

    public void CompleteAttack(Enemy enemy)
    {
        if (hasAttacked)
            return;

        hasAttacked = true;

        if (enemy.IsPaddleOverlappingAttackCollider())
        {
            GameManager.Instance.User.Health.TakeDamage(enemy.Stats.AttackDamage);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUserDamagedSound();
        }

        enemy.ChangeState(enemy.IdleState);
    }
}
