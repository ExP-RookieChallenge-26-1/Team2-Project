public class AttackState : EnemyBaseState
{
    private bool hasAttacked;
    private float attackTimer;
    private const float AttackDelay = 0.3f;

    public override void Enter(Enemy enemy)
    {
        hasAttacked = false;
        attackTimer = 0f;
        enemy.MarkAttacked();
        enemy.PlayAttackAnimation();
    }

    public override void Tick(Enemy enemy)
    {
        if (hasAttacked)
            return;

        attackTimer += UnityEngine.Time.deltaTime;

        if (attackTimer < AttackDelay)
            return;

        hasAttacked = true;

        GameManager.Instance.User.Health.TakeDamage(enemy.Stats.AttackDamage);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUserDamagedSound();

        enemy.ChangeState(enemy.IdleState);
    }
}