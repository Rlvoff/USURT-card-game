using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public static FieldManager Instance { get; private set; }

    [Header("Размеры поля")]
    [SerializeField] private int rows = 2;
    [SerializeField] private int columns = 4;

    private CardInstance[,] playerField;
    private CardInstance[,] enemyField;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerField = new CardInstance[rows, columns];
        enemyField = new CardInstance[rows, columns];
    }

    // Метод для размещения карты на поле - ИСПРАВЛЕНО
    public void PlaceCardOnField(CardInstance card, bool isPlayer, int row, int column)
    {
        var field = isPlayer ? playerField : enemyField;

        if (row < 0 || row >= rows || column < 0 || column >= columns)
        {
            Debug.LogError("Некорректные координаты слота");
            return;
        }

        if (field[row, column] != null)
        {
            Debug.LogError("Слот уже занят");
            return;
        }

        field[row, column] = card;
        card.currentRow = row;
        card.currentColumn = column;

        UpdateProtection();
    }

    // Получить карту в слоте
    public CardInstance GetCard(bool isPlayer, int row, int column)
    {
        var field = isPlayer ? playerField : enemyField;

        if (row < 0 || row >= rows || column < 0 || column >= columns)
            return null;

        return field[row, column];
    }

    // Проверить, защищён ли слот
    public bool IsSlotProtected(bool isPlayer, int row, int column)
    {
        var field = isPlayer ? playerField : enemyField;

        // Проверяем, есть ли карта защиты в переднем ряду
        if (row == 1) // задний ряд
        {
            if (column >= 0 && column < columns)
            {
                CardInstance frontCard = field[0, column];
                if (frontCard != null && frontCard.isProtecting)
                    return true;
            }
        }
        return false;
    }

    // Удалить карту
    public void RemoveCard(CardInstance card)
    {
        if (card == null) return;

        var field = card.currentRow >= 0 && card.currentRow < rows &&
                   card.currentColumn >= 0 && card.currentColumn < columns
                   ? (IsPlayerCard(card) ? playerField : enemyField) : null;

        if (field != null)
            field[card.currentRow, card.currentColumn] = null;

        UpdateProtection();
    }

    private bool IsPlayerCard(CardInstance card)
    {
        // Простая проверка - можно доработать
        return true; // По умолчанию считаем картой игрока
    }

    // Обновить состояние защиты
    private void UpdateProtection()
    {
        // Здесь можно обновить визуальные индикаторы защиты
        // Например, подсветить защищённые слоты
    }

    // Очистить поле
    public void ClearField()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (playerField[r, c] != null)
                    Destroy(playerField[r, c].gameObject);
                if (enemyField[r, c] != null)
                    Destroy(enemyField[r, c].gameObject);

                playerField[r, c] = null;
                enemyField[r, c] = null;
            }
        }
    }

    public static class TargetResolver
    {
        public static List<CardInstance> GetAdjacentCards(FieldManager field, CardInstance source)
        {
            var result = new List<CardInstance>();
            int row = source.currentRow;
            int col = source.currentColumn;

            if (col > 0)
            {
                var leftCard = field.GetCard(true, row, col - 1);
                if (leftCard != null) result.Add(leftCard);
            }
            if (col < 3)
            {
                var rightCard = field.GetCard(true, row, col + 1);
                if (rightCard != null) result.Add(rightCard);
            }
            return result;
        }

        public static List<CardInstance> GetAllCards(FieldManager field, bool playerCards = true)
        {
            var result = new List<CardInstance>();
            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 4; c++)
                {
                    var card = field.GetCard(playerCards, r, c);
                    if (card != null) result.Add(card);
                }
            return result;
        }
    }
}