using UnityEngine;
using System.Collections.Generic;

public class HandVisuals : MonoBehaviour
{
    [Header("Настройки руки")]
    [SerializeField] private float fanSpread = -10f;        // Разброс углов
    [SerializeField] private float cardSpacing = 100f;      // Расстояние между картами
    [SerializeField] private float verticalSpacing = 30f;    // Вертикальный разброс
    [SerializeField] private float animationSpeed = 10f;     // Скорость анимации

    [Header("Позиции руки")]
    [SerializeField] private float visibleY = 0f;            // Позиция когда рука видна
    [SerializeField] private float hiddenY = -200f;          // Позиция когда рука скрыта

    private RectTransform handRect;
    private List<Card> cardsInHand = new List<Card>();
    private bool isHandVisible = true;
    private bool isDragging = false;  // Флаг, что карта перетаскивается

    private void Start()
    {
        handRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Анимация появления/скрытия руки
        float targetY = isHandVisible ? visibleY : hiddenY;
        Vector3 pos = handRect.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * animationSpeed);
        handRect.anchoredPosition = pos;

        // Обновляем позиции карт в руке только если не тащим карту
        if (!isDragging)
        {
            UpdateHandVisuals();
        }
    }

    public void AddCard(Card card)
    {
        if (!cardsInHand.Contains(card))
        {
            cardsInHand.Add(card);
            card.transform.SetParent(transform);
            card.originalParent = transform;
            card.currentLocation = Card.CardLocation.InHand;
            UpdateHandVisuals();
        }
    }

    public void RemoveCard(Card card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
            UpdateHandVisuals();
        }
    }

    // Вызывается из Card при начале перетаскивания
    public void OnCardDragStart(Card card)
    {
        isDragging = true;
        Debug.Log("[HandVisuals] Drag started");
    }

    // Вызывается из Card при окончании перетаскивания
    public void OnCardDragEnd(Card card)
    {
        isDragging = false;
        UpdateHandVisuals(); // пересчитать позиции после возврата карты
        Debug.Log("[HandVisuals] Drag ended");
    }

    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        if (cardCount == 1)
        {
            // Одна карта по центру
            SetCardTransform(cardsInHand[0], 0, 0, 0);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            Card card = cardsInHand[i];

            // Пропускаем карту, которую перетаскивают (хотя isDragging=true, но на всякий случай)
            if (card != null && card.IsDragging())
                continue;

            // Вычисляем позицию и поворот для карты
            float t = (float)i / (cardCount - 1); // 0..1

            // Угол поворота (веер)
            float angle = Mathf.Lerp(-fanSpread, fanSpread, t);

            // Горизонтальное смещение
            float horizontalOffset = (i - (cardCount - 1) / 2f) * cardSpacing;

            // Вертикальное смещение (парабола)
            float normalizedPos = (2f * i / (cardCount - 1) - 1f);
            float verticalOffset = verticalSpacing * (1 - normalizedPos * normalizedPos);

            SetCardTransform(card, horizontalOffset, verticalOffset, angle);
        }
    }

    private void SetCardTransform(Card card, float x, float y, float angle)
    {
        if (card == null) return;

        RectTransform cardRect = card.GetComponent<RectTransform>();

        // Устанавливаем целевую позицию и поворот
        Vector3 targetPos = new Vector3(x, y, 0);
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        // Плавно перемещаем
        cardRect.anchoredPosition = Vector3.Lerp(cardRect.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);
        cardRect.localRotation = Quaternion.Lerp(cardRect.localRotation, targetRot, Time.deltaTime * animationSpeed);
    }

    public void ShowHand(bool show)
    {
        isHandVisible = show;
    }

    public void SetHandVisible(bool visible)
    {
        isHandVisible = visible;
    }
}