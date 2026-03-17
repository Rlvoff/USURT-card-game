using UnityEngine;
using System.Collections.Generic;

// Перечисление статусов квеста
public enum QuestStatus
{
    NotStarted,
    InProgress,
    ReadyToComplete,
    Completed
}

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestData> quests;
}

[System.Serializable]
public class QuestData
{
    [Header("Основная информация")]
    public string questId;              // уникальный ID
    public string questName;             // название квеста
    [TextArea(3, 5)] public string description; // описание
}