using UnityEngine;
using System;
using UnityEngine.UI; // для кнопки

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum TurnOwner
    {
        Player,
        Enemy
    }

    [SerializeField] public TurnOwner currentTurn = TurnOwner.Player;
    [SerializeField] public int turnNumber = 1;

    [Header("Мана")]
    [SerializeField] public int playerMaxMana = 20;
    [SerializeField] public int playerCurrentMana = 20;
    [SerializeField] public int enemyMaxMana = 20;
    [SerializeField] public int enemyCurrentMana = 20;

    [Header("Здоровье")]
    [SerializeField] public int playerHealth = 30;
    [SerializeField] public int enemyHealth = 30;

    [Header("UI")]
    [SerializeField] public Button endTurnButton; // ссылка на кнопку

    public event Action<TurnOwner> OnTurnStart;
    public event Action<TurnOwner> OnTurnEnd;

    public TurnOwner CurrentTurn => currentTurn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Подключаем кнопку
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButtonClick);

        StartPlayerTurn();
    }

    // Вызывается кнопкой
    public void OnEndTurnButtonClick()
    {
        Debug.Log("Кнопка End Turn нажата!");
        EndTurn();
    }

    public void EndTurn()
    {
        OnTurnEnd?.Invoke(currentTurn);

        if (currentTurn == TurnOwner.Player)
        {
            // Переключаем на врага
            StartEnemyTurn();
        }
        else
        {
            // Переключаем на игрока
            StartPlayerTurn();
            endTurnButton.interactable = true;
        }
    }

    private void StartPlayerTurn()
    {
        currentTurn = TurnOwner.Player;
        turnNumber++;

        // Добор карт
        if (HandManager.Instance != null)
            HandManager.Instance.DrawCardsToMax(5);

        // Активируем карты игрока
        ActivateCardsForTurn(true);

        OnTurnStart?.Invoke(TurnOwner.Player);
        Debug.Log($"=== Ход ИГРОКА {turnNumber} ===");
        Debug.Log($"Мана: {playerCurrentMana}/{playerMaxMana}");
    }

    private void StartEnemyTurn()
    {
        currentTurn = TurnOwner.Enemy;

        // Активируем карты врага
        ActivateCardsForTurn(false);

        OnTurnStart?.Invoke(TurnOwner.Enemy);
        Debug.Log($"=== Ход ВРАГА {turnNumber} ===");
        Debug.Log($"Мана врага: {enemyCurrentMana}/{enemyMaxMana}");

        // ЗАКОММЕНТИРОВАНО: здесь потом будет логика ИИ
        // EnemyAIPlay();

        // Для теста сразу возвращаем ход игроку через 2 секунды
        endTurnButton.interactable = false;
        Invoke(nameof(EndTurn), 2f);
    }

    private void ActivateCardsForTurn(bool isPlayer)
    {
        if (FieldManager.Instance == null) return;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                CardInstance card = FieldManager.Instance.GetCard(isPlayer, row, col);
                if (card != null)
                {
                    card.canAttackThisTurn = true;
                    Debug.Log($"Карта {card.cardData?.cardName} может атаковать");
                }
            }
        }
    }

    public bool SpendMana(bool isPlayer, int amount)
    {
        if (isPlayer)
        {
            if (playerCurrentMana >= amount)
            {
                playerCurrentMana -= amount;
                Debug.Log($"Потрачено {amount} маны. Осталось: {playerCurrentMana}");
                return true;
            }
            else
            {
                Debug.Log($"Недостаточно маны! Есть: {playerCurrentMana}, нужно: {amount}");
                return false;
            }
        }
        else
        {
            if (enemyCurrentMana >= amount)
            {
                enemyCurrentMana -= amount;
                return true;
            }
            return false;
        }
    }

    public void DamagePlayer(bool isPlayer, int amount)
    {
        if (isPlayer)
        {
            playerHealth -= amount;
            Debug.Log($"Игрок получил {amount} урона. Осталось: {playerHealth}");
            if (playerHealth <= 0)
                GameOver(false);
        }
        else
        {
            enemyHealth -= amount;
            Debug.Log($"Противник получил {amount} урона. Осталось: {enemyHealth}");
            if (enemyHealth <= 0)
                GameOver(true);
        }
    }

    private void GameOver(bool playerWon)
    {
        Debug.Log(playerWon ? "🎉 ПОБЕДА!" : "💀 ПОРАЖЕНИЕ!");
    }
}