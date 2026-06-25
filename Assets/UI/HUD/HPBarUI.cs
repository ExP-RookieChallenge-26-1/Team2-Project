using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

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
        this.fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}
