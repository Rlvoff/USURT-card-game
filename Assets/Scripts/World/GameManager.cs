using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Добавляем для работы со сценами

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private InventoryManager inventoryManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке новой сцены сбрасываем ссылки
        player = null;
        playerInteraction = null;
        inventoryManager = null;
    }

    public void SetDialogueActive(bool active)
    {
        // Находим игрока если нужно
        if (player == null)
            player = FindFirstObjectByType<PlayerMovement>();

        // Находим взаимодействие если нужно
        if (playerInteraction == null)
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (player != null)
        {
            player.enabled = !active;
            player.SetRunning(false);
        }

        // Отключаем возможность взаимодействия во время диалога
        if (playerInteraction != null)
            playerInteraction.enabled = !active;

        // Отключаем возможность открыть инвентарь во время диалога
        if (inventoryManager != null)
           inventoryManager.enabled = !active;

        Debug.Log($"Dialogue active: {active}. Player movement: {!active}, Interaction: {!active}, Inventory: {!active}");
    }

    public void SetInventoryActive(bool active)
    {
        // Находим игрока если нужно
        if (player == null)
            player = FindFirstObjectByType<PlayerMovement>();

        // Находим взаимодействие если нужно
        if (playerInteraction == null)
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        if (player != null)
        {
            player.enabled = !active;
            player.SetRunning(false);
        }

        // Отключаем возможность взаимодействия во время открытия инвентаря
        if (playerInteraction != null)
            playerInteraction.enabled = !active;

        Debug.Log($"Player movement: {!active}, Interaction: {!active}, Notifications visible: {!active}");

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.SetNotificationsVisible(!active);
    }

    public void SetMenuActive(bool active)
    {
        // Находим игрока если нужно
        if (player == null)
            player = FindFirstObjectByType<PlayerMovement>();

        if (playerInteraction == null)
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (player != null)
            player.enabled = !active;

        if (playerInteraction != null)
            playerInteraction.enabled = !active;

        if (inventoryManager != null)
            inventoryManager.enabled = !active;

        Debug.Log($"Menu active: {active}. Player enabled: {!active}");
    }
}