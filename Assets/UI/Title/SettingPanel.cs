using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    [SerializeField] private Button backButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private TitleUI titleUI;
    [SerializeField] private CreditPanel creditPanel;

    private bool resumeTimeOnClose;
    private float resumeTimeScale = 1f;

    private void Awake()
    {
        ResolveOptionalReferences();
    }

    private void OnEnable()
    {
        ResolveOptionalReferences();
    }

    private void Start()
    {
        InitializeSliders();

        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        if (creditButton != null)
            creditButton.onClick.AddListener(OnCreditClicked);
    }

    private void InitializeSliders()
    {
        if (masterSlider != null)
        {
            masterSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetMasterVolume() : 1f;
            UpdateValueText(masterValueText, masterSlider.value);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetBGMVolume() : 1f;
            UpdateValueText(bgmValueText, bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
            UpdateValueText(sfxValueText, sfxSlider.value);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
        UpdateValueText(masterValueText, value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
        UpdateValueText(bgmValueText, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        UpdateValueText(sfxValueText, value);
    }

    private void UpdateValueText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100).ToString() + "%";
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
        ResumeGameplayTimeIfNeeded();

        if (titleUI != null)
            titleUI.ShowTitleUI();
    }

    private void OnCreditClicked()
    {
        if (creditPanel == null)
            return;

        gameObject.SetActive(false);
        creditPanel.ShowFrom(gameObject);
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
        if (creditButton != null)
            creditButton.onClick.RemoveListener(OnCreditClicked);
    }

    private void ResolveOptionalReferences()
    {
        if (masterSlider == null)
            masterSlider = FindChildComponent<Slider>("MasterGroup/MasterSlider");
        if (bgmSlider == null)
            bgmSlider = FindChildComponent<Slider>("BGMGroup/BGMSlider");
        if (sfxSlider == null)
            sfxSlider = FindChildComponent<Slider>("SFXGroup/SFXSlider");

        if (masterValueText == null)
            masterValueText = FindChildComponent<TextMeshProUGUI>("MasterGroup/MasterValueText");
        if (bgmValueText == null)
            bgmValueText = FindChildComponent<TextMeshProUGUI>("BGMGroup/BGMValueText");
        if (sfxValueText == null)
            sfxValueText = FindChildComponent<TextMeshProUGUI>("SFXGroup/SFXValueText");

        if (backButton == null)
            backButton = FindChildComponent<Button>("BackButton");

        if (creditButton == null)
        {
            Transform creditTransform = transform.Find("CreditButton");
            if (creditTransform != null)
                creditButton = creditTransform.GetComponent<Button>();
        }

        if (creditPanel == null)
            creditPanel = FindFirstObjectByType<CreditPanel>(FindObjectsInactive.Include);
    }

    public void ShowForGameplayPause(float resumeScale)
    {
        resumeTimeOnClose = true;
        resumeTimeScale = Mathf.Max(0f, resumeScale);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void ResumeGameplayTimeIfNeeded()
    {
        if (!resumeTimeOnClose)
            return;

        if (Time.timeScale <= 0f)
            Time.timeScale = resumeTimeScale;

        resumeTimeOnClose = false;
        resumeTimeScale = 1f;
    }

    private T FindChildComponent<T>(string path) where T : Component
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<T>() : null;
    }
}
