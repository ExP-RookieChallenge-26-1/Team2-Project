using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUI : MonoBehaviour
{
    [SerializeField] private CardSelector cardSelector;
    [SerializeField] private CardSlotUI[] slots;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameManager gameManager;

    private List<CardData> currentCards = new List<CardData>();
    private int selectedIndex = -1;

    private void Start()
    {
        confirmButton.interactable = false;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    public void ShowCardsByIds(int[] ids, float[] weights)
    {
        List<CardData> selectedCards = cardSelector.Pick3ByIds(ids, weights);
        Show(selectedCards);
    }

    public void ShowCardsByNames(string[] names, float[] weights)
    {
        List<CardData> selectedCards = cardSelector.Pick3ByNames(names, weights);
        Show(selectedCards);
    }

    private void Show(List<CardData> cards)
    {
        currentCards = cards;
        selectedIndex = -1;
        confirmButton.interactable = false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < cards.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(cards[i], this, i);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectCard(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetSelected(i == selectedIndex);
        }

        confirmButton.interactable = true;
    }

    private void OnClickConfirm()
    {
        if (selectedIndex < 0 || selectedIndex >= currentCards.Count)
            return;

        CardData selectedCard = currentCards[selectedIndex];
        gameManager.ApplyUpgrade(selectedCard);
    }
}