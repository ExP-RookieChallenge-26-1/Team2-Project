using UnityEngine;

public class BallAnimation
{
	private Animator animator;
	private Ball ball;
	private SpriteRenderer spriteRenderer;

	private int isMovingUpHash;
	private int attackHash;
	private int cloneHash;

	public BallAnimation(Ball ball)
	{
		this.ball = ball;
		this.animator = ball.GetComponent<Animator>();
		this.spriteRenderer = ball.GetComponent<SpriteRenderer>();
		this.isMovingUpHash = Animator.StringToHash("IsMovingUp");
		this.attackHash = Animator.StringToHash("Attack");
		this.cloneHash = Animator.StringToHash("Clone");

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

	public void TriggerClone()
	{
		this.animator.SetTrigger(this.cloneHash);
	}

	public void TriggerAttack()
	{
		this.animator.SetTrigger(this.attackHash);
	}

	private void UpdateFlipState()
	{
		if (this.ball.Physics.Velocity.x == 0f)
			return;

		this.spriteRenderer.flipX = this.ball.Physics.Velocity.x < 0f;
	}
}
