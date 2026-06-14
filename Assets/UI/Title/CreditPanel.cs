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

    private void Start()
    {
        rectTransform = creditText.GetComponent<RectTransform>();
        skipButton.onClick.AddListener(OnSkipClicked);

        rectTransform.anchoredPosition = new Vector2(0, -800f);
        scrollCoroutine = StartCoroutine(ScrollCredits());
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
        titleUI.ShowTitleUI();
    }

    private void OnDestroy()
    {
        skipButton.onClick.RemoveListener(OnSkipClicked);
    }
}
