using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        ConfigureFillImage();
    }

    private void Start()
    {
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

    private void SetFill(float ratio)
    {
        if (this.fillImage == null)
            return;

        ConfigureFillImage();
        this.fillImage.fillAmount = Mathf.Clamp01(ratio);
    }

    private void ConfigureFillImage()
    {
        if (this.fillImage == null)
            return;

        this.fillImage.type = Image.Type.Filled;
        this.fillImage.fillMethod = Image.FillMethod.Horizontal;
        this.fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
    }
}
