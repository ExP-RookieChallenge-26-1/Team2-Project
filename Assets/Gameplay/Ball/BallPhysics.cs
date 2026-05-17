using UnityEngine;

public class BallPhysics
{
	private readonly Ball ball;
	public Vector2 Velocity { get; private set; }

	public BallPhysics(Ball ball)
	{
		this.ball = ball;
	}

	public void Tick()
	{
		if (Time.deltaTime <= 0f)
			return;

		this.ball.transform.position += (Vector3)(Velocity * Time.deltaTime);
	}

	public void Launch()
	{
		float speed;

		speed = this.ball.Stats.Speed;
		this.Velocity = new Vector2(speed * 0.5f, speed * 0.866f);
	}

	public void ApplyWallReflection(Bounds bounds)
	{
		this.Velocity = CalculateWallReflection(bounds, this.ball.transform.position, this.Velocity, this.ball.Stats.Radius);
	}

	public void ApplyPaddleReflection(Bounds bounds, Paddle paddle)
	{
		this.Velocity = CalculatePaddleReflection(bounds, this.ball.transform.position, this.Velocity, this.ball.Stats.Speed, paddle);
	}

	public void ApplyPaddleReflectionAlternative(Bounds bounds, Paddle paddle)
	{
		this.Velocity = CalculatePaddleReflectionAlternative(bounds, this.ball.transform.position, this.Velocity, this.ball.Stats.Speed, paddle);
	}

	public static Vector2 CalculateWallReflection(Bounds bounds, Vector2 pos, Vector2 velocity, float radius)
	{
		float overlapLeft;
		float overlapRight;
		float overlapBottom;
		float overlapTop;
		float minOverlap;
		float epsilon;

		overlapLeft = (pos.x + radius) - bounds.min.x;
		overlapRight = bounds.max.x - (pos.x - radius);
		overlapBottom = (pos.y + radius) - bounds.min.y;
		overlapTop = bounds.max.y - (pos.y - radius);

		minOverlap = Mathf.Min(overlapLeft, overlapRight, overlapBottom, overlapTop);
		epsilon = 0.001f;

		if (Mathf.Abs(minOverlap - overlapLeft) <= epsilon && velocity.x > 0)
			velocity.x *= -1;
		if (Mathf.Abs(minOverlap - overlapRight) <= epsilon && velocity.x < 0)
			velocity.x *= -1;
		if (Mathf.Abs(minOverlap - overlapBottom) <= epsilon && velocity.y > 0)
			velocity.y *= -1;
		if (Mathf.Abs(minOverlap - overlapTop) <= epsilon && velocity.y < 0)
			velocity.y *= -1;

		return velocity;
	}

	public static Vector2 CalculatePaddleReflection(Bounds bounds, Vector2 pos, Vector2 velocity, float speed, Paddle paddle)
	{
		float minVelocityY;
		float maxVelocityX;
		
		if (velocity.y >= 0)
			return velocity;
		
		if (pos.y < bounds.max.y)
			return velocity;

		minVelocityY = speed * Mathf.Sin(20f * Mathf.Deg2Rad);
		maxVelocityX = Mathf.Sqrt(speed * speed - minVelocityY * minVelocityY);

		velocity.x += paddle.Stats.ReflectionWeight * paddle.Physics.Velocity;
		velocity.x = Mathf.Clamp(velocity.x, -maxVelocityX, maxVelocityX);
		velocity.y = Mathf.Sqrt(speed * speed - velocity.x * velocity.x);

		return velocity;
	}

	public static Vector2 CalculatePaddleReflectionAlternative(Bounds bounds, Vector2 pos, Vector2 velocity, float speed, Paddle paddle)
	{
		float hitPos;
		float normalizedHit;
		float angle;
		float minAngle;
		float maxAngle;

		if (velocity.y >= 0)
			return velocity;

		if (pos.y < bounds.max.y)
			return velocity;

		hitPos = pos.x - bounds.center.x;
		normalizedHit = Mathf.Clamp(hitPos / (bounds.size.x * 0.5f), -1f, 1f);

		minAngle = 15f * Mathf.Deg2Rad;
		maxAngle = 90f * Mathf.Deg2Rad;
		angle = Mathf.Lerp(maxAngle, minAngle, Mathf.Abs(normalizedHit));

		velocity.x = speed * Mathf.Cos(angle) * Mathf.Sign(normalizedHit);
		velocity.y = speed * Mathf.Sin(angle);

		return velocity;
	}
}