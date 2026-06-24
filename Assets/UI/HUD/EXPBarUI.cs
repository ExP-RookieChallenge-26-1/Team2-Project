using UnityEngine;

public class EXPBarUI : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalPos;
    private Vector3 initialLocalScale;
    private float spriteHalfWidth;

    private void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.initialLocalPos = transform.localPosition;
        this.initialLocalScale = transform.localScale;
    }

    private void Start()
    {
        this.spriteHalfWidth = this.spriteRenderer.sprite.bounds.extents.x;

        var level = GameManager.Instance.User.Level;
        level.OnExpChanged += OnExpChanged;
        SetFill(level.CurrentExp / (float)level.RequiredExp);
    }

    private void OnDestroy()
    {
        var level = GameManager.Instance?.User?.Level;
        if (level != null)
            level.OnExpChanged -= OnExpChanged;
    }

    private void OnExpChanged(int current, int max)
    {
        SetFill(current / (float)max);
    }

    // 왼쪽 고정, 오른쪽이 줄어듦
    private void SetFill(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        float newScaleX = this.initialLocalScale.x * ratio;
        float offset = this.spriteHalfWidth * this.initialLocalScale.x * (1f - ratio);

        transform.localScale = new Vector3(newScaleX, this.initialLocalScale.y, this.initialLocalScale.z);
        transform.localPosition = new Vector3(
            this.initialLocalPos.x - offset,
            this.initialLocalPos.y,
            this.initialLocalPos.z);
    }
}
