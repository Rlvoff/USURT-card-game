using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPreviewManager : MonoBehaviour
{
    public static InventoryPreviewManager Instance { get; private set; }

    [Header("Элементы превью")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private float previewScale = 1.5f;

    [Header("Настройки для карт")]
    [SerializeField] private bool showForCards = true;

    [Header("Настройки для предметов")]
    [SerializeField] private bool showForItems = true;
    [SerializeField] private GameObject itemPreviewPrefab; // специальный префаб для превью предметов

    private GameObject currentPreviewObject;
    private RectTransform previewRectTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (previewPanel != null)
        {
            previewRectTransform = previewPanel.GetComponent<RectTransform>();
            previewPanel.SetActive(false);
        }
    }

    public void ShowPreview(GameObject sourceObject)
    {
        if (previewPanel == null || sourceObject == null) return;

        // Удаляем старую копию, если есть
        if (currentPreviewObject != null)
            Destroy(currentPreviewObject);

        // Определяем тип объекта и показываем соответствующее превью
        if (showForCards && sourceObject.GetComponent<InventoryCardSlot>() != null)
        {
            ShowCardPreview(sourceObject);
        }
        else if (showForItems && sourceObject.GetComponent<InventoryItemSlot>() != null)
        {
            ShowItemPreview(sourceObject);
        }
        else
        {
            return; // неизвестный тип или отключено
        }

        previewPanel.SetActive(true);
    }

    private void ShowCardPreview(GameObject sourceObject)
    {
        // Для карт - копируем сам объект с масштабированием
        currentPreviewObject = Instantiate(sourceObject, previewPanel.transform);

        RectTransform copyRect = currentPreviewObject.GetComponent<RectTransform>();
        if (copyRect != null)
        {
            copyRect.anchoredPosition = Vector2.zero;
            copyRect.localScale = Vector3.one * previewScale;
        }

        // Отключаем скрипты и raycast
        DisableScripts(currentPreviewObject);
        DisableRaycast(currentPreviewObject);
    }

    private void ShowItemPreview(GameObject sourceObject)
    {
        if (itemPreviewPrefab == null)
        {
            Debug.LogError("Item Preview Prefab is not assigned!");
            return;
        }

        // Получаем данные предмета из исходного слота
        InventoryItemSlot itemSlot = sourceObject.GetComponent<InventoryItemSlot>();
        if (itemSlot == null || itemSlot.CurrentItemData == null)
        {
            Debug.LogError("Item data not found!");
            return;
        }

        // Создаём превью из специального префаба
        currentPreviewObject = Instantiate(itemPreviewPrefab, previewPanel.transform);

        // Заполняем данные в превью
        FillItemPreview(currentPreviewObject, itemSlot.CurrentItemData);

        // Позиционируем
        RectTransform previewRect = currentPreviewObject.GetComponent<RectTransform>();
        if (previewRect != null)
        {
            previewRect.anchoredPosition = Vector2.zero;
            previewRect.localScale = Vector3.one * previewScale;
        }

        // Отключаем raycast
        DisableRaycast(currentPreviewObject);
    }

    private void FillItemPreview(GameObject previewObj, ItemData itemData)
    {
        // Ищем и заполняем иконку
        Image[] icons = previewObj.GetComponentsInChildren<Image>();
        foreach (Image icon in icons)
        {
            if (icon.name.Contains("Icon"))
                icon.sprite = itemData.icon;
        }

        // Ищем и заполняем тексты по имени или тегу
        TextMeshProUGUI[] texts = previewObj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name.Contains("Name"))
                text.text = itemData.name;
            else if (text.name.Contains("Description"))
                text.text = itemData.description;
        }
    }

    private void DisableScripts(GameObject obj)
    {
        InventoryCardSlot cardSlot = obj.GetComponent<InventoryCardSlot>();
        if (cardSlot != null) cardSlot.enabled = false;

        InventoryItemSlot itemSlot = obj.GetComponent<InventoryItemSlot>();
        if (itemSlot != null) itemSlot.enabled = false;
    }

    private void DisableRaycast(GameObject obj)
    {
        Image[] images = obj.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }

        TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            text.raycastTarget = false;
        }
    }

    public void HidePreview()
    {
        if (previewPanel != null)
        {
            previewPanel.SetActive(false);

            if (currentPreviewObject != null)
                Destroy(currentPreviewObject);
        }
    }
}