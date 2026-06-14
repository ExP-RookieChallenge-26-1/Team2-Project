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
    [SerializeField] private TitleUI titleUI;

    private void Start()
    {
        InitializeSliders();

        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void InitializeSliders()
    {
        if (AudioManager.Instance != null)
        {
            masterSlider.value = AudioManager.Instance.GetMasterVolume();
            bgmSlider.value = AudioManager.Instance.GetBGMVolume();
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        }
        else
        {
            masterSlider.value = 1f;
            bgmSlider.value = 1f;
            sfxSlider.value = 1f;
        }

        UpdateValueText(masterValueText, masterSlider.value);
        UpdateValueText(bgmValueText, bgmSlider.value);
        UpdateValueText(sfxValueText, sfxSlider.value);
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
        titleUI.ShowTitleUI();
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        backButton.onClick.RemoveListener(OnBackClicked);
    }
}
