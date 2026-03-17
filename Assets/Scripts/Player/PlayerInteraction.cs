using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private float checkInterval = 0.2f;

    private PlayerMovement playerMovement;
    private GameInput gameInput;
    private IInteractable currentInteractable;
    private float checkTimer;

    private void Start() // Если Awake то срабатывает до GameInput
    {
        playerMovement = GetComponent<PlayerMovement>();
        GameInput.Instance.PlayerInputActions.Player.Interact.performed += OnInteractPerformed; // Подписываемся на событие нажатия E                                                                                            
    }

    private void OnEnable()
    {
        // Подписываемся на событие нажатия E
        if (GameInput.Instance != null)
        {
            GameInput.Instance.PlayerInputActions.Player.Interact.performed += OnInteractPerformed;
        }

        // Сбрасываем текущий объект, чтобы принудительно перепроверить
        currentInteractable = null;
    }

    private void OnDisable()
    {
        // Отписываемся от события
        if (GameInput.Instance != null)
        {
            GameInput.Instance.PlayerInputActions.Player.Interact.performed -= OnInteractPerformed;
        }

        // Прячем подсказку
        InteractionPrompt.Instance?.HidePrompt();
    }

    private void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        TryInteract();
    }

    private void Update()
    {
        // Только проверка объектов, никакого ввода здесь!
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            CheckForInteractable();
        }
    }
    private void TryInteract()
    {
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement not found!");
            return;
        }

        Vector2 direction = GetFacingDirection();
        Vector2 rayOrigin = transform.position;

        Debug.DrawRay(rayOrigin, direction * interactionDistance, Color.red, 2f);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, interactionDistance, interactableLayers);

        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log("Interactable found! Name: " + hit.collider.gameObject.name);
                interactable.Interact(playerMovement);
            }
            else
            {
                Debug.Log("No IInteractable component on " + hit.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Nothing hit");
        }
    }

    private Vector2 GetFacingDirection()
    {
        if (playerMovement.IsUp()) return Vector2.up;
        if (playerMovement.IsDown()) return Vector2.down;
        if (playerMovement.IsLeft()) return Vector2.left;
        if (playerMovement.IsRight()) return Vector2.right;
        return Vector2.down;
    }

    private void CheckForInteractable()
    {
        Vector2 direction = GetFacingDirection();
        Vector2 rayOrigin = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, interactionDistance, interactableLayers);

        IInteractable newInteractable = null;

        if (hit.collider != null)
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        // Если объект изменился
        if (newInteractable != currentInteractable)
        {
            // Прячем старую подсказку
            if (currentInteractable != null)
            {
                InteractionPrompt.Instance?.HidePrompt();
            }

            currentInteractable = newInteractable;

            // Показываем новую подсказку
            if (currentInteractable != null && currentInteractable.CanInteract(playerMovement))
            {
                string prompt = currentInteractable.GetInteractionPrompt();
                InteractionPrompt.Instance?.ShowPrompt(prompt);
            }
        }
    }
}