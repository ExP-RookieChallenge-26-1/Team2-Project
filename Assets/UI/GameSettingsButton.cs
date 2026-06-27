using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Button))]
public sealed class GameSettingsButton : MonoBehaviour
{
    [SerializeField] private SettingPanel settingPanel;
    [SerializeField] private SettingPanel settingPanelPrefab;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveListener(OpenSettings);
        button.onClick.AddListener(OpenSettings);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OpenSettings);
    }

    private void OpenSettings()
    {
        SettingPanel panel = ResolveSettingPanel();
        if (panel == null)
            return;

        float resumeTimeScale = GetCurrentResumeTimeScale();
        Time.timeScale = 0f;
        panel.ShowForGameplayPause(resumeTimeScale);
    }

    public static float CalculateResumeTimeScale(GameStateMachine.State state, float currentTimeScale)
    {
        switch (state)
        {
            case GameStateMachine.State.Playing:
                return currentTimeScale > 0f ? 1f : 0f;
            case GameStateMachine.State.Enhancement:
                return currentTimeScale > 0f ? currentTimeScale : 1f;
            case GameStateMachine.State.GameOver:
                return 0f;
            default:
                return currentTimeScale > 0f ? currentTimeScale : 1f;
        }
    }

    private static float GetCurrentResumeTimeScale()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != null)
            return CalculateResumeTimeScale(GameManager.Instance.State.Current, Time.timeScale);

        return Time.timeScale > 0f ? Time.timeScale : 1f;
    }

    private SettingPanel ResolveSettingPanel()
    {
        if (settingPanel != null)
            return settingPanel;

        settingPanel = Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
        if (settingPanel != null)
            return settingPanel;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || settingPanelPrefab == null)
            return null;

        settingPanel = Instantiate(settingPanelPrefab, canvas.transform);
        settingPanel.name = settingPanelPrefab.name;
        settingPanel.gameObject.SetActive(false);
        return settingPanel;
    }
}
