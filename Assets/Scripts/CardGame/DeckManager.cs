using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deckText;
    public static DeckManager Instance { get; private set; }
    [SerializeField] private List<CardData> deckCards = new List<CardData>();
    private Queue<CardData> drawPile = new Queue<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeDeck(List<InventoryManager.DeckCard> playerDeckCards)
    {
        deckCards.Clear();

        foreach (var deckCard in playerDeckCards)
        {
            CardData data = InventoryManager.Instance.GetCardData(deckCard.cardId);
            if (data != null)
                this.deckCards.Add(data);
        }

        ShuffleDeck();
        LogDeckStatus("Колода инициализирована");
    }

    private void ShuffleDeck()
    {
        List<CardData> temp = new List<CardData>(deckCards);
        drawPile.Clear();

        while (temp.Count > 0)
        {
            int randomIndex = Random.Range(0, temp.Count);
            drawPile.Enqueue(temp[randomIndex]);
            temp.RemoveAt(randomIndex);
        }

        LogDeckStatus("Колода перемешана");
    }

    public CardData DrawCard()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                // Перемешиваем сброс в новую колоду
                deckCards = new List<CardData>(discardPile);
                discardPile.Clear();
                ShuffleDeck();
                LogDeckStatus("Колода перемешана из сброса");
            }
            else
            {
                Debug.Log("❌ Колода пуста!");
                return null;
            }
        }

        CardData drawnCard = drawPile.Dequeue();
        LogDeckStatus($"Карта '{drawnCard.cardName}' взята. Осталось в колоде: {drawPile.Count}");

        return drawnCard;
    }

    public void DiscardCard(CardData card)
    {
        if (card != null)
        {
            discardPile.Add(card);
            LogDeckStatus($"Карта '{card.cardName}' отправлена в сброс. В сбросе: {discardPile.Count}");
        }
    }

    // Метод для отображения статуса колоды в консоли
    private void LogDeckStatus(string action)
    {
        Debug.Log($"[DeckManager] {action}");
        Debug.Log($"В колоде для добора: {drawPile.Count} карт");
        Debug.Log($"В сбросе: {discardPile.Count} карт");
        Debug.Log($"Всего карт: {deckCards.Count}");
        deckText.text = drawPile.Count.ToString() + "/" + deckCards.Count.ToString();

        if (drawPile.Count > 0)
        {
            string nextCards = "";
            int count = Mathf.Min(3, drawPile.Count);
            CardData[] cards = drawPile.ToArray();
            for (int i = 0; i < count; i++)
            {
                nextCards += cards[i].cardName;
                if (i < count - 1) nextCards += ", ";
            }
            Debug.Log($"Следующие карты: {nextCards}");
        }
    }

    // Публичные методы для получения информации
    public int GetRemainingCards()
    {
        return drawPile.Count;
    }

    public int GetTotalCards()
    {
        return deckCards.Count;
    }

    public int GetDiscardCount()
    {
        return discardPile.Count;
    }

    public bool IsEmpty()
    {
        return drawPile.Count == 0;
    }
}