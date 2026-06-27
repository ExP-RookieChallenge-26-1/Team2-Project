using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class TilemapAspectCropper : MonoBehaviour
{
    private const float DefaultReferenceAspect = 0.6f;
    private const string DefaultTilemapChildName = "Tilemap";

    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(0.01f)] private float referenceAspect = DefaultReferenceAspect;
    [SerializeField] private string tilemapChildName = DefaultTilemapChildName;
    [SerializeField] private Transform targetTilemap;
    [SerializeField] private bool relayoutOnCameraChange = true;

    private Vector3 originalTilemapLocalScale = Vector3.one;
    private bool hasOriginalTilemapScale;

    public float ReferenceAspect => this.referenceAspect;

    private void Awake()
    {
        ApplyCrop();
    }

    private void OnEnable()
    {
        ApplyCrop();
    }

    private void LateUpdate()
    {
        if (this.relayoutOnCameraChange)
            ApplyCrop();
    }

    private void OnValidate()
    {
        this.referenceAspect = Mathf.Max(0.01f, this.referenceAspect);
    }

    public void Configure(Camera camera, float referenceAspect = DefaultReferenceAspect)
    {
        this.targetCamera = camera;
        this.referenceAspect = Mathf.Max(0.01f, referenceAspect);
        ApplyCrop();
    }

    public void ApplyCrop()
    {
        Transform tilemap = ResolveTilemap();
        Camera camera = ResolveCamera();
        if (tilemap == null || camera == null)
            return;

        CacheOriginalTilemapScale(tilemap);

        float scale = CalculateTilemapCoverScale(this.referenceAspect, camera.aspect);
        tilemap.localScale = new Vector3(
            this.originalTilemapLocalScale.x * scale,
            this.originalTilemapLocalScale.y * scale,
            this.originalTilemapLocalScale.z);
    }

    public static float CalculateTilemapCoverScale(float referenceAspect, float currentAspect)
    {
        float safeReferenceAspect = Mathf.Max(0.01f, referenceAspect);
        float safeCurrentAspect = Mathf.Max(0.01f, currentAspect);
        return Mathf.Max(1f, safeCurrentAspect / safeReferenceAspect);
    }

    private Transform ResolveTilemap()
    {
        if (this.targetTilemap != null)
            return this.targetTilemap;

        if (!string.IsNullOrWhiteSpace(this.tilemapChildName))
            this.targetTilemap = transform.Find(this.tilemapChildName);

        if (this.targetTilemap != null)
            return this.targetTilemap;

        Tilemap tilemap = GetComponentInChildren<Tilemap>(true);
        if (tilemap != null)
            this.targetTilemap = tilemap.transform;

        return this.targetTilemap;
    }

    private Camera ResolveCamera()
    {
        return this.targetCamera != null ? this.targetCamera : Camera.main;
    }

    private void CacheOriginalTilemapScale(Transform tilemap)
    {
        if (this.hasOriginalTilemapScale)
            return;

        this.originalTilemapLocalScale = tilemap.localScale;
        this.hasOriginalTilemapScale = true;
    }
}
