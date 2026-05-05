using UnityEngine;

public class PaddlePhysics
{
	private readonly Paddle paddle;
	public float Velocity { get; private set; }

	public PaddlePhysics(Paddle paddle)
	{
		this.paddle = paddle;
	}

	public void Tick(float deltaTime)
	{
		Vector2 pointerDelta;
		float moveRange;
		float worldDeltaPosX;
		float newPosX;
		
		if (deltaTime <= 0f)
			return;

		pointerDelta = GameManager.Instance.Input.PointerDelta;

		if (!GameManager.Instance.Input.PointerPressed)
		{
			this.Velocity = 0f;
			return;
		}

		moveRange = this.paddle.Stats.moveRange;
		worldDeltaPosX = pointerDelta.x / Screen.width * moveRange * 2f;
		this.Velocity = Mathf.Clamp(worldDeltaPosX / deltaTime, -this.paddle.Stats.maxPaddleSpeed, this.paddle.Stats.maxPaddleSpeed);

		newPosX = Mathf.Clamp(this.paddle.transform.position.x + worldDeltaPosX, -moveRange, moveRange);
		this.paddle.transform.position = new Vector2(newPosX, this.paddle.transform.position.y);
	}
}