using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private const string BackgroundSortingLayerName = "Background";
    private const float DefaultWorldHeight = 10f;
    private const float DefaultStageDurationSeconds = 180f;

    [Serializable]
    public class Layer
    {
        [Tooltip("Sprite used by this parallax layer.")]
        [SerializeField] private Sprite sprite;

        [Min(0f)]
        [InspectorName("Move Amount (Speed)")]
        [Tooltip("Total downward movement over Stage Duration. Larger values move faster.")]
        [SerializeField] private float travelDistance = 1f;

        [Tooltip("Rendering order within the Background sorting layer. Higher values render in front.")]
        [SerializeField] private int sortingOrder;

        [NonSerialized] private Transform visual;
        [NonSerialized] private Vector3 startLocalPosition;
        [NonSerialized] private float spriteWorldHeight;

        public Sprite Sprite => this.sprite;
        public float TravelDistance => this.travelDistance;
        public int SortingOrder => this.sortingOrder;
        public Transform Visual => this.visual;
        public Vector3 StartLocalPosition => this.startLocalPosition;
        public float SpriteWorldHeight => this.spriteWorldHeight;

        public void Configure(Sprite sprite, float travelDistance, int sortingOrder)
        {
            this.sprite = sprite;
            this.travelDistance = travelDistance;
            this.sortingOrder = sortingOrder;
        }

        public void SetRuntimeObject(Transform visual, Vector3 startLocalPosition, float spriteWorldHeight)
        {
            this.visual = visual;
            this.startLocalPosition = startLocalPosition;
            this.spriteWorldHeight = spriteWorldHeight;
        }
    }

    [SerializeField] private Layer[] layers = Array.Empty<Layer>();

    [Min(0.01f)]
    [Tooltip("Seconds it takes for every layer to complete its one downward movement.")]
    [SerializeField] private float stageDurationSeconds = DefaultStageDurationSeconds;

    [Min(0.01f)]
    [Tooltip("World height used when no orthographic main camera is available, mostly for tests.")]
    [SerializeField] private float fallbackWorldHeight = DefaultWorldHeight;

    [Tooltip("Rebuilds sprite renderer children when play mode starts.")]
    [SerializeField] private bool rebuildOnStart = true;

    private float elapsedSeconds;

    public Layer[] Layers => this.layers;
    public float StageDurationSeconds => this.stageDurationSeconds;

    private void Start()
    {
        if (this.rebuildOnStart)
            Rebuild();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Configure(
        Layer[] layers,
        float stageDurationSeconds = DefaultStageDurationSeconds,
        float fallbackWorldHeight = DefaultWorldHeight)
    {
        this.layers = layers ?? Array.Empty<Layer>();
        this.stageDurationSeconds = stageDurationSeconds;
        this.fallbackWorldHeight = fallbackWorldHeight;
    }

    public void Rebuild()
    {
        ClearRuntimeChildren();
        this.elapsedSeconds = 0f;

        Vector2 worldSize = ResolveWorldSize();
        for (int i = 0; i < this.layers.Length; ++i)
        {
            BuildLayer(this.layers[i], i, worldSize);
        }

        ApplyProgress(0f);
    }

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        this.elapsedSeconds += deltaTime;
        ApplyProgress(CalculateStageProgress(this.elapsedSeconds));
    }

    public float CalculateStageProgress(float elapsedSeconds)
    {
        if (this.stageDurationSeconds <= 0f)
            return 1f;

        return Mathf.Clamp01(elapsedSeconds / this.stageDurationSeconds);
    }

    public Vector3 CalculateLayerOffset(float progress, Layer layer)
    {
        if (layer == null)
            return Vector3.zero;

        return Vector3.down * Mathf.Max(0f, layer.TravelDistance) * Mathf.Clamp01(progress);
    }

    private void ApplyProgress(float progress)
    {
        for (int i = 0; i < this.layers.Length; ++i)
        {
            ApplyLayerProgress(this.layers[i], progress);
        }
    }

    private void ApplyLayerProgress(Layer layer, float progress)
    {
        if (layer == null || layer.Visual == null)
            return;

        layer.Visual.localPosition = layer.StartLocalPosition + CalculateLayerOffset(progress, layer);
    }

    private void BuildLayer(Layer layer, int index, Vector2 worldSize)
    {
        if (layer == null || layer.Sprite == null)
        {
            Debug.LogWarning($"ParallaxBackground layer {index} has no sprite.", this);
            return;
        }

        float spriteWidth = layer.Sprite.bounds.size.x;
        float spriteHeight = layer.Sprite.bounds.size.y;
        if (spriteWidth <= 0f || spriteHeight <= 0f)
        {
            Debug.LogWarning($"ParallaxBackground layer {index} sprite has invalid bounds.", this);
            return;
        }

        float scale = Mathf.Max(worldSize.x / spriteWidth, worldSize.y / spriteHeight);
        float spriteWorldHeight = spriteHeight * scale;
        Transform visual = CreateLayerVisual(layer, index, Vector3.zero, scale);

        layer.SetRuntimeObject(visual, visual.localPosition, spriteWorldHeight);
    }

    private Transform CreateLayerVisual(Layer layer, int layerIndex, Vector3 localPosition, float scale)
    {
        GameObject visual = new GameObject($"Layer {layerIndex + 1}");
        visual.transform.SetParent(this.transform, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = layer.Sprite;
        renderer.sortingLayerName = BackgroundSortingLayerName;
        renderer.sortingOrder = layer.SortingOrder;

        return visual.transform;
    }

    private Vector2 ResolveWorldSize()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.orthographic)
        {
            float worldHeight = camera.orthographicSize * 2f;
            float worldWidth = worldHeight * Mathf.Max(camera.aspect, 0.01f);
            return new Vector2(worldWidth, worldHeight);
        }

        return new Vector2(this.fallbackWorldHeight, this.fallbackWorldHeight);
    }

    private void ClearRuntimeChildren()
    {
        for (int i = this.transform.childCount - 1; i >= 0; --i)
        {
            Transform child = this.transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
