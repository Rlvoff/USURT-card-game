using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    public static CardGameManager Instance { get; private set; }

    [SerializeField] private TurnManager turnManager;
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private DeckManager deckManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Инициализация колоды
        if (InventoryManager.Instance != null)
        {
            List<InventoryManager.DeckCard> playerDeckCards = InventoryManager.Instance.GetDeckCards();
            deckManager.InitializeDeck(playerDeckCards);

            Debug.Log($"Загружено {playerDeckCards.Count} карт из колоды игрока");
        }

        // Начальная рука
        handManager.DrawCardsToMax(5);
    }

    public interface IDamageable
    {
        void TakeDamage(int damage);
    }

    public class PlayerController : MonoBehaviour, IDamageable
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private int health = 30;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            Debug.Log($"Игрок получил {damage} урона. Осталось: {health}");

            if (health <= 0)
                Debug.Log("Игрок проиграл!");
        }
    }


    public class EnemyController : MonoBehaviour, IDamageable
    {
        public static EnemyController Instance { get; private set; }

        [SerializeField] private int health = 30;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            Debug.Log($"Враг получил {damage} урона. Осталось: {health}");

            if (health <= 0)
                Debug.Log("Игрок победил!");
        }
    }
}