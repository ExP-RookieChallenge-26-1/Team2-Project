using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUI : MonoBehaviour
{
	[SerializeField] private CardSelector cardSelector;
	[SerializeField] private CardSlotUI[] slots;
	[SerializeField] private Button confirmButton;

	private List<CardData> currentCards;
	private int selectedIndex;

	private void Awake()
	{
		this.currentCards = new List<CardData>();
		this.selectedIndex = -1;
	}

	private void Start()
	{
		this.confirmButton.interactable = false;
		this.confirmButton.onClick.RemoveAllListeners();
		this.confirmButton.onClick.AddListener(OnClickConfirm);
		GameManager.Instance.State.OnChanged += OnGameStateChanged;
	}

	private void OnDestroy()
	{
		GameManager.Instance.State.OnChanged -= OnGameStateChanged;	
	}

	public void ShowCardsByIds(int[] ids, float[] weights)
	{
		List<CardData> selectedCards;
		selectedCards = this.cardSelector.Pick3ByIds(ids, weights);
		Show(selectedCards);
	}

	public void ShowCardsByNames(string[] names, float[] weights)
	{
		List<CardData> selectedCards;
		selectedCards = this.cardSelector.Pick3ByNames(names, weights);
		Show(selectedCards);
	}

	private void Show(List<CardData> cards)
	{
		this.currentCards = cards;
		this.selectedIndex = -1;
		this.confirmButton.interactable = false;

		for (int i = 0; i < this.slots.Length; ++i)
		{
			if (i < cards.Count)
			{
				this.slots[i].gameObject.SetActive(true);
				this.slots[i].Setup(cards[i], this, i);
			}
			else
				this.slots[i].gameObject.SetActive(false);
		}
	}

	public void SelectCard(int index)
	{
		this.selectedIndex = index;

		for (int i = 0; i < this.slots.Length; ++i)
			this.slots[i].SetSelected(i == this.selectedIndex);

		this.confirmButton.interactable = true;
	}

	private void OnClickConfirm()
	{
		if (this.selectedIndex < 0 || this.selectedIndex >= this.currentCards.Count)
			return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUpgradeSelectSound();

        this.currentCards[this.selectedIndex].Apply();
		GameManager.Instance.State.Change(GameStateMachine.State.Playing);
	}

	private void OnGameStateChanged(GameStateMachine.State newState)
	{
		if (newState == GameStateMachine.State.Playing)
			this.gameObject.SetActive(false);
	}
}