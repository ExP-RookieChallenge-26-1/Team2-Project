using UnityEngine;

[DisallowMultipleComponent]
public class MonsterHealthBar : MonoBehaviour
{
    public enum VerticalAnchor
    {
        Top,
        Bottom
    }

    [SerializeField] private Vector2 size = new Vector2(1.15f, 0.2f);
    [SerializeField] private float verticalOffset = 0.18f;
    [SerializeField] private VerticalAnchor verticalAnchor = VerticalAnchor.Top;
    [SerializeField] private int sortingOrderOffset = 1;
    [SerializeField] private string bodySpriteResourcePath = "UI/MonsterHealthBar/MonsterHealthBarBody";
    [SerializeField] private Sprite bodySprite;
    [SerializeField] private Vector2 fillAreaRatio = new Vector2(0.82f, 0.42f);
    [SerializeField] private Vector2 fillAreaOffsetRatio = Vector2.zero;
    [SerializeField] private Color fillColor = new Color(1f, 0.24f, 0.43f, 0.95f);
    [SerializeField] private bool hideAtZeroHealth = true;

    private static Sprite whiteSprite;

    private SpriteRenderer targetRenderer;
    private Transform barRoot;
    private SpriteRenderer bodyRenderer;
    private SpriteRenderer fillRenderer;
    private int currentHp;
    private int maxHp = 1;
    private bool hasHealthValue;

    private void Awake()
    {
        this.targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (!this.hasHealthValue || this.barRoot == null || !this.barRoot.gameObject.activeSelf)
            return;

        FollowTarget();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        if (this.barRoot != null)
            Destroy(this.barRoot.gameObject);
    }

    public void Configure(Vector2 newSize, float newVerticalOffset, VerticalAnchor newVerticalAnchor = VerticalAnchor.Top)
    {
        this.size = new Vector2(Mathf.Max(0.01f, newSize.x), Mathf.Max(0.01f, newSize.y));
        this.verticalOffset = newVerticalOffset;
        this.verticalAnchor = newVerticalAnchor;

        ApplyFill();
    }

    public void SetHealth(int current, int max)
    {
        this.maxHp = Mathf.Max(1, max);
        this.currentHp = Mathf.Clamp(current, 0, this.maxHp);
        this.hasHealthValue = true;

        EnsureBar();
        ApplyFill();

        if (this.currentHp <= 0 && this.hideAtZeroHealth)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        FollowTarget();
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetHideAtZeroHealth(bool shouldHide)
    {
        this.hideAtZeroHealth = shouldHide;
    }

    private void EnsureBar()
    {
        if (this.barRoot != null)
            return;

        GameObject root = new GameObject($"{gameObject.name}_HealthBar");
        this.barRoot = root.transform;

        this.bodyRenderer = CreateRenderer("Body", Color.white, 0);
        this.fillRenderer = CreateRenderer("Fill", this.fillColor, 1);

        ApplySorting();
    }

    private SpriteRenderer CreateRenderer(string objectName, Color color, int sortingOffset)
    {
        GameObject part = new GameObject(objectName);
        part.transform.SetParent(this.barRoot, false);

        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOffset;
        return renderer;
    }

    private void ApplyFill()
    {
        if (this.bodyRenderer == null || this.fillRenderer == null)
            return;

        float ratio = this.maxHp <= 0 ? 0f : this.currentHp / (float)this.maxHp;
        float fillAreaWidth = this.size.x * Mathf.Clamp01(this.fillAreaRatio.x);
        float fillAreaHeight = this.size.y * Mathf.Clamp01(this.fillAreaRatio.y);
        float fillWidth = fillAreaWidth * Mathf.Clamp01(ratio);

        ApplyBody();

        this.fillRenderer.transform.localScale = new Vector3(fillWidth, fillAreaHeight, 1f);
        this.fillRenderer.transform.localPosition = new Vector3(
            -fillAreaWidth * 0.5f + fillWidth * 0.5f + this.size.x * this.fillAreaOffsetRatio.x,
            this.size.y * this.fillAreaOffsetRatio.y,
            -0.01f);
    }

    private void ApplyBody()
    {
        Sprite sprite = GetBodySprite();

        if (sprite != null)
        {
            this.bodyRenderer.sprite = sprite;
            this.bodyRenderer.color = Color.white;
            Vector2 spriteSize = sprite.bounds.size;
            this.bodyRenderer.transform.localScale = new Vector3(
                this.size.x / spriteSize.x,
                this.size.y / spriteSize.y,
                1f);
        }
        else
        {
            this.bodyRenderer.sprite = GetWhiteSprite();
            this.bodyRenderer.color = new Color(0.32f, 0.12f, 0.08f, 0.95f);
            this.bodyRenderer.transform.localScale = new Vector3(this.size.x, this.size.y, 1f);
        }

        this.bodyRenderer.transform.localPosition = Vector3.zero;
    }

    private void FollowTarget()
    {
        if (this.barRoot == null)
            return;

        if (this.targetRenderer == null)
            this.targetRenderer = GetComponent<SpriteRenderer>();

        Bounds bounds = this.targetRenderer != null
            ? this.targetRenderer.bounds
            : new Bounds(transform.position, Vector3.one);

        float y = this.verticalAnchor == VerticalAnchor.Bottom
            ? bounds.min.y - this.verticalOffset - this.size.y * 0.5f
            : bounds.max.y + this.verticalOffset;

        this.barRoot.position = new Vector3(bounds.center.x, y, transform.position.z);
        this.barRoot.rotation = Quaternion.identity;
        this.barRoot.localScale = Vector3.one;

        ApplySorting();
    }

    private void ApplySorting()
    {
        if (this.bodyRenderer == null || this.fillRenderer == null)
            return;

        if (this.targetRenderer == null)
            this.targetRenderer = GetComponent<SpriteRenderer>();

        if (this.targetRenderer == null)
            return;

        this.bodyRenderer.sortingLayerID = this.targetRenderer.sortingLayerID;
        this.fillRenderer.sortingLayerID = this.targetRenderer.sortingLayerID;
        this.bodyRenderer.sortingOrder = this.targetRenderer.sortingOrder + this.sortingOrderOffset;
        this.fillRenderer.sortingOrder = this.bodyRenderer.sortingOrder + 1;
    }

    private void SetVisible(bool isVisible)
    {
        if (this.barRoot != null)
            this.barRoot.gameObject.SetActive(isVisible);
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null)
            return whiteSprite;

        Texture2D texture = new Texture2D(1, 1)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        whiteSprite.name = "RuntimeWhiteSprite";
        return whiteSprite;
    }

    private Sprite GetBodySprite()
    {
        if (this.bodySprite != null)
            return this.bodySprite;

        if (string.IsNullOrEmpty(this.bodySpriteResourcePath))
            return null;

        this.bodySprite = Resources.Load<Sprite>(this.bodySpriteResourcePath);
        return this.bodySprite;
    }
}
