using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("Настройки слота")]
    [SerializeField] private bool isPlayerSlot = true;  // true - слот игрока, false - противника
    [SerializeField] private int row = 0;               // 0 - передний ряд, 1 - задний ряд
    [SerializeField] private int column = 0;            // 0-3 (4 колонки)

    [Header("Визуальные элементы")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 0.3f);

    private CardInstance currentCard;
    private bool isOccupied = false;

    // Ссылки на соседние слоты (настраиваются в инспекторе)
    [Header("Связи слотов")]
    [SerializeField] private CardSlot frontSlot;  // слот перед этим
    [SerializeField] private CardSlot backSlot;   // слот за этим

    private void Start()
    {
        if (slotBackground != null)
            slotBackground.color = normalColor;
    }

    public bool IsOccupied() => isOccupied;
    public CardInstance GetCurrentCard() => currentCard;
    public int GetRow() => row;
    public int GetColumn() => column;
    public CardSlot GetFrontSlot() => frontSlot;
    public CardSlot GetBackSlot() => backSlot;
    public bool IsPlayerSlot()
    {
        return isPlayerSlot;
    }

    // Проверка, можно ли положить карту в этот слот
    public bool CanPlaceCard(Card card)
    {
        // Слот должен быть свободен
        if (isOccupied) return false;

        // Проверяем тип карты
        if (card == null || card.cardData == null) return false;

        // Карты эффектов не ставятся на поле
        if (card.cardData is EffectCardData) return false;

        // Проверяем, что карту кладёт игрок в свой слот
        // (предполагаем, что карты всегда кладёт игрок)
        if (!isPlayerSlot) return false; // Нельзя класть в слот противника

        // Юниты можно ставить
        return true;
    }

    // Размещение карты в слоте
    public void PlaceCard(Card card)
    {
        if (card == null) return;

        Debug.Log($"[CardSlot] PlaceCard: {card.gameObject.name} -> слот [{row},{column}]");

        // Загружаем префаб карты
        GameObject cardPrefab = Resources.Load<GameObject>("Cards/CardPrefab");
        if (cardPrefab == null)
        {
            Debug.LogError("[CardSlot] CardPrefab не найден в Resources/Cards/");
            return;
        }

        // Создаём экземпляр префаба
        GameObject cardInstanceGO = Instantiate(cardPrefab, transform);
        cardInstanceGO.name = card.cardData.cardName;

        // Получаем RectTransform
        RectTransform cardRect = cardInstanceGO.GetComponent<RectTransform>();

        if (cardRect != null)
        {
            // Центрируем карту в слоте
            cardRect.anchoredPosition = Vector2.zero;

            // Уменьшаем размер карты (0.5 = половина от оригинала)
            cardRect.localScale = new Vector3(0.5f, 0.5f, 1f);

            // Поворот сбрасываем
            cardRect.localRotation = Quaternion.identity;

            Debug.Log($"[CardSlot] Карта уменьшена до масштаба 0.5");
        }

        // Добавляем CardInstance
        CardInstance instance = cardInstanceGO.AddComponent<CardInstance>();
        instance.Initialize(card.cardData, row, column);
        instance.currentSlot = this;

        // Настраиваем визуал через Card.Setup
        Card visualCard = cardInstanceGO.GetComponent<Card>();
        if (visualCard != null)
        {
            visualCard.Setup(card.cardData);
            visualCard.currentLocation = Card.CardLocation.OnField;
            visualCard.enabled = false;
            visualCard.ResetRotation();
        }

        currentCard = instance;
        isOccupied = true;

        if (FieldManager.Instance != null)
            FieldManager.Instance.PlaceCardOnField(instance, isPlayerSlot, row, column);

        if (HandManager.Instance != null)
            HandManager.Instance.RemoveCardFromHand(card);

        Destroy(card.gameObject);

        UpdateSlotVisual();
        Debug.Log($"[CardSlot] Карта {card.cardData.cardName} размещена в слоте");
    }

    // Удаление карты из слота
    public void RemoveCard()
    {
        if (currentCard != null)
        {
            Destroy(currentCard.gameObject);
            currentCard = null;
        }
        isOccupied = false;
        UpdateSlotVisual();
    }

    // Проверка, защищён ли этот слот картой в переднем ряду
    public bool IsProtected()
    {
        if (frontSlot != null && frontSlot.IsOccupied())
        {
            CardInstance frontCard = frontSlot.GetCurrentCard();
            if (frontCard != null && frontCard.isProtecting)
            {
                return true;
            }
        }
        return false;
    }

    // Получить карту, которая защищает этот слот (если есть)
    public CardInstance GetProtector()
    {
        if (frontSlot != null && frontSlot.IsOccupied())
        {
            CardInstance frontCard = frontSlot.GetCurrentCard();
            if (frontCard != null && frontCard.isProtecting)
            {
                return frontCard;
            }
        }
        return null;
    }

    // Обработка сброса карты
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Получаем карту из руки
            Card draggedCard = eventData.pointerDrag.GetComponent<Card>();

            if (draggedCard != null && CanPlaceCard(draggedCard))
            {
                // Проверяем, хватает ли маны
                if (TurnManager.Instance != null)
                {
                    bool canSpendMana = TurnManager.Instance.SpendMana(
                        isPlayerSlot,
                        draggedCard.cardData.manaCost
                    );

                    if (!canSpendMana)
                    {
                        Debug.Log("Недостаточно маны!");
                        return;
                    }
                }

                // Размещаем карту
                PlaceCard(draggedCard);

                // Подсвечиваем слот при успешном размещении
                if (slotBackground != null)
                {
                    StartCoroutine(FlashSlot());
                }
            }
        }
    }

    // Визуальная подсветка при размещении карты
    private System.Collections.IEnumerator FlashSlot()
    {
        slotBackground.color = highlightColor;
        yield return new WaitForSeconds(0.2f);
        slotBackground.color = normalColor;
    }

    // Обновление визуала слота (можно вызывать при изменениях)
    private void UpdateSlotVisual()
    {
        if (slotBackground != null)
        {
            if (isOccupied)
            {
                // Можно затемнить занятый слот или оставить как есть
                slotBackground.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            }
            else
            {
                slotBackground.color = normalColor;
            }
        }
    }

    // Визуальная подсветка при наведении (опционально)
    private void OnMouseEnter()
    {
        if (!isOccupied && slotBackground != null)
        {
            slotBackground.color = highlightColor;
        }
    }

    private void OnMouseExit()
    {
        if (!isOccupied && slotBackground != null)
        {
            slotBackground.color = normalColor;
        }
    }
}