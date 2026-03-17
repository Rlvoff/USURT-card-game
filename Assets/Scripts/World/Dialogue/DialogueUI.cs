using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement; // Добавляем для работы со сценами

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private GameObject dialogueGroup;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject speakerNamePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Настройки текста")]
    [SerializeField] private float typingSpeed = 0.05f; // Cкорость печати

    private NPC currentNPC;
    private DialogueNode currentNode;
    private List<Button> choiceButtons = new List<Button>();
    private GameInput gameInput;

    private Coroutine typingCoroutine;
    private string currentFullText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        dialogueGroup.SetActive(false);
        dialoguePanel.SetActive(false);
        speakerNamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Отписываемся от событий при уничтожении
        if (gameInput != null && gameInput.PlayerInputActions != null)
        {
            gameInput.PlayerInputActions.Player.Cancel.performed -= OnCancelPerformed;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке новой сцены обновляем ссылку на GameInput
        gameInput = GameInput.Instance;
    }

    private void Start()
    {
        gameInput = GameInput.Instance;
    }

    private void OnCancelPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log("Cancel pressed! Dialogue active: " + (dialoguePanel != null && dialoguePanel.activeSelf));

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            Debug.Log("Ending dialogue...");
            EndDialogue();
        }
    }

    public void StartDialogue(NPC npc)
    {
        if (npc == null)
        {
            Debug.LogError("NPC is null!");
            return;
        }

        currentNPC = npc;
        currentNode = npc.GetStartNode();

        if (currentNode == null)
        {
            Debug.LogError($"NPC {npc.name} has no start node!");
            return;
        }

        dialogueGroup.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCurrentNode();

        // Останавливаем движение игрока
        if (GameManager.Instance != null)
            GameManager.Instance.SetDialogueActive(true);

        // Подписываемся на отмену диалога
        if (gameInput != null)
        {
            gameInput.PlayerInputActions.Player.Cancel.performed += OnCancelPerformed;
        }
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        // Отображаем имя говорящего
        if (speakerNameText != null)
        {
            speakerNameText.text = currentNode.speakerName ?? "NPC";
            speakerNamePanel.SetActive(true);
        }

        // Сохраняем полный текст
        currentFullText = currentNode.dialogueText ?? "...";

        // Если есть выборы - показываем кнопки выбора
        if (currentNode.choices != null && currentNode.choices.Length > 0)
        {
            ShowChoices(currentNode.choices);
        }
        else
        {
            // Если нет выборов - завершаем диалог
            EndDialogue();
        }

        // Останавливаем предыдущую корутину, если была
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Запускаем печать текста
        typingCoroutine = StartCoroutine(TypeText());
    }

    // Корутина для печати текста
    private IEnumerator TypeText()
    {
        dialogueText.text = ""; // Очищаем текст

        // Печатаем по одной букве
        foreach (char c in currentFullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    private void ShowChoices(DialogueNode.Choice[] choices)
    {
        // Очищаем старые кнопки
        HideChoices();

        if (choices == null || choices.Length == 0)
        {
            Debug.LogWarning("No choices to show");
            return;
        }

        if (choicesPanel == null)
        {
            Debug.LogError("ChoicesPanel is not assigned!");
            return;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("ChoiceButtonPrefab is not assigned!");
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == null)
            {
                Debug.LogWarning($"Choice at index {i} is null, skipping");
                continue;
            }

            if (string.IsNullOrEmpty(choices[i].choiceText))
            {
                Debug.LogWarning($"Choice at index {i} has no text, skipping");
                continue;
            }

            int index = i;
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);

            // Находим текст на кнопке
            TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choices[i].choiceText;
            }
            else
            {
                Debug.LogError("Button prefab has no TextMeshProUGUI component!");
            }

            // Добавляем обработчик
            choiceButton.onClick.AddListener(() => OnChoiceSelected(choices[index]));
            choiceButtons.Add(choiceButton);
        }

        // Если после фильтрации не осталось кнопок
        if (choiceButtons.Count == 0)
        {
            Debug.LogWarning("No valid choices after filtering");
            EndDialogue();
            return;
        }

        choicesPanel.SetActive(true);
    }

    private void HideChoices()
    {
        foreach (var button in choiceButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        choiceButtons.Clear();

        if (choicesPanel != null)
            choicesPanel.SetActive(false);
    }

    private void OnChoiceSelected(DialogueNode.Choice choice)
    {
        if (choice == null)
        {
            Debug.LogError("Selected choice is null!");
            EndDialogue();
            return;
        }

        // Выполняем действие, связанное с выбором
        try
        {
            choice.onChoiceSelected?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error invoking choice event: {e.Message}");
        }

        // Переходим к следующему узлу
        currentNode = choice.nextNode;
        ShowCurrentNode();
    }

    public void EndDialogue()
    {
        // Останавливаем печать, если идёт
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (speakerNamePanel != null)
            speakerNamePanel.SetActive(false);

        // Отписываемся от событий
        if (gameInput != null && gameInput.PlayerInputActions != null)
        {
            gameInput.PlayerInputActions.Player.Cancel.performed -= OnCancelPerformed;
        }

        currentNPC = null;
        currentNode = null;
        HideChoices();

        if (GameManager.Instance != null)
            GameManager.Instance.SetDialogueActive(false);
    }

    public bool IsDialogueActive()
    {
        return dialogueGroup != null && dialogueGroup.activeSelf;
    }
}