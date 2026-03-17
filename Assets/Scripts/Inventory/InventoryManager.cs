using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Добавляем для работы со сценами

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Databases - справочники всех возможных предметов")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private QuestDatabase questDatabase;

    [Header("Инвентарь игрока - то, что уже собрано")]
    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
    [SerializeField] private List<InventoryCard> cards = new List<InventoryCard>();
    [SerializeField] private List<DeckCard> deckCards = new List<DeckCard>(); // Карты в колоде

    [Header("UI элементы")]
    [SerializeField] private GameObject canvasInventoryGroup;
    [SerializeField] private Transform questContainer;       // Родительский объект для списка квестов
    [SerializeField] private Transform availableDeckCardsContainer; // Родительский объект в котором находятся карты доступные для колоды
    [SerializeField] private Transform deckCardsContainer; // Родительский объект в котором находятся карты добавленные в колоду
    [SerializeField] private Transform itemsContainer; // Родительский объект в котором находятся предметы из инвентаря
    [SerializeField] private GameObject questSlotPrefab; // префаб для квеста
    [SerializeField] private GameObject itemSlotPrefab; // префаб предмета в инвентаре
    [SerializeField] private GameObject cardSlotPrefab;      // префаб открытой карты
    [SerializeField] private GameObject unknownCardSlotPrefab; // префаб закрытой карты
    [SerializeField] private GameObject emptyDeckSlotPrefab; // префаб свободного слота в колоде

    [Header("Настройки колоды")]
    [SerializeField] private int maxDeckSize = 20; // Максимальное количество карт в колоде

    // Класс для хранения предмета в инвентаре
    [System.Serializable]
    public class InventoryItem
    {
        public int itemId;
        public int quantity;
    }

    // Класс для хранения карты в коллекции
    [System.Serializable]
    public class InventoryCard
    {
        public int cardId;
    }

    // Класс для хранения карты в колоде
    [System.Serializable]
    public class DeckCard
    {
        public int cardId;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TrySubscribeToInput();
    }

    private void Start()
    {
        TrySubscribeToInput();
    }

    private void OnDisable()
    {
        // Отписываемся от событий
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (GameInput.Instance != null)
        {
            GameInput.Instance.PlayerInputActions.Player.Inventory.performed -= OpenInventory;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке новой сцены скрываем инвентарь
        if (canvasInventoryGroup != null)
        {
            canvasInventoryGroup.SetActive(false);
        }

        // Переподписываемся на ввод (на случай, если GameInput пересоздался)
        TrySubscribeToInput();

        // Переинициализируем ссылки на UI элементы
        FindUIElements();
    }

    private void TrySubscribeToInput()
    {
        if (GameInput.Instance != null)
        {
            // Отписываемся сначала (на всякий случай)
            GameInput.Instance.PlayerInputActions.Player.Inventory.performed -= OpenInventory;
            // Подписываемся
            GameInput.Instance.PlayerInputActions.Player.Inventory.performed += OpenInventory;
        }
        else
        {
            Debug.Log("GameInput.Instance недоступен, подписка отложена");
        }
    }

    private void FindUIElements()
    {
        // Если UI элементы не заданы в инспекторе, пытаемся найти их в сцене
        if (canvasInventoryGroup == null)
        {
            // Ищем Canvas по тегу или имени
            GameObject canvas = GameObject.Find("InventoryCanvas");
            if (canvas != null)
                canvasInventoryGroup = canvas;
        }

        // Аналогично для других контейнеров...
        if (questContainer == null)
            questContainer = GameObject.Find("QuestContainer")?.transform;

        if (availableDeckCardsContainer == null)
            availableDeckCardsContainer = GameObject.Find("AvailableCardsContainer")?.transform;

        if (deckCardsContainer == null)
            deckCardsContainer = GameObject.Find("DeckCardsContainer")?.transform;

        if (itemsContainer == null)
            itemsContainer = GameObject.Find("ItemsContainer")?.transform;
    }

    private void OpenInventory(InputAction.CallbackContext context)
    {
        // Проверяем, существует ли Canvas
        if (canvasInventoryGroup == null)
        {
            Debug.LogError("Inventory Canvas не найден!");
            return;
        }

        bool willBeActive = !canvasInventoryGroup.activeSelf;
        canvasInventoryGroup.SetActive(willBeActive);
        Debug.Log($"inventory open: {willBeActive}");

        if (willBeActive)
        {
            DeckInventoryDisplay();      // Показываем все карты и колоду
            ItemInventoryDisplay();      // Показываем предметы
            QuestDisplay();              // Показываем квесты

            // Безопасно вызываем HidePreview
            if (InventoryPreviewManager.Instance != null)
                InventoryPreviewManager.Instance.HidePreview();
        }

        if (GameManager.Instance != null)
            GameManager.Instance.SetInventoryActive(willBeActive);
    }

    // Обновление отображения колоды (теперь показывает все карты)
    public void DeckInventoryDisplay()
    {
        ClearContainer(availableDeckCardsContainer);
        ClearContainer(deckCardsContainer);
        DisplayAllAvailableCards();  // Все карты из базы, с сортировкой
        DisplayDeckCards();          // Карты в колоде
    }

    // Обновление отображения предметов
    public void ItemInventoryDisplay()
    {
        ClearContainer(itemsContainer);
        DisplayOwnedItems();
    }

    public void QuestDisplay()
    {
        ClearContainer(questContainer);
        DisplayActiveQuests();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // Отображение активных квестов
    private void DisplayActiveQuests()
    {
        if (QuestManager.Instance == null || questDatabase == null) return;

        // Получаем все активные квесты из QuestManager
        List<string> readyQuests = QuestManager.Instance.GetReadyToCompleteQuests();
        List<string> activeQuests = QuestManager.Instance.GetActiveQuests();

        // Сначала показываем квесты, готовые к сдаче (приоритет)
        foreach (string questId in readyQuests)
        {
            DisplayQuest(questId, QuestStatus.ReadyToComplete);
        }

        // Потом остальные активные
        foreach (string questId in activeQuests)
        {
            DisplayQuest(questId, QuestStatus.InProgress);
        }
    }

    private void DisplayQuest(string questId, QuestStatus status)
    {
        QuestData questData = GetQuestData(questId);
        if (questData == null) return;

        GameObject slot = Instantiate(questSlotPrefab, questContainer);
        InventoryQuestSlot questSlot = slot.GetComponent<InventoryQuestSlot>();

        if (questSlot != null)
        {
            questSlot.Setup(questData, status);
        }
    }

    // Вспомогательный метод для получения данных квеста
    private QuestData GetQuestData(string questId)
    {
        return questDatabase?.quests.Find(q => q.questId == questId);
    }

    // Отображение всех карт из базы (открытые и закрытые)
    private void DisplayAllAvailableCards()
    {
        if (cardDatabase == null) return;

        // Списки для открытых и закрытых карт
        List<CardData> ownedCards = new List<CardData>();
        List<CardData> unknownCards = new List<CardData>();

        // Собираем все карты из базы
        AddCardsByStatus(cardDatabase.attackingCards, ownedCards, unknownCards);
        AddCardsByStatus(cardDatabase.protectingCards, ownedCards, unknownCards);
        AddCardsByStatus(cardDatabase.effectCards, ownedCards, unknownCards);

        // Сортируем по ID
        ownedCards.Sort((a, b) => a.id.CompareTo(b.id));
        unknownCards.Sort((a, b) => a.id.CompareTo(b.id));

        // Сначала показываем открытые карты
        foreach (var cardData in ownedCards)
        {
            DisplayAvailableCard(cardData);
        }

        // Потом показываем закрытые карты
        foreach (var cardData in unknownCards)
        {
            DisplayAvailableCard(cardData);
        }
    }

    private void AddCardsByStatus(List<AttackingCardData> cardList, List<CardData> owned, List<CardData> unknown)
    {
        foreach (var card in cardList)
        {
            if (cards.Exists(c => c.cardId == card.id))
                owned.Add(card);
            else
                unknown.Add(card);
        }
    }

    private void AddCardsByStatus(List<ProtectingCardData> cardList, List<CardData> owned, List<CardData> unknown)
    {
        foreach (var card in cardList)
        {
            if (cards.Exists(c => c.cardId == card.id))
                owned.Add(card);
            else
                unknown.Add(card);
        }
    }

    private void AddCardsByStatus(List<EffectCardData> cardList, List<CardData> owned, List<CardData> unknown)
    {
        foreach (var card in cardList)
        {
            if (cards.Exists(c => c.cardId == card.id))
                owned.Add(card);
            else
                unknown.Add(card);
        }
    }

    // Вспомогательный метод для отображения одной карты в панели доступных
    private void DisplayAvailableCard(CardData cardData)
    {
        if (cardData == null) return;

        bool hasCard = cards.Exists(c => c.cardId == cardData.id);
        bool isInDeck = deckCards.Exists(d => d.cardId == cardData.id);

        // Если карта уже в колоде - не показываем в доступных
        if (isInDeck) return;

        // Выбираем нужный префаб
        GameObject prefabToUse = hasCard ? cardSlotPrefab : unknownCardSlotPrefab;

        GameObject slot = Instantiate(prefabToUse, availableDeckCardsContainer);

        // Для открытых карт настраиваем данные
        if (hasCard)
        {
            InventoryCardSlot slotUI = slot.GetComponent<InventoryCardSlot>();
            if (slotUI != null)
            {
                slotUI.Setup(cardData);
                slotUI.SetAsDeckCard(false);
                slotUI.SetInteractable(true);
            }
        }
        else
        {
            // Для закрытых карт отключаем возможность взаимодействия
            InventoryCardSlot slotUI = slot.GetComponent<InventoryCardSlot>();
            if (slotUI != null)
            {
                slotUI.SetInteractable(false);
            }
        }
    }

    // Отображение карт в колоде
    private void DisplayDeckCards()
    {
        if (cardDatabase == null) return;

        // Сначала показываем карты в колоде
        foreach (var deckCard in deckCards)
        {
            CardData cardData = FindCardInDatabase(deckCard.cardId);
            if (cardData != null)
            {
                GameObject slot = Instantiate(cardSlotPrefab, deckCardsContainer);

                // Уменьшаем карту
                RectTransform rectTransform = slot.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(0.35f, 0.35f, 0f);
                }

                InventoryCardSlot slotUI = slot.GetComponent<InventoryCardSlot>();
                if (slotUI != null)
                {
                    slotUI.Setup(cardData);
                    slotUI.SetAsDeckCard(true);
                }
            }
        }

        // Заполняем оставшиеся слоты пустыми префабами
        int remainingSlots = maxDeckSize - deckCards.Count;
        for (int i = 0; i < remainingSlots; i++)
        {
            Instantiate(emptyDeckSlotPrefab, deckCardsContainer);
        }
    }

    // Отображение предметов, которые есть у игрока
    private void DisplayOwnedItems()
    {
        if (itemDatabase == null) return;

        foreach (var ownedItem in items)
        {
            ItemData itemData = GetItemData(ownedItem.itemId);
            if (itemData != null)
            {
                GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
                InventoryItemSlot slotUI = slot.GetComponent<InventoryItemSlot>();
                if (slotUI != null)
                {
                    slotUI.Setup(itemData, ownedItem.quantity);
                }
            }
        }
    }

    public void AddItem(int itemId, int quantity)
    {
        InventoryItem existingItem = items.Find(i => i.itemId == itemId);
        ItemData itemData = GetItemData(itemId);
        string itemName = itemData?.name ?? "Unknown";

        if (existingItem != null)
        {
            existingItem.quantity += quantity;
            Debug.Log($"Добавлено к существующему: {quantity}x {itemName}. Теперь всего: {existingItem.quantity}");
        }
        else
        {
            items.Add(new InventoryItem { itemId = itemId, quantity = quantity });
            Debug.Log($"Добавлен новый предмет: {quantity}x {itemName}");
        }

        // Уведомление
        Debug.Log($"Предмет: {itemName}, иконка: {(itemData?.icon != null ? itemData.icon.name : "NULL")}");
        if (NotificationManager.Instance != null && itemData != null)
        {
            if (quantity == 1)
            {
                NotificationManager.Instance.ShowItemMessage($"Получен предмет '{itemName}'", itemData.icon);
            }

            else
            {
                NotificationManager.Instance.ShowItemMessage($"Получен предмет '{itemName}' x{quantity}", itemData.icon);
            }
        }
    }

    public void AddCard(int cardId)
    {
        Debug.Log($"Добавлена карта в коллекцию: {cardId}");

        CardData card = GetCardData(cardId);
        if (card != null)
        {
            // Проверяем, есть ли уже такая карта
            bool alreadyExists = cards.Exists(c => c.cardId == cardId);

            if (alreadyExists)
            {
                Debug.Log($"Карта {card.cardName} уже есть в коллекции, не добавляем");
                return;
            }

            cards.Add(new InventoryCard { cardId = cardId });
            Debug.Log($"Карта {card.cardName} добавлена в коллекцию");

            // Уведомление о получении карты
            if (NotificationManager.Instance != null)
            {
                string message = $"Открыта новая карта '{card.cardName}'";
                NotificationManager.Instance.ShowCardMessage(message, card.icon);
            }

        }
    }

    // Добавить карту в колоду
    public void AddCardToDeck(int cardId)
    {
        // Проверяем, есть ли такая карта у игрока
        bool hasCard = cards.Exists(c => c.cardId == cardId);
        if (!hasCard)
        {
            Debug.Log("У вас нет этой карты!");
            return;
        }

        // Проверяем, не превышен ли лимит колоды
        if (deckCards.Count >= maxDeckSize)
        {
            Debug.Log($"Колода заполнена (максимум {maxDeckSize} карт)");
            return;
        }

        // Проверяем, не добавлена ли уже карта в колоду
        bool alreadyInDeck = deckCards.Exists(c => c.cardId == cardId);
        if (!alreadyInDeck)
        {
            deckCards.Add(new DeckCard { cardId = cardId });

            if (InventoryPreviewManager.Instance != null)
                InventoryPreviewManager.Instance.HidePreview();

            Debug.Log($"Карта добавлена в колоду");

            // Обновляем отображение колоды
            if (canvasInventoryGroup != null && canvasInventoryGroup.activeSelf)
            {
                DeckInventoryDisplay();
            }
        }
        else
        {
            Debug.Log($"Карта уже есть в колоде!");
        }
    }

    // Удалить карту из колоды
    public void RemoveCardFromDeck(int cardId)
    {
        DeckCard card = deckCards.Find(c => c.cardId == cardId);
        if (card != null)
        {
            deckCards.Remove(card);
            Debug.Log($"Карта удалена из колоды");

            // Обновляем отображение колоды
            if (canvasInventoryGroup != null && canvasInventoryGroup.activeSelf)
            {
                DeckInventoryDisplay();
            }
        }
    }

    public void RemoveItem(int itemId, int quantity = 1)
    {
        InventoryItem existingItem = items.Find(i => i.itemId == itemId);
        ItemData itemData = GetItemData(itemId);
        string itemName = GetItemData(itemId)?.name ?? "Unknown";

        if (existingItem != null)
        {
            if (existingItem.quantity <= quantity)
            {
                items.Remove(existingItem);
                Debug.Log($"Предмет {itemName} полностью удалён из инвентаря");
            }
            else
            {
                existingItem.quantity -= quantity;
                Debug.Log($"Удалено {quantity} шт. предмета {itemName}. Осталось: {existingItem.quantity}");
            }

            Debug.Log($"Предмет: {itemName}, иконка: {(itemData?.icon != null ? itemData.icon.name : "NULL")}");
            if (NotificationManager.Instance != null && itemData != null)
            {
                NotificationManager.Instance.ShowItemMessage($"-{quantity} {itemName}", itemData.icon);
            }

        }
        else
        {
            Debug.Log($"Предмета {itemName} нет в инвентаре");
        }

    }

    public ItemData GetItemData(int id)
    {
        return itemDatabase?.items.Find(item => item.id == id);
    }

    public List<InventoryCard> GetCards()
    {
        return cards;
    }

    public List<DeckCard> GetDeckCards()
    {
        return deckCards; // Карты, которые игрок собрал в колоду
    }

    public CardData GetCardData(int id)
    {
        CardData card = FindCardInDatabase(id);
        if (card == null)
        {
            Debug.LogError($"Card with ID {id} not found in database!");
        }
        return card;
    }

    // Вспомогательный метод для поиска карты во всех списках
    private CardData FindCardInDatabase(int id)
    {
        if (cardDatabase == null) return null;

        // Ищем в списке атакующих карт
        CardData card = cardDatabase.attackingCards.Find(card => card.id == id);
        if (card != null) return card;

        // Ищем в списке защищающихся карт
        card = cardDatabase.protectingCards.Find(card => card.id == id);
        if (card != null) return card;

        // Ищем в списке карт эффектов
        card = cardDatabase.effectCards.Find(card => card.id == id);
        return card; // может быть null
    }

    public int GetMaxDeckSize()
    {
        return maxDeckSize; // Теперь возвращаем реальное значение
    }

    public bool IsInventoryOpen()
    {
        return canvasInventoryGroup != null && canvasInventoryGroup.activeSelf;
    }

    public void CloseInventory()
    {
        if (canvasInventoryGroup != null && canvasInventoryGroup.activeSelf)
        {
            canvasInventoryGroup.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.SetInventoryActive(false);
        }
    }
}