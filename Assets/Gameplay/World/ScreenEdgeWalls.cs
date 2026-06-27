using UnityEngine;

public class ScreenEdgeWalls : MonoBehaviour
{
    private const string LeftWallName = "LeftWall";
    private const string RightWallName = "RightWall";
    private const string CeilingName = "Ceiling";
    private const string PhysicalLeftWallName = "WallLeft";
    private const string PhysicalRightWallName = "WallRight";
    private const string VisualContainerName = "ScreenEdgeWallVisuals";
    private const string DefaultSortingLayerName = "UI";
    private const int DefaultSortingOrder = 100;
    private const float DefaultSideWallVisibleFraction = 0.5f;
    private const float DefaultSideWallHeightScale = 1.05f;
    private const float DefaultCeilingTopBleedWorld = 0.1f;
    private const float DefaultCeilingTopCropFraction = 0.32f;
    private const float DefaultCeilingHorizontalBleedWorld = 0.1f;
    private const float DefaultColliderThicknessWorld = 0.5f;

    [SerializeField] private Sprite leftWallSprite;
    [SerializeField] private Sprite rightWallSprite;
    [SerializeField] private Sprite ceilingSprite;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private string sortingLayerName = DefaultSortingLayerName;
    [SerializeField] private int sortingOrder = DefaultSortingOrder;
    [SerializeField] private bool relayoutOnCameraChange = true;
    [SerializeField, Range(0f, 1f)] private float sideWallVisibleFraction = DefaultSideWallVisibleFraction;
    [SerializeField, Min(1f)] private float sideWallHeightScale = DefaultSideWallHeightScale;
    [SerializeField, Min(0f)] private float ceilingTopBleedWorld = DefaultCeilingTopBleedWorld;
    [SerializeField, Range(0f, 1f)] private float ceilingTopCropFraction = DefaultCeilingTopCropFraction;
    [SerializeField, Min(0f)] private float ceilingHorizontalBleedWorld = DefaultCeilingHorizontalBleedWorld;
    [SerializeField, Min(0.01f)] private float colliderThicknessWorld = DefaultColliderThicknessWorld;
    [SerializeField] private BoxCollider2D leftWallCollider;
    [SerializeField] private BoxCollider2D rightWallCollider;
    [SerializeField] private BoxCollider2D ceilingCollider;

    private SpriteRenderer leftWallRenderer;
    private SpriteRenderer rightWallRenderer;
    private SpriteRenderer ceilingRenderer;
    private Transform visualRoot;

    private void Start()
    {
        Rebuild();
    }

    private void LateUpdate()
    {
        if (this.relayoutOnCameraChange)
            ApplyLayout();
    }

    public void Configure(
        Sprite leftWallSprite,
        Sprite rightWallSprite,
        Sprite ceilingSprite,
        Camera targetCamera,
        string sortingLayerName = DefaultSortingLayerName,
        int sortingOrder = DefaultSortingOrder)
    {
        this.leftWallSprite = leftWallSprite;
        this.rightWallSprite = rightWallSprite;
        this.ceilingSprite = ceilingSprite;
        this.targetCamera = targetCamera;
        this.sortingLayerName = string.IsNullOrWhiteSpace(sortingLayerName)
            ? DefaultSortingLayerName
            : sortingLayerName;
        this.sortingOrder = sortingOrder;
    }

    public void Rebuild()
    {
        this.visualRoot = EnsureVisualRoot();
        ClearVisualChildren();

        this.leftWallRenderer = CreateVisual(LeftWallName, this.leftWallSprite, 1);
        this.rightWallRenderer = CreateVisual(RightWallName, this.rightWallSprite, 1);
        this.ceilingRenderer = CreateVisual(CeilingName, this.ceilingSprite);

        ApplyLayout();
    }

    public void ApplyLayout()
    {
        Camera camera = ResolveCamera();
        if (camera == null || !camera.orthographic)
            return;

        float worldHeight = camera.orthographicSize * 2f;
        float worldWidth = worldHeight * camera.aspect;
        Vector3 cameraCenter = camera.transform.position;

        float leftEdge = cameraCenter.x - worldWidth * 0.5f;
        float rightEdge = cameraCenter.x + worldWidth * 0.5f;
        float topEdge = cameraCenter.y + worldHeight * 0.5f;

        float visibleFraction = Mathf.Clamp01(this.sideWallVisibleFraction);
        float sideHeightScale = Mathf.Max(1f, this.sideWallHeightScale);
        float topBleed = Mathf.Max(0f, this.ceilingTopBleedWorld);
        float topCropFraction = Mathf.Clamp01(this.ceilingTopCropFraction);
        float horizontalBleed = Mathf.Max(0f, this.ceilingHorizontalBleedWorld);

        LayoutVerticalWall(this.leftWallRenderer, leftEdge, worldHeight, true, cameraCenter, visibleFraction, sideHeightScale);
        LayoutVerticalWall(this.rightWallRenderer, rightEdge, worldHeight, false, cameraCenter, visibleFraction, sideHeightScale);
        LayoutCeiling(this.ceilingRenderer, worldWidth, topEdge, cameraCenter, topBleed, topCropFraction, horizontalBleed);
        LayoutPhysicalColliders(leftEdge, rightEdge, topEdge, worldWidth, worldHeight, cameraCenter);
    }

    private SpriteRenderer CreateVisual(string objectName, Sprite sprite, int sortingOrderOffset = 0)
    {
        if (sprite == null)
            return null;

        var visual = new GameObject(objectName);
        visual.transform.SetParent(this.visualRoot, false);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = this.sortingLayerName;
        renderer.sortingOrder = this.sortingOrder + sortingOrderOffset;
        return renderer;
    }

    private static void LayoutVerticalWall(
        SpriteRenderer renderer,
        float edgeX,
        float worldHeight,
        bool attachToLeftEdge,
        Vector3 cameraCenter,
        float visibleFraction,
        float heightScaleMultiplier)
    {
        if (renderer == null || renderer.sprite == null || renderer.sprite.bounds.size.y <= 0f)
            return;

        float scale = worldHeight * heightScaleMultiplier / renderer.sprite.bounds.size.y;
        float width = renderer.sprite.bounds.size.x * scale;
        float x = attachToLeftEdge
            ? edgeX + width * (visibleFraction - 0.5f)
            : edgeX + width * (0.5f - visibleFraction);

        renderer.transform.localScale = Vector3.one * scale;
        renderer.transform.position = new Vector3(x, cameraCenter.y, renderer.transform.position.z);
    }

    private static void LayoutCeiling(
        SpriteRenderer renderer,
        float worldWidth,
        float topEdge,
        Vector3 cameraCenter,
        float topBleed,
        float topCropFraction,
        float horizontalBleed)
    {
        if (renderer == null || renderer.sprite == null || renderer.sprite.bounds.size.x <= 0f)
            return;

        float targetWidth = worldWidth + horizontalBleed * 2f;
        float scale = targetWidth / renderer.sprite.bounds.size.x;
        float height = renderer.sprite.bounds.size.y * scale;
        float cropBleed = Mathf.Max(topBleed, height * topCropFraction);

        renderer.transform.localScale = Vector3.one * scale;
        renderer.transform.position = new Vector3(cameraCenter.x, topEdge - height * 0.5f + cropBleed, renderer.transform.position.z);
    }

    private void LayoutPhysicalColliders(
        float leftEdge,
        float rightEdge,
        float topEdge,
        float worldWidth,
        float worldHeight,
        Vector3 cameraCenter)
    {
        float thickness = Mathf.Max(0.01f, this.colliderThicknessWorld);
        BoxCollider2D left = ResolveCollider(ref this.leftWallCollider, PhysicalLeftWallName);
        BoxCollider2D right = ResolveCollider(ref this.rightWallCollider, PhysicalRightWallName);
        BoxCollider2D ceiling = ResolveCollider(ref this.ceilingCollider, CeilingName);

        LayoutVerticalCollider(left, leftEdge, cameraCenter.y, worldHeight, thickness);
        LayoutVerticalCollider(right, rightEdge, cameraCenter.y, worldHeight, thickness);
        LayoutCeilingCollider(ceiling, cameraCenter.x, topEdge, worldWidth, thickness);
    }

    private BoxCollider2D ResolveCollider(ref BoxCollider2D collider, string childName)
    {
        if (collider != null)
            return collider;

        Transform child = this.transform.Find(childName);
        if (child == null)
            return null;

        collider = child.GetComponent<BoxCollider2D>();
        return collider;
    }

    private static void LayoutVerticalCollider(
        BoxCollider2D collider,
        float centerX,
        float centerY,
        float height,
        float thickness)
    {
        if (collider == null)
            return;

        ResetColliderShape(collider);
        collider.transform.position = new Vector3(centerX, centerY, collider.transform.position.z);
        collider.transform.localScale = new Vector3(thickness, height, ResolveScaleZ(collider.transform));
    }

    private static void LayoutCeilingCollider(
        BoxCollider2D collider,
        float centerX,
        float topEdge,
        float width,
        float thickness)
    {
        if (collider == null)
            return;

        ResetColliderShape(collider);
        collider.transform.position = new Vector3(centerX, topEdge + thickness * 0.5f, collider.transform.position.z);
        collider.transform.localScale = new Vector3(width, thickness, ResolveScaleZ(collider.transform));
    }

    private static void ResetColliderShape(BoxCollider2D collider)
    {
        collider.offset = Vector2.zero;
        collider.size = Vector2.one;
    }

    private static float ResolveScaleZ(Transform transform)
    {
        return Mathf.Approximately(transform.localScale.z, 0f) ? 1f : transform.localScale.z;
    }

    private Camera ResolveCamera()
    {
        if (this.targetCamera != null)
            return this.targetCamera;

        return Camera.main;
    }

    private Transform EnsureVisualRoot()
    {
        Transform existing = this.transform.Find(VisualContainerName);
        if (existing != null)
            return existing;

        var container = new GameObject(VisualContainerName);
        container.transform.SetParent(this.transform, false);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        return container.transform;
    }

    private void ClearVisualChildren()
    {
        if (this.visualRoot == null)
            return;

        for (int i = this.visualRoot.childCount - 1; i >= 0; --i)
        {
            Transform child = this.visualRoot.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
