using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Добавляем для работы со сценами

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public PlayerInputActions PlayerInputActions => playerInputActions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerInputActions = new PlayerInputActions();
            playerInputActions.Enable();
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
        // Ничего не делаем, просто обеспечиваем переинициализацию
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public bool IsInteractPressed()
    {
        if (playerInputActions == null) return false;
        return playerInputActions.Player.Interact.WasPressedThisFrame();
    }

    public bool IsCancelPressed()
    {
        if (playerInputActions == null) return false;
        return playerInputActions.Player.Cancel.WasPressedThisFrame();
    }
}