using UnityEngine;

public class PaddlePhysics
{
	private readonly Paddle paddle;
	public float Velocity { get; private set; }

	public PaddlePhysics(Paddle paddle)
	{
		this.paddle = paddle;
	}

	public void Tick()
	{
		Vector2 pointerDelta;
		float moveRange;
		float worldDeltaPosX;
		float newPosX;
		
		if (Time.deltaTime <= 0f)
			return;

		pointerDelta = GameManager.Instance.Input.PointerDelta;

		if (!GameManager.Instance.Input.PointerPressed)
		{
			this.Velocity = 0f;
			return;
		}

		moveRange = this.paddle.Stats.MoveRange;
		worldDeltaPosX = pointerDelta.x / Screen.width * moveRange * 2f;
		this.Velocity = Mathf.Clamp(worldDeltaPosX / Time.deltaTime, -this.paddle.Stats.MaxPaddleSpeed, this.paddle.Stats.MaxPaddleSpeed);

		newPosX = Mathf.Clamp(this.paddle.transform.position.x + worldDeltaPosX, -moveRange, moveRange);
		this.paddle.transform.position = new Vector2(newPosX, this.paddle.transform.position.y);
	}
}