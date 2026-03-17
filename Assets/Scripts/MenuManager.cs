using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Главное меню")]
    [SerializeField] private GameObject mainMenuGroup;

    [Header("Другие окна (автоматически)")]
    private List<GameObject> openPanels = new List<GameObject>();

    private bool isMainMenuOpen = false;

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

        if (mainMenuGroup != null)
            mainMenuGroup.SetActive(false);
    }

    private void Start()
    {
        // Подписываемся на глобальное нажатие Esc
        if (GameInput.Instance != null)
        {
            GameInput.Instance.PlayerInputActions.Player.Cancel.performed += OnGlobalCancel;
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.PlayerInputActions.Player.Cancel.performed -= OnGlobalCancel;
        }
    }

    private void OnGlobalCancel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Сначала проверяем диалог
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive())
        {
            DialogueUI.Instance.EndDialogue();
            return;
        }

        // Потом проверяем инвентарь
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen())
        {
            InventoryManager.Instance.CloseInventory();
            return;
        }

        // Если ничего не открыто - открываем главное меню
        ToggleMainMenu();
    }

    public void ToggleMainMenu()
    {
        if (isMainMenuOpen)
        {
            CloseMainMenu();
        }
        else
        {
            OpenMainMenu();
        }
    }

    public void OpenMainMenu()
    {
        if (mainMenuGroup == null) return;

        mainMenuGroup.SetActive(true);
        isMainMenuOpen = true;

        // Принудительно скрываем подсказку без анимации
        if (InteractionPrompt.Instance != null)
            InteractionPrompt.Instance.SetVisible(false);

        // Скрываем превью инвентаря
        if (InventoryPreviewManager.Instance != null)
            InventoryPreviewManager.Instance.HidePreview();

        // Скрываем уведомления
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.SetNotificationsVisible(false);
        }

        // Останавливаем время
        Time.timeScale = 0f;

        // Блокируем ввод игрока
        if (GameManager.Instance != null)
            GameManager.Instance.SetMenuActive(true);
    }

    public void CloseMainMenu()
    {
        if (mainMenuGroup == null) return;

        mainMenuGroup.SetActive(false);
        isMainMenuOpen = false;

        if (InventoryPreviewManager.Instance != null)
            InventoryPreviewManager.Instance.HidePreview();

        // Показываем уведомления
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.SetNotificationsVisible(true);
        }

        // Возвращаем время
        Time.timeScale = 1f;

        // Разблокируем ввод игрока
        if (GameManager.Instance != null)
            GameManager.Instance.SetMenuActive(false);
    }

    public bool IsMainMenuOpen()
    {
        return isMainMenuOpen;
    }
}