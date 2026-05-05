using UnityEngine;

public class BallPhysics
{
	private readonly Ball ball;
	public Vector2 velocity;

	public BallPhysics(Ball ball)
	{
		this.ball = ball;
	}

	public void Tick(float deltaTime)
	{
		if (deltaTime <= 0f)
			return;

		this.ball.transform.position += (Vector3)(velocity * deltaTime);
	}

	public void Launch()
	{
		float speed;

		speed = this.ball.Stats.speed;
		this.velocity = new Vector2(speed * 0.5f, speed * 0.866f);
	}

	public void ReflectX()
	{
		this.velocity.x *= -1;
	}

	public void ReflectY()
	{
		this.velocity.y *= -1;
	}

	public void ResolveWallCollision(Bounds bounds)
	{
		float overlapLeft;
		float overlapRight;
		float overlapBottom;
		float overlapTop;
		float minOverlap;
		float radius;
		float epsilon;
		Vector2 pos;

		radius = this.ball.Stats.radius;
		pos = this.ball.transform.position;

		overlapLeft = (pos.x + radius) - bounds.min.x;
		overlapRight = bounds.max.x - (pos.x - radius);
		overlapBottom = (pos.y + radius) - bounds.min.y;
		overlapTop = bounds.max.y - (pos.y - radius);

		minOverlap = Mathf.Min(overlapLeft, overlapRight, overlapBottom, overlapTop);
		epsilon = 0.001f;

		if (Mathf.Abs(minOverlap - overlapLeft) <= epsilon && this.velocity.x > 0)
			ReflectX();
		if (Mathf.Abs(minOverlap - overlapRight) <= epsilon && this.velocity.x < 0)
			ReflectX();
		if (Mathf.Abs(minOverlap - overlapBottom) <= epsilon && this.velocity.y > 0)
			ReflectY();
		if (Mathf.Abs(minOverlap - overlapTop) <= epsilon && this.velocity.y < 0)
			ReflectY();
	}

	public void ResolvePaddleCollision(Bounds bounds, Paddle paddle)
	{
		float speed;
		Vector2 pos;

		speed = this.ball.Stats.speed;
		pos = this.ball.transform.position;

		if (this.velocity.y >= 0)
			return;
		
		if (pos.y < bounds.max.y)
			return;
		

		this.velocity.x += paddle.Stats.reflectionWeight * paddle.Physics.Velocity;
		// TODO: When velocity.x exceeds speed, velocity.y becomes NaN. Need design decision.
		if (this.velocity.x * this.velocity.x > speed * speed)
			Debug.LogWarning("ResolvePaddleCollision: newVelocity.x exceeds speed. velocity.y will be NaN");
		
		this.velocity.y = Mathf.Sqrt(speed * speed - this.velocity.x * this.velocity.x);
	}
}