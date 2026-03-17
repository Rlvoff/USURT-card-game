using UnityEngine;
using UnityEngine.EventSystems;

public class DontDestroy : MonoBehaviour
{
    private static Canvas canvasInstance;
    private static EventSystem eventSystemInstance;

    // Сбрасываем статические поля при перезагрузке сцены в редакторе
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        canvasInstance = null;
        eventSystemInstance = null;
    }

    private void Awake()
    {
        // Определяем тип объекта
        Canvas canvas = GetComponent<Canvas>();
        EventSystem eventSystem = GetComponent<EventSystem>();

        if (canvas != null)
        {
            if (canvasInstance == null)
            {
                canvasInstance = canvas;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (eventSystem != null)
        {
            if (eventSystemInstance == null)
            {
                eventSystemInstance = eventSystem;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}