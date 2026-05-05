using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Ball ball;
    [SerializeField] private Paddle paddle;
    [SerializeField] private World world;

    [Header("Simulation")]
    [SerializeField] private int steps = 40;
    [SerializeField] private float dt = 0.05f;

    [Header("Visual")]
    [SerializeField] private float startWidth = 0.05f;
    [SerializeField] private float endWidth = 0.01f;
    [SerializeField] private float dashTiling = 8f;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        SetupVisual();
    }

    private void Update()
    {
        Simulate();
    }

    private void Simulate()
    {
        if (ball == null) return;

        Vector2 simPos = ball.transform.position;
        Vector2 simVel = ball.Physics.velocity;

        float radius = ball.Stats.radius;
        float speed = ball.Stats.speed;

        Vector3[] positions = new Vector3[steps];
        int validCount = 0;

        for (int i = 0; i < steps; i++)
        {
            simPos += simVel * dt;

            ResolveCollision(simPos, ref simVel, radius);
            ResolveTile(simPos, ref simVel, radius);
            ResolvePaddle(simPos, ref simVel, radius, speed);

            positions[i] = simPos;
            validCount = i + 1;

            if (float.IsNaN(simVel.x) || float.IsNaN(simVel.y))
                break;
        }

        line.positionCount = validCount;

        for (int i = 0; i < validCount; i++)
            line.SetPosition(i, positions[i]);
    }

    // -------------------------
    // Visual (🔥 핵심)
    // -------------------------

    private void SetupVisual()
    {
        // Material (기본 Sprite shader 사용)
        line.material = new Material(Shader.Find("Sprites/Default"));

        // 점선 텍스처 생성 (코드로)
        line.material.mainTexture = GenerateDashTexture();

        // 점선 반복
        line.textureMode = LineTextureMode.Tile;
        line.material.mainTextureScale = new Vector2(dashTiling, 1f);

        // 두께 (곡선)
        line.widthCurve = new AnimationCurve(
            new Keyframe(0f, startWidth),
            new Keyframe(1f, endWidth)
        );

        // 부드러운 끝
        line.numCornerVertices = 4;
        line.numCapVertices = 4;

        // 색 + 알파
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.yellow, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        line.colorGradient = gradient;
    }

    // 점선 텍스처 자동 생성
    private Texture2D GenerateDashTexture()
    {
        int width = 64;
        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;

        for (int i = 0; i < width; i++)
        {
            bool visible = (i % 8) < 4; // 4px on / 4px off
            tex.SetPixel(i, 0, visible ? Color.white : new Color(1, 1, 1, 0));
        }

        tex.Apply();
        return tex;
    }

    // -------------------------
    // Collision
    // -------------------------

    private void ResolveCollision(Vector2 pos, ref Vector2 vel, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, radius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Wall"))
                ResolveWallBounds(hit.bounds, pos, ref vel, radius);
        }
    }

    private void ResolveWallBounds(Bounds bounds, Vector2 pos, ref Vector2 vel, float radius)
    {
        float overlapLeft = (pos.x + radius) - bounds.min.x;
        float overlapRight = bounds.max.x - (pos.x - radius);
        float overlapBottom = (pos.y + radius) - bounds.min.y;
        float overlapTop = bounds.max.y - (pos.y - radius);

        float minOverlap = Mathf.Min(overlapLeft, overlapRight, overlapBottom, overlapTop);
        float eps = 0.001f;

        if (Mathf.Abs(minOverlap - overlapLeft) <= eps && vel.x > 0) vel.x *= -1;
        if (Mathf.Abs(minOverlap - overlapRight) <= eps && vel.x < 0) vel.x *= -1;
        if (Mathf.Abs(minOverlap - overlapBottom) <= eps && vel.y > 0) vel.y *= -1;
        if (Mathf.Abs(minOverlap - overlapTop) <= eps && vel.y < 0) vel.y *= -1;
    }

    private void ResolvePaddle(Vector2 pos, ref Vector2 vel, float radius, float speed)
    {
        if (paddle == null) return;

        Bounds bounds = paddle.GetComponent<Collider2D>().bounds;

        if (vel.y >= 0) return;
        if (pos.y > bounds.max.y + radius) return;

        vel.x += paddle.Stats.reflectionWeight * paddle.Physics.Velocity;

        // ❗ NaN 유지
        vel.y = Mathf.Sqrt(speed * speed - vel.x * vel.x);
    }

    private void ResolveTile(Vector2 pos, ref Vector2 vel, float radius)
    {
        Tilemap[] tilemaps = world.Spawner.GetActiveTilemaps();

        foreach (var tilemap in tilemaps)
        {
            if (tilemap == null) continue;

            int range = Mathf.CeilToInt(radius / tilemap.cellSize.x) + 1;
            Vector3Int center = tilemap.WorldToCell(pos);

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector3Int cell = new Vector3Int(center.x + x, center.y + y, 0);

                    if (!tilemap.HasTile(cell)) continue;

                    Vector3 worldPos = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;
                    Bounds bounds = new Bounds(worldPos, tilemap.cellSize);

                    if (IsTouching(pos, bounds, radius))
                        ResolveWallBounds(bounds, pos, ref vel, radius);
                }
            }
        }
    }

    private bool IsTouching(Vector2 pos, Bounds bounds, float radius)
    {
        Vector2 closest;
        closest.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        closest.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);

        Vector2 dist = pos - closest;
        return dist.sqrMagnitude <= radius * radius;
    }
}