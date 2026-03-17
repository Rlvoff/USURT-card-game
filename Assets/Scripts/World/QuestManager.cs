using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; // Добавляем для работы со сценами

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("База данных квестов")]
    [SerializeField] private QuestDatabase questDatabase;

    [Header("Состояния квестов")]
    [SerializeField] private List<QuestState> activeQuests = new List<QuestState>();
    [SerializeField] private List<QuestState> completedQuests = new List<QuestState>();

    public List<string> GetActiveQuests()
    {
        List<string> result = new List<string>();
        foreach (var quest in activeQuests)
        {
            if (!quest.isReadyToComplete)
                result.Add(quest.questId);
        }
        return result;
    }

    public List<string> GetReadyToCompleteQuests()
    {
        List<string> result = new List<string>();
        foreach (var quest in activeQuests)
        {
            if (quest.isReadyToComplete)
                result.Add(quest.questId);
        }
        return result;
    }

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
        // Ничего не делаем, просто обеспечиваем переинициализацию
    }

    public QuestData GetQuestData(string questId)
    {
        return questDatabase?.quests.Find(q => q.questId == questId);
    }

    public QuestStatus GetQuestStatus(string questId)
    {
        if (completedQuests.Any(q => q.questId == questId))
            return QuestStatus.Completed;

        if (activeQuests.Any(q => q.questId == questId))
        {
            var quest = activeQuests.Find(q => q.questId == questId);
            return quest.isReadyToComplete ? QuestStatus.ReadyToComplete : QuestStatus.InProgress;
        }

        return QuestStatus.NotStarted;
    }

    public void StartQuest(string questId)
    {
        if (GetQuestStatus(questId) != QuestStatus.NotStarted)
        {
            Debug.Log($"Quest {questId} already started or completed");
            return;
        }

        activeQuests.Add(new QuestState { questId = questId, isReadyToComplete = false });
        Debug.Log($"Quest started: {questId}");

        // Уведомление о новом квесте
        if (NotificationManager.Instance != null)
        {
            QuestData questData = GetQuestData(questId);
            string questName = questData != null ? questData.questName : questId;
            NotificationManager.Instance.ShowQuestMessage($"Квест '{questName}' начат");
        }
    }

    public void ReadyToCompleteQuest(string questId)
    {
        var quest = activeQuests.Find(q => q.questId == questId);
        if (quest != null)
        {
            quest.isReadyToComplete = true;
            Debug.Log($"Quest ready to complete: {questId}");

            // Уведомление о готовности квеста
            if (NotificationManager.Instance != null)
            {
                QuestData questData = GetQuestData(questId);
                string questName = questData != null ? questData.questName : questId;
                NotificationManager.Instance.ShowQuestMessage($"Квест '{questName}' готов к сдаче");
            }
        }
    }

    public void CompleteQuest(string questId)
    {
        var activeQuest = activeQuests.Find(q => q.questId == questId);
        if (activeQuest == null)
        {
            Debug.Log($"Quest {questId} is not active");
            return;
        }

        activeQuests.Remove(activeQuest);
        completedQuests.Add(new QuestState { questId = questId });

        Debug.Log($"Quest completed: {questId}");

        // Уведомление о завершении квеста
        if (NotificationManager.Instance != null)
        {
            QuestData questData = GetQuestData(questId);
            string questName = questData != null ? questData.questName : questId;
            NotificationManager.Instance.ShowQuestMessage($"Квест '{questName}' выполнен");
        }
    }

    public bool IsQuestCompleted(string questId)
    {
        return completedQuests.Any(q => q.questId == questId);
    }

    public bool IsQuestReadyToComplete(string questId)
    {
        var quest = activeQuests.Find(q => q.questId == questId);
        return quest != null && quest.isReadyToComplete;
    }
}

[System.Serializable]
public class QuestState
{
    public string questId;
    public bool isReadyToComplete;
}