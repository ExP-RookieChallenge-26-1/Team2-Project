using UnityEngine;

public class BallAnimation
{
	private Animator animator;
	private Ball ball;
	private SpriteRenderer spriteRenderer;

	private int isMovingUpHash;
	private int attackHash;
	private int cloneHash;
	private int upsizingHash;
	private int downsizingHash;
	private int isRespawningHash;
	private int isGameOverHash;

	public BallAnimation(Ball ball)
	{
		this.ball = ball;
		this.animator = ball.GetComponent<Animator>();
		this.spriteRenderer = ball.GetComponent<SpriteRenderer>();
		this.isMovingUpHash = Animator.StringToHash("IsMovingUp");
		this.attackHash = Animator.StringToHash("Attack");
		this.cloneHash = Animator.StringToHash("Clone");
		this.upsizingHash = Animator.StringToHash("Upsizing");
		this.downsizingHash = Animator.StringToHash("Downsizing");
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
		this.animator.SetTrigger(this.upsizingHash);
	}

	public void TriggerDownsizing()
	{
		this.animator.SetTrigger(this.downsizingHash);
	}

	public void TriggerClone()
	{
		this.animator.SetTrigger(this.cloneHash);
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
		if (this.ball.Physics.Velocity.x == 0f)
			return;

		this.spriteRenderer.flipX = this.ball.Physics.Velocity.x < 0f;
	}
}
