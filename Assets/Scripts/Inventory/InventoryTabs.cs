using UnityEngine;
using UnityEngine.UI;

public class InventoryTabs : MonoBehaviour
{
    [Header("Панели вкладок")]
    [SerializeField] private GameObject itemsPanel;
    [SerializeField] private GameObject questsPanel;
    [SerializeField] private GameObject deckPanel;

    [Header("Кнопки вкладок")]
    [SerializeField] private Button itemsTabButton;
    [SerializeField] private Button questsTabButton;
    [SerializeField] private Button deckTabButton;

    private void Start()
    {
        // Подписываемся на кнопки
        itemsTabButton.onClick.AddListener(ShowItems); 
        deckTabButton.onClick.AddListener(ShowDeck);
        questsTabButton.onClick.AddListener(ShowQuests);

        // Показываем стартовую вкладку (например, коллекцию)
        ShowDeck();
    }

    public void ShowItems()
    {
        // Переключение панелей
        itemsPanel.SetActive(true);
        questsPanel.SetActive(false);
        deckPanel.SetActive(false);

        // Переключение кнопок
        itemsTabButton.interactable = false;
        questsTabButton.interactable = true;
        deckTabButton.interactable = true;

        // Обновляем отображение предметов
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ItemInventoryDisplay();
    }

    public void ShowQuests()
    {
        // Переключение панелей
        itemsPanel.SetActive(false);
        questsPanel.SetActive(true);
        deckPanel.SetActive(false);

        // Переключение кнопок
        itemsTabButton.interactable = true;
        questsTabButton.interactable = false;
        deckTabButton.interactable = true;

        // Обновляем отображение коллекции
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.QuestDisplay();
    }

    public void ShowDeck()
    {
        // Переключение панелей
        itemsPanel.SetActive(false);
        questsPanel.SetActive(false);
        deckPanel.SetActive(true);

        // Переключение кнопок
        itemsTabButton.interactable = true;
        questsTabButton.interactable = true;
        deckTabButton.interactable = false;

        // Обновляем отображение колоды
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.DeckInventoryDisplay();
    }
}