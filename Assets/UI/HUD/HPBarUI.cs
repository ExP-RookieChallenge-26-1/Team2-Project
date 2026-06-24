using UnityEngine;

public class HPBarUI : MonoBehaviour
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

        var health = GameManager.Instance.User.Health;
        health.OnHpChanged += OnHpChanged;
        SetFill(health.CurrentHp / (float)health.MaxHp);
    }

    private void OnDestroy()
    {
        var health = GameManager.Instance?.User?.Health;
        if (health != null)
            health.OnHpChanged -= OnHpChanged;
    }

    private void OnHpChanged(int current, int max)
    {
        SetFill(current / (float)max);
    }

    // 왼쪽 고정, 오른쪽이 줄어듦
    // 스프라이트 피벗이 가운데이므로 스케일 줄인 만큼 왼쪽으로 위치 보정
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
