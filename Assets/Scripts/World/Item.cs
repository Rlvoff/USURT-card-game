using UnityEngine;
using UnityEngine.Events;

public interface IPersistable
{
    string GetUniqueID();
    string GetState();
    void SetState(string state);
}

public class Item : BaseInteractable, IPersistable
{
    [SerializeField] private int itemId;
    [SerializeField] private int quantity = 1;
    [SerializeField] private ItemType itemType;

    [Header("Сохранение")]
    [SerializeField] private string uniqueID; // Уникальный ID для этого экземпляра
    private bool isCollected = false; // Был ли предмет уже собран

    [SerializeField] private UnityEvent onPickup;

    private string itemName;

    private void Start()
    {
        // Генерируем уникальный ID, если не задан
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"{itemType}_{itemId}_{gameObject.scene.name}_{transform.position.x:F1}_{transform.position.y:F1}";
        }

        if (itemType == ItemType.Regular)
        {
            if (InventoryManager.Instance != null)
            {
                ItemData data = InventoryManager.Instance.GetItemData(itemId);
                if (data != null)
                {
                    itemName = data.name;
                }
            }
        }
        else // Card
        {
            if (InventoryManager.Instance != null)
            {
                CardData data = InventoryManager.Instance.GetCardData(itemId);
                if (data != null)
                {
                    itemName = data.cardName;
                }
            }
        }
    }

    public override void Interact(PlayerMovement player)
    {
        if (isCollected) return; // Если уже собран - ничего не делаем

        Debug.Log($"Picked up {quantity}x {itemName} (Type: {itemType})");

        if (itemType == ItemType.Card)
        {
            InventoryManager.Instance.AddCard(itemId);
        }
        else
        {
            InventoryManager.Instance.AddItem(itemId, quantity);
        }

        onPickup?.Invoke();

        // Помечаем как собранный и сохраняем состояние
        isCollected = true;
        gameObject.SetActive(false);

        // Сохраняем состояние в SceneStateManager
        SceneStateManager.Instance?.RegisterChange(this);
    }

    public override string GetInteractionPrompt()
    {
        if (itemType == ItemType.Card)
        {
            return $"Поднять карту {itemName} [E]";
        }
        else
        {
            return $"Поднять {itemName} [E]";
        }
    }

    // Реализация IPersistable
    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string GetState()
    {
        return isCollected ? "collected" : "available";
    }

    public void SetState(string state)
    {
        isCollected = (state == "collected");
        gameObject.SetActive(!isCollected);
    }
}