using UnityEngine;

public class BallAnimation
{
	private Animator animator;
	private Ball ball;

	private int isMovingUpHash;

	public BallAnimation(Animator animator, Ball ball)
	{
		this.animator = animator;
		this.ball = ball;
		this.isMovingUpHash = Animator.StringToHash("IsMovingUp");

		if (this.animator == null)
			Debug.LogError("BallAnimation: Animator를 찾을 수 없습니다!");
	}

	public void Tick()
	{
		if (this.animator == null || this.ball == null)
			return;

		UpdateIdleUpDownState();
	}

	private void UpdateIdleUpDownState()
	{
		this.animator.SetBool(this.isMovingUpHash, this.ball.Physics.Velocity.y >= 0f);
	}
}
