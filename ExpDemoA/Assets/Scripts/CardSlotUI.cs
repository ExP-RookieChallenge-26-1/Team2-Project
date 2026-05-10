using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rectTransform;

    private CardData currentCard;
    private EnhanceUI parentUI;
    private int slotIndex;
    private Vector2 originalPos;

    private void Awake()
    {
        originalPos = rectTransform.anchoredPosition;
    }

    public void Setup(CardData card, EnhanceUI ui, int index)
    {
        currentCard = card;
        parentUI = ui;
        slotIndex = index;

        titleText.text = card.cardName;
        descriptionText.text = card.description;

        icon.sprite = card.icon;
        icon.enabled = card.icon != null;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickCard);

        SetSelected(false);
    }

    private void OnClickCard()
    {
        parentUI.SelectCard(slotIndex);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            rectTransform.anchoredPosition = originalPos + new Vector2(0f, 20f);
        }
        else
        {
            rectTransform.anchoredPosition = originalPos;
        }
    }

    public CardData GetCard()
    {
        return currentCard;
    }
}