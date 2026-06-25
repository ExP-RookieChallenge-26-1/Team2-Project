using UnityEngine;
using UnityEngine.UI;

public class EXPBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private void Start()
    {
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

    private void SetFill(float ratio)
    {
        this.fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}
