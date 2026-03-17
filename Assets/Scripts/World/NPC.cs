using UnityEngine;
using System.Collections.Generic;

public class NPC : BaseInteractable
{
    [Header("Основные настройки")]
    [SerializeField] private string npcName = "NPC";

    [Header("Обычный диалог (когда нет активных квестов)")]
    [SerializeField] private DialogueNode defaultDialogue;

    [Header("Цепочка квестов")]
    [SerializeField] private List<QuestDialogue> questDialogues; // квесты по порядку

    [System.Serializable]
    public class QuestDialogue
    {
        public string questId;                          // ID квеста
        public DialogueNode notStartedDialogue;         // до взятия
        public DialogueNode inProgressDialogue;         // взят, но не выполнен
        public DialogueNode readyToCompleteDialogue;    // можно сдать
    }

    public DialogueNode GetStartNode()
    {
        if (QuestManager.Instance == null)
            return defaultDialogue;

        // Ищем первый НЕ ВЫПОЛНЕННЫЙ квест в цепочке
        foreach (var questDialogue in questDialogues)
        {
            QuestStatus status = QuestManager.Instance.GetQuestStatus(questDialogue.questId);

            // Если квест ещё не завершён - показываем его диалоги
            if (status != QuestStatus.Completed)
            {
                return GetDialogueForQuest(questDialogue, status);
            }
            // Если завершён - идём к следующему квесту в списке
        }

        // Все квесты выполнены - показываем обычный диалог
        return defaultDialogue;
    }

    private DialogueNode GetDialogueForQuest(QuestDialogue questDialogue, QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.NotStarted:
                return questDialogue.notStartedDialogue ?? defaultDialogue;

            case QuestStatus.InProgress:
                return questDialogue.inProgressDialogue ?? defaultDialogue;

            case QuestStatus.ReadyToComplete:
                return questDialogue.readyToCompleteDialogue ?? defaultDialogue;

            default:
                return defaultDialogue;
        }
    }

    public override void Interact(PlayerMovement player)
    {
        Debug.Log($"Starting dialogue with {npcName}");

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.StartDialogue(this);
        }
    }

    public override string GetInteractionPrompt()
    {
        return $"Поговорить с {npcName} [E]";
    }

}