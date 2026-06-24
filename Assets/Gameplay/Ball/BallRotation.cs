using UnityEngine;

public class BallRotation
{
    private readonly Ball ball;

    public bool IsEnabled { get; set; } = false;

    public BallRotation(Ball ball)
    {
        this.ball = ball;
    }

    public void Tick()
    {
        if (!this.IsEnabled)
            return;

        Vector2 velocity = this.ball.Physics.Velocity;
        if (velocity.sqrMagnitude == 0f)
            return;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
        this.ball.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
