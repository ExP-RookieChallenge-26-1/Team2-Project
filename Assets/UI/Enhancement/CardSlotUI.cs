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
	private Vector2 origitnalPosition;

	private void Awake()
	{
		this.origitnalPosition = this.rectTransform.anchoredPosition;
	}

	public void Setup(CardData card, EnhanceUI ui, int index)
	{
		this.currentCard = card;
		this.parentUI = ui;
		this.slotIndex = index;

		this.titleText.text = card.CardName;
		this.descriptionText.text = card.Description;
		this.icon.sprite = card.Icon;
		this.icon.enabled = card.Icon != null;

		this.button.onClick.RemoveAllListeners();
		this.button.onClick.AddListener(OnClickCard);

		SetSelected(false);
	}

	private void OnClickCard()
	{
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUpgradeSelectSound();

        this.parentUI.SelectCard(this.slotIndex);
	}
	
	public void SetSelected(bool selected)
	{
		this.rectTransform.anchoredPosition = selected ? this.origitnalPosition + new Vector2(0f, 20f) : this.origitnalPosition;
	}

	public CardData GetCard()
	{
		return this.currentCard;
	}
}