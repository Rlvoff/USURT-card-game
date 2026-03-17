using UnityEngine;

public class DialogueActions : MonoBehaviour
{
    public static DialogueActions Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==== Методы для работы с инвентарём ====
    public void AddItem(int itemId, int quantity)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddItem(itemId, quantity);
        else
            Debug.LogError("InventoryManager не найден!");
    }

    public void AddCard(int cardId)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddCard(cardId);
        else
            Debug.LogError("InventoryManager не найден!");
    }

    public void AddCardToDeck(int cardId)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddCardToDeck(cardId);
        else
            Debug.LogError("InventoryManager не найден!");
    }

    public void RemoveCardFromDeck(int cardId)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveCardFromDeck(cardId);
        else
            Debug.LogError("InventoryManager не найден!");
    }

    public void RemoveItem(int itemId, int quantity = 1)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(itemId, quantity);
        else
            Debug.LogError("InventoryManager не найден!");
    }

    // ==== Методы для работы с квестами ====
    public void StartQuest(string questId)
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.StartQuest(questId);
        else
            Debug.LogError("QuestManager не найден!");
    }

    public void CompleteQuest(string questId)
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(questId);
        else
            Debug.LogError("QuestManager не найден!");
    }

    public void ReadyToCompleteQuest(string questId)
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.ReadyToCompleteQuest(questId);
        else
            Debug.LogError("QuestManager не найден!");
    }

    // ==== Методы для работы со сценами ====
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    // ==== Методы для работы с деньгами/опытом (если нужно) ====
    public void AddGold(int amount)
    {
        Debug.Log($"Добавлено {amount} золота");
        // Здесь вызов к системе валюты
    }

    public void AddExperience(int amount)
    {
        Debug.Log($"Добавлено {amount} опыта");
        // Здесь вызов к системе опыта
    }
}