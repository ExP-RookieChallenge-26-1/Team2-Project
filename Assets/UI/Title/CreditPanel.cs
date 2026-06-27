using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CreditPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private Button skipButton;
    [SerializeField] private TitleUI titleUI;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float delayBeforeScroll = 1f;
    [SerializeField] private float delayAfterScroll = 2f;

    private RectTransform rectTransform;
    private Coroutine scrollCoroutine;
    private GameObject returnPanel;

    private void Awake()
    {
        ResolveOptionalReferences();
    }

    private void OnEnable()
    {
        ResolveOptionalReferences();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        if (rectTransform == null)
            return;

        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);

        if (scrollSpeed <= 0f)
            return;

        rectTransform.anchoredPosition = new Vector2(0, -800f);
        scrollCoroutine = StartCoroutine(ScrollCredits());
    }

    private void OnDisable()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }

        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnSkipClicked);
    }

    public void ShowFrom(GameObject panelToReturnTo)
    {
        returnPanel = panelToReturnTo;
        gameObject.SetActive(true);
    }

    private IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(delayBeforeScroll);

        while (rectTransform.anchoredPosition.y < 800f)
        {
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(delayAfterScroll);
        CloseCreditPanel();
    }

    private void OnSkipClicked()
    {
        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);
        CloseCreditPanel();
    }

    private void CloseCreditPanel()
    {
        gameObject.SetActive(false);

        if (returnPanel != null)
        {
            returnPanel.SetActive(true);
            returnPanel = null;
            return;
        }

        if (titleUI != null)
            titleUI.ShowTitleUI();
    }

    private void ResolveOptionalReferences()
    {
        if (creditText == null)
            creditText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (creditText != null)
            rectTransform = creditText.GetComponent<RectTransform>();

        if (skipButton == null)
        {
            Transform skipTransform = transform.Find("SkipButton");
            if (skipTransform != null)
                skipButton = skipTransform.GetComponent<Button>();
        }
    }
}
