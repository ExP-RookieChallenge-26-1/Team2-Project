using UnityEngine;

public class BallAnimation
{
	private Animator animator;
	private Ball ball;
	private SpriteRenderer spriteRenderer;

	private int isMovingUpHash;
	private int attackHash;
	private int isRespawningHash;
	private int isGameOverHash;

	public BallAnimation(Ball ball)
	{
		this.ball = ball;
		this.animator = ball.GetComponent<Animator>();
		this.spriteRenderer = ball.GetComponent<SpriteRenderer>();
		this.isMovingUpHash = Animator.StringToHash("IsMovingUp");
		this.attackHash = Animator.StringToHash("Attack");
		this.isRespawningHash = Animator.StringToHash("IsRespawning");
		this.isGameOverHash = Animator.StringToHash("IsGameOver");

		if (this.animator == null)
			Debug.LogError("BallAnimation: Animator를 찾을 수 없습니다!");
	}

	public void Tick()
	{
		if (this.animator == null || this.ball == null)
			return;

		UpdateIdleUpDownState();
		UpdateFlipState();
	}

	private void UpdateIdleUpDownState()
	{
		this.animator.SetBool(this.isMovingUpHash, this.ball.Physics.Velocity.y >= 0f);
	}

	public void TriggerUpsizing()
	{
		// Giant visual clips were removed; GiantSkill still handles size changes.
	}

	public void TriggerDownsizing()
	{
		// Giant visual clips were removed; GiantSkill still handles size reset.
	}

	public void TriggerClone()
	{
		// Clone visual clips were removed; CloneSkill still handles spawning.
	}

	public void TriggerAttack()
	{
		this.animator.SetTrigger(this.attackHash);
	}

	public void SetRespawning(bool value)
	{
		this.animator.SetBool(this.isRespawningHash, value);
	}

	public void SetGameOver()
	{
		this.animator.SetBool(this.isGameOverHash, true);
	}

	private void UpdateFlipState()
	{
		if (this.spriteRenderer == null || this.ball.Physics.Velocity.x == 0f)
			return;

		this.spriteRenderer.flipX = this.ball.Physics.Velocity.x < 0f;
	}
}
