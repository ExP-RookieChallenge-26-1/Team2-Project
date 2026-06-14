using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button creditButton;

    [SerializeField] private SettingPanel settingPanel;
    [SerializeField] private CreditPanel creditPanel;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingButton.onClick.AddListener(OnSettingClicked);
        creditButton.onClick.AddListener(OnCreditClicked);
    }

    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
            Object.Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("GameScene");
    }

    private void OnSettingClicked()
    {
        HideTitleUI();
        settingPanel.gameObject.SetActive(true);
    }

    private void OnCreditClicked()
    {
        HideTitleUI();
        creditPanel.gameObject.SetActive(true);
    }

    public void ShowTitleUI()
    {
        gameObject.SetActive(true);
    }

    public void HideTitleUI()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        settingButton.onClick.RemoveListener(OnSettingClicked);
        creditButton.onClick.RemoveListener(OnCreditClicked);
    }
}
