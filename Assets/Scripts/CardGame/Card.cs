using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Данные карты")]
    [HideInInspector] public CardData cardData;

    [Header("Визуальные элементы")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI phrazeText;
    [SerializeField] private UnityEngine.UI.Image cardImage;      
    [SerializeField] private UnityEngine.UI.Image backgroundImage;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private Image speciesBackground;
    [SerializeField] private GameObject attackElement;
    [SerializeField] private GameObject healthElement;
    [SerializeField] private GameObject effectDescriptionElement;

    [Header("Настройки анимации")]
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private float hoverScale = 1.2f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Quaternion originalHandRotation;
    private bool isDragging = false;

    [HideInInspector] public Transform originalParent;

    public enum CardLocation
    {
        InDeck,
        InHand,
        OnField,
        Discarded
    }

    public CardLocation currentLocation = CardLocation.InDeck;
    private Vector2 dragOffset;
    private HandVisuals handVisuals;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
        handVisuals = GetComponentInParent<HandVisuals>();
        currentLocation = CardLocation.InDeck;

        Debug.Log($"[Card] Awake: {gameObject.name}, canvas found: {canvas != null}");
    }

    private void Update()
    {
        // Плавное изменение масштаба
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );

        // Плавное изменение поворота (если не тащим)
        if (!isDragging)
        {
            // Добавляем проверку, чтобы избежать NaN
            if (targetRotation != Quaternion.identity && targetRotation != transform.localRotation)
            {
                transform.localRotation = Quaternion.Lerp(
                    transform.localRotation,
                    targetRotation,
                    Time.deltaTime * animationSpeed
                );
            }
        }
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public void Setup(CardData data)
    {
        cardData = data;
        Debug.Log($"[Card] Setup: {data.cardName}, type: {data.GetType()}");

        // Базовая информация
        if (nameText != null)
            nameText.text = data.cardName;

        if (manaText != null)
            manaText.text = data.manaCost.ToString();

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (phrazeText != null)
            phrazeText.text = data.phraze;

        if (cardImage != null && data.icon != null)
            cardImage.sprite = data.icon;

        // Устанавливаем текст и видимость вида
        bool showSpecies = data.species != CardSpecies.Neutral;

        if (speciesText != null)
        {
            speciesText.gameObject.SetActive(showSpecies);
            if (showSpecies)
            {
                switch (data.species)
                {
                    case CardSpecies.Student:
                        speciesText.text = "Студент";
                        break;
                    case CardSpecies.Teacher:
                        speciesText.text = "Преподаватель";
                        break;
                }
            }
        }

        if (speciesBackground != null)
            speciesBackground.gameObject.SetActive(showSpecies);

        // Настройка в зависимости от типа карты
        bool showAttack = false;
        bool showHealth = false;
        int attack = 0;
        int health = 0;

        if (data is AttackingCardData attackingCard)
        {
            showAttack = true;
            showHealth = true;
            attack = attackingCard.attackPoints;
            health = attackingCard.defensePoints;
            Debug.Log($"[Card] Attacking card: attack={attack}, health={health}");
        }
        else if (data is ProtectingCardData protectingCard)
        {
            showHealth = true;
            health = protectingCard.defensePoints;
            Debug.Log($"[Card] Protecting card: health={health}");
        }
        else if (data is EffectCardData effectCard)
        {
            Debug.Log($"[Card] Effect card");
        }

        // Управление видимостью элементов
        if (attackElement != null)
            attackElement.SetActive(showAttack);

        if (healthElement != null)
            healthElement.SetActive(showHealth);

        // Установка значений
        if (attackText != null)
            attackText.text = attack.ToString();

        if (healthText != null)
            healthText.text = health.ToString();

        // Цвет фона
        if (backgroundImage != null)
        {
            if (data is AttackingCardData)
                backgroundImage.color = new Color(0.9f, 0.5f, 0.5f, 1f);
            else if (data is ProtectingCardData)
                backgroundImage.color = new Color(0.5f, 0.7f, 0.9f, 1f);
            else if (data is EffectCardData)
                backgroundImage.color = new Color(0.8f, 0.8f, 0.5f, 1f);

            // Устанавливаем цвет speciesBackground такой же как у backgroundImage, если вид показывается
            if (speciesBackground != null && showSpecies)
                speciesBackground.color = backgroundImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[Card] OnPointerEnter: {cardData?.cardName}");
        if (!isDragging && currentLocation == CardLocation.InHand)
        {
            targetScale = originalScale * hoverScale;
            transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Card] OnPointerExit: {cardData?.cardName}");
        if (!isDragging && currentLocation == CardLocation.InHand)
        {
            targetScale = originalScale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[Card] OnBeginDrag: {gameObject.name}");

        if (currentLocation != CardLocation.InHand)
        {
            Debug.Log($"[Card] Not in hand, drag ignored");
            return;
        }

        isDragging = true;

        // Сохраняем угол из руки
        originalHandRotation = transform.localRotation;

        // Ставим карту ПРЯМО при перетаскивании - ЭТО ВАЖНО!
        targetRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity; // Мгновенно, без анимации

        // Запоминаем родителя и поднимаем над всем
        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Уведомляем HandVisuals
        if (handVisuals != null)
            handVisuals.OnCardDragStart(this);

        // Вычисляем смещение курсора относительно центра карты
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out dragOffset
        );

        Vector2 cardLocalPos = rectTransform.anchoredPosition;
        dragOffset = cardLocalPos - dragOffset;

        Debug.Log($"[Card] Drag started with offset: {dragOffset}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Получаем позицию курсора в локальных координатах Canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPoint
        );

        // Двигаем карту за курсором с учётом смещения
        rectTransform.anchoredPosition = localPoint + dragOffset;

        Debug.Log($"[Card] Dragging to: {rectTransform.anchoredPosition}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[Card] OnEndDrag");

        if (!isDragging) return;

        isDragging = false;

        if (handVisuals != null)
            handVisuals.OnCardDragEnd(this);

        bool placed = false;

        // Находим все объекты под курсором
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            Debug.Log($"[Card] Raycast: {result.gameObject.name}");

            CardSlot slot = result.gameObject.GetComponent<CardSlot>();
            if (slot == null)
                slot = result.gameObject.GetComponentInParent<CardSlot>();

            if (slot != null && slot.CanPlaceCard(this) && slot.IsPlayerSlot())
            {
                // Проверяем, хватает ли маны
                if (TurnManager.Instance == null)
                {
                    Debug.Log("[Card] TurnManager не найден!");
                    continue;
                }

                int manaCost = cardData != null ? cardData.manaCost : 0;

                if (TurnManager.Instance.SpendMana(true, manaCost))
                {
                    Debug.Log($"[Card] Маны хватает ({manaCost}), размещаем карту");
                    slot.PlaceCard(this);
                    placed = true;
                    break;
                }
                else
                {
                    Debug.Log($"[Card] Не хватает маны! Нужно: {manaCost}");
                    // Здесь можно добавить визуальное уведомление
                    ShowManaWarning();
                    // Продолжаем проверку других слотов (хотя обычно не нужно)
                }
            }
        }

        if (!placed)
        {
            Debug.Log("[Card] Карта не размещена, возвращаем в руку");
            ReturnToHand();
        }
    }

    private void ShowManaWarning()
    {
        // Визуальное уведомление - можно сделать красную вспышку на мане
        Debug.LogWarning("[Card] НЕДОСТАТОЧНО МАНЫ!");

        // Здесь можно добавить:
        // - Красную вспышку на счетчике маны
        // - Текст "Not enough mana!"
        // - Звуковой эффект
        // - Вибрацию карты и т.д.
    }

    private void ReturnToHand()
    {
        isDragging = false;
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        // Восстанавливаем угол карты в руке
        targetRotation = originalHandRotation;

        Debug.Log($"[Card] Returned to hand");
    }

    public void ResetRotation()
    {
        targetRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;
    }
}