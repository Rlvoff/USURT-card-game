using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance { get; private set; }

    private Dictionary<string, Dictionary<string, string>> sceneStates = new Dictionary<string, Dictionary<string, string>>();

    // НОВОЕ: хранение позиций игрока для каждой сцены
    private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();

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
        SceneManager.sceneUnloaded += OnSceneUnloaded; // Добавляем отслеживание выгрузки сцены
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Восстанавливаем состояние объектов
        StartCoroutine(RestoreNextFrame(scene.name));

        // Восстанавливаем позицию игрока
        if (playerPositions.ContainsKey(scene.name))
        {
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                player.transform.position = playerPositions[scene.name];
                Debug.Log($"Восстановлена позиция игрока в {scene.name}: {playerPositions[scene.name]}");
            }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Сохраняем позицию игрока перед выгрузкой сцены
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            playerPositions[scene.name] = player.transform.position;
            Debug.Log($"Сохранена позиция игрока в {scene.name}: {player.transform.position}");
        }
    }

    private System.Collections.IEnumerator RestoreNextFrame(string sceneName)
    {
        yield return null;
        RestoreSceneState(sceneName);
    }

    public void RegisterChange(IPersistable obj)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneStates.ContainsKey(sceneName))
            sceneStates[sceneName] = new Dictionary<string, string>();

        sceneStates[sceneName][obj.GetUniqueID()] = obj.GetState();
    }

    private void RestoreSceneState(string sceneName)
    {
        if (!sceneStates.ContainsKey(sceneName)) return;

        var states = sceneStates[sceneName];
        var persistables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var obj in persistables)
        {
            if (obj is IPersistable persistable && states.ContainsKey(persistable.GetUniqueID()))
            {
                persistable.SetState(states[persistable.GetUniqueID()]);
            }
        }
    }

    // Метод для принудительного сохранения текущей позиции
    public void SaveCurrentPlayerPosition()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            playerPositions[currentScene] = player.transform.position;
        }
    }
}