using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private int maxHandSize = 5;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private HandVisuals handVisuals;

    private List<Card> cardsInHand = new List<Card>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void DrawCardsToMax(int maxSize)
    {
        int attempts = 0;
        int maxAttempts = 100;

        while (cardsInHand.Count < maxSize && attempts < maxAttempts)
        {
            if (DeckManager.Instance == null) break;

            CardData nextCard = DeckManager.Instance.DrawCard();
            if (nextCard == null)
            {
                Debug.Log("Колода пуста!");
                break;
            }

            DrawCard(nextCard);
            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogError("Возможно бесконечный цикл в DrawCardsToMax!");
    }

    public void DrawCard(CardData cardData)
    {
        GameObject cardGO = Instantiate(cardPrefab, handVisuals.transform);
        Card card = cardGO.GetComponent<Card>();
        card.Setup(cardData);
        card.currentLocation = Card.CardLocation.InHand;

        cardsInHand.Add(card);
        handVisuals.AddCard(card);
    }

    public void RemoveCardFromHand(Card card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            handVisuals.RemoveCard(card);
        }
    }

    public void ShowHand(bool show)
    {
        handVisuals.ShowHand(show);
    }

    public int GetHandSize()
    {
        return cardsInHand.Count;
    }

    public bool HasSpace()
    {
        return cardsInHand.Count < maxHandSize;
    }
}