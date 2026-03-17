using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Элементы UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image background;

    private ItemData currentItemData;

    public ItemData CurrentItemData => currentItemData;

    public void Setup(ItemData itemData, int quantity)
    {
        currentItemData = itemData;

        if (itemData == null)
        {
            Debug.LogError("ItemData is null!");
            return;
        }

        if (iconImage != null && itemData.icon != null)
           iconImage.sprite = itemData.icon;

        if (nameText != null)
            nameText.text = itemData.name;

        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? ("x" + quantity.ToString()) : "";
        }

        if (background != null)
        {
            //background.color = Color.grey;
        }

        ConfigureRaycastTargets();
    }

    private void ConfigureRaycastTargets()
    {
        // Отключаем raycast на текстах
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            text.raycastTarget = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItemData != null && InventoryPreviewManager.Instance != null)
        {
            InventoryPreviewManager.Instance.ShowPreview(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryPreviewManager.Instance != null)
        {
            InventoryPreviewManager.Instance.HidePreview();
        }
    }
}