using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class BallTrajectory : MonoBehaviour
{
	private Ball ball;
	private LineRenderer lineRenderer;
	private Vector3[] trajectoryPoints;
	[SerializeField] private Paddle paddle;
	[SerializeField] private int steps = 40;
	[SerializeField] private float deltaTime = 0.05f;

	private void Awake()
	{
		this.ball = GetComponent<Ball>();
		this.lineRenderer = GetComponent<LineRenderer>();
		this.trajectoryPoints = new Vector3[this.steps];
		InitializeLineRenderer();
	}

	public void Tick()
	{
		Simulate();
	}

	private void Simulate()
	{
		Vector2 position;
		Vector2 velocity;
		int trajectoryCount;

		position = this.ball.transform.position;
		velocity = this.ball.Physics.Velocity;
		trajectoryCount = this.steps;

		for  (int i = 0; i < this.steps; ++i)
		{
			BallCollision.Collision collision;

			position += velocity * this.deltaTime;
			collision = BallCollision.DetectClosestCollision(position, this.ball.Stats.Radius, this.ball.Physics.Velocity);

			switch (collision.type)
			{
				case BallCollision.Collision.Type.Wall:
				case BallCollision.Collision.Type.Terrain:
					velocity = BallPhysics.CalculateWallReflection(collision.bounds, position, velocity, this.ball.Stats.Radius);
					break;
				case BallCollision.Collision.Type.Paddle:
					velocity = BallPhysics.CalculatePaddleReflectionAlternative(collision.bounds, position, velocity, this.ball.Stats.Speed, this.paddle);
					break;
			}

			if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y))
			{
				trajectoryCount = i;
				break;
			}

			trajectoryPoints[i] = position;
		}

		this.lineRenderer.positionCount = trajectoryCount;
		this.lineRenderer.SetPositions(this.trajectoryPoints);
	}

	private void InitializeLineRenderer()
	{
		Gradient gradient;
		GradientColorKey[] colorKeys;
		GradientAlphaKey[] alphaKeys;

		this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		this.lineRenderer.material.mainTexture = GenerateDashTexture();
		this.lineRenderer.textureMode = LineTextureMode.Tile;
		this.lineRenderer.material.mainTextureScale = new Vector2(8f, 1f);
		this.lineRenderer.widthCurve = new AnimationCurve(
			new Keyframe(0f, 0.05f),
			new Keyframe(1f, 0.01f)
		);
		this.lineRenderer.numCornerVertices = 4;
		this.lineRenderer.numCapVertices = 4;
		
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(Color.yellow, 0f),
			new GradientColorKey(Color.yellow, 1f)
		};
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(0.4f, 0.5f),
			new GradientAlphaKey(0f, 1f)
		};

		gradient = new Gradient();
		gradient.SetKeys(colorKeys, alphaKeys);
		this.lineRenderer.colorGradient = gradient;
	}

	private Texture2D GenerateDashTexture()
	{
		int width;
		Texture2D texture;

		width = 64;
		texture = new Texture2D(width, 1, TextureFormat.RGBA32, false);
		texture.wrapMode = TextureWrapMode.Repeat;

		for (int i = 0; i < width; ++i)
		{
			bool visible;
			Color color;

			visible = (i % 8) < 4;
			color = visible ? Color.white : new Color(1f, 1f, 1f, 0f);
			texture.SetPixel(i, 0, color);
		}

		texture.Apply();
		return texture;
	}
}
