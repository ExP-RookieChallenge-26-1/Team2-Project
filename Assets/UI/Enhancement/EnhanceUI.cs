using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUI : MonoBehaviour
{
	private const string ConfirmButtonNormalSpritePath = "UI/CardConfirmButtonNormal";
	private const string ConfirmButtonPressedSpritePath = "UI/CardConfirmButtonPressed";

	[SerializeField] private CardSelector cardSelector;
	[SerializeField] private CardSlotUI[] slots;
	[SerializeField] private Button confirmButton;

	private List<CardData> currentCards;
	private CardUseContext currentContext;
	private int selectedIndex;

	private void Awake()
	{
		this.currentCards = new List<CardData>();
		this.currentContext = CardUseContext.None;
		this.selectedIndex = -1;
		ConfigureConfirmButtonVisual();
	}

	private void Start()
	{
		ConfigureConfirmButtonVisual();
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
		ShowCardsByIds(ids, weights, CardUseContext.None);
	}

	public void ShowCardsByIds(int[] ids, float[] weights, CardUseContext context)
	{
		List<CardData> selectedCards;
		selectedCards = this.cardSelector.Pick3ByIds(ids, weights);
		Show(selectedCards, context);
	}

	public void ShowCardsByNames(string[] names, float[] weights)
	{
		ShowCardsByNames(names, weights, CardUseContext.None);
	}

	public void ShowCardsByNames(string[] names, float[] weights, CardUseContext context)
	{
		List<CardData> selectedCards;
		selectedCards = this.cardSelector.Pick3ByNames(names, weights);
		Show(selectedCards, context);
	}

	private void Show(List<CardData> cards, CardUseContext context)
	{
		this.currentCards = cards;
		this.currentContext = context;
		this.selectedIndex = -1;
		ConfigureConfirmButtonVisual();
		this.confirmButton.interactable = false;

		for (int i = 0; i < this.slots.Length; ++i)
		{
			if (i < cards.Count)
			{
				this.slots[i].gameObject.SetActive(true);
				this.slots[i].Setup(cards[i], this, i, context);
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

        this.currentCards[this.selectedIndex].Apply(this.currentContext);
		GameManager.Instance.State.Change(GameStateMachine.State.Playing);
	}

	private void OnGameStateChanged(GameStateMachine.State newState)
	{
		if (newState == GameStateMachine.State.Playing)
			this.gameObject.SetActive(false);
	}

	private void ConfigureConfirmButtonVisual()
	{
		if (this.confirmButton == null)
			return;

		Sprite normalSprite = Resources.Load<Sprite>(ConfirmButtonNormalSpritePath);
		Sprite pressedSprite = Resources.Load<Sprite>(ConfirmButtonPressedSpritePath);

		if (this.confirmButton.image != null)
		{
			if (normalSprite != null)
				this.confirmButton.image.sprite = normalSprite;

			this.confirmButton.image.color = Color.white;
			this.confirmButton.image.preserveAspect = true;
		}

		ColorBlock colors = this.confirmButton.colors;
		colors.normalColor = Color.white;
		colors.highlightedColor = Color.white;
		colors.pressedColor = Color.white;
		colors.selectedColor = Color.white;
		colors.disabledColor = Color.white;
		this.confirmButton.colors = colors;

		this.confirmButton.transition = Selectable.Transition.SpriteSwap;
		SpriteState spriteState = this.confirmButton.spriteState;
		spriteState.highlightedSprite = null;
		spriteState.pressedSprite = pressedSprite;
		spriteState.selectedSprite = null;
		spriteState.disabledSprite = null;
		this.confirmButton.spriteState = spriteState;
	}
}
