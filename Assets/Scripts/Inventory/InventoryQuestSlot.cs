using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryQuestSlot : MonoBehaviour
{
    [Header("Элементы UI")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image statusIndicator;
    [SerializeField] private TextMeshProUGUI statusText;

    private QuestData currentQuestData;
    private QuestStatus currentStatus;

    public void Setup(QuestData questData, QuestStatus status)
    {
        currentQuestData = questData;
        currentStatus = status;

        if (questData == null)
        {
            Debug.LogError("QuestData is null!");
            return;
        }

        // Название квеста
        if (questNameText != null)
            questNameText.text = questData.questName;

        // Описание
        if (descriptionText != null)
            descriptionText.text = questData.description;

        // Настройка индикатора статуса
        UpdateStatusVisual();
    }

    private void UpdateStatusVisual()
    {
        switch (currentStatus)
        {
            case QuestStatus.InProgress:
                SetStatusColor(Color.gray, "Начато");
                break;
            case QuestStatus.ReadyToComplete:
                SetStatusColor(Color.green, "Готово");
                break;
            default:
                SetStatusColor(Color.white, "");
                break;
        }
    }

    private void SetStatusColor(Color color, string statusTextValue)
    {
        if (statusIndicator != null)
            statusIndicator.color = color;

        if (statusText != null)
            statusText.text = statusTextValue;
    }
}