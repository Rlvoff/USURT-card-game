using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ThreeStepStair : MonoBehaviour
{
    [Header("Настройки ступеней")]
    public float step1Y = 0.2f; // Y координата 1 ступени
    public float step2Y = 0.4f; // Y координата 2 ступени
    public float step3Y = 0.6f; // Y координата 3 ступени

    [Header("Слои")]
    public int groundLayer = 1;
    public int step1Layer = 2;
    public int step2Layer = 3;
    public int step3Layer = 4;

    private Transform player;
    private SpriteRenderer playerSprite;
    private int currentStep = 0; // 0-пол, 1,2,3-ступени
    private bool isMoving = false;
    private bool playerInStairZone = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
            playerSprite = player.GetComponentInChildren<SpriteRenderer>();

        // Создаем зону входа (маленький триггер внизу)
        CreateEntryTrigger();
    }

    void CreateEntryTrigger()
    {
        GameObject triggerObj = new GameObject("StairEntry");
        triggerObj.transform.parent = transform;
        triggerObj.transform.localPosition = new Vector3(0, -0.4f, 0);

        BoxCollider2D trigger = triggerObj.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.8f, 0.2f);

        StairEntry entry = triggerObj.AddComponent<StairEntry>();
        entry.parentStair = this;
    }

    void Update()
    {
        if (player == null || isMoving || !playerInStairZone) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Отладка по F1
        if (keyboard.f1Key.wasPressedThisFrame)
        {
            Debug.Log($"Ступень: {currentStep}, Позиция Y: {player.position.y}");
        }

        // Подъем на следующую ступень
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            if (currentStep < 3)
            {
                StartCoroutine(MoveToStep(currentStep + 1));
            }
        }

        // Спуск на предыдущую ступень
        if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
        {
            if (currentStep > 1)
            {
                StartCoroutine(MoveToStep(currentStep - 1));
            }
            else if (currentStep == 1)
            {
                StartCoroutine(MoveToGround());
            }
        }
    }

    // Вызывается из триггера входа
    public void PlayerEnteredStair()
    {
        if (!playerInStairZone)
        {
            playerInStairZone = true;
            currentStep = 1;

            // Поднимаем на первую ступень
            Vector3 pos = player.position;
            pos.y = transform.position.y + step1Y;
            player.position = pos;

            if (playerSprite != null)
                playerSprite.sortingOrder = step1Layer;

            Debug.Log("На лестнице, ступень 1");
        }
    }

    // Вызывается при выходе
    public void PlayerExitedStair()
    {
        playerInStairZone = false;
        currentStep = 0;
        Debug.Log("Вышел с лестницы");
    }

    IEnumerator MoveToStep(int targetStep)
    {
        isMoving = true;

        // Блокируем движение игрока
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Vector3 startPos = player.position;
        Vector3 targetPos = startPos;
        int targetLayer = step1Layer;

        // Определяем целевую позицию
        switch (targetStep)
        {
            case 1:
                targetPos.y = transform.position.y + step1Y;
                targetLayer = step1Layer;
                Debug.Log("→ На 1 ступень");
                break;
            case 2:
                targetPos.y = transform.position.y + step2Y;
                targetLayer = step2Layer;
                Debug.Log("→ На 2 ступень");
                break;
            case 3:
                targetPos.y = transform.position.y + step3Y;
                targetLayer = step3Layer;
                Debug.Log("→ На 3 ступень");
                break;
        }

        // Плавное движение
        float moveTime = 0.2f;
        float elapsed = 0;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            player.position = Vector3.Lerp(startPos, targetPos, t);

            // Меняем слой в середине движения
            if (t > 0.4f && playerSprite != null)
            {
                playerSprite.sortingOrder = targetLayer;
            }

            yield return null;
        }

        // Точная фиксация позиции
        player.position = targetPos;
        if (playerSprite != null)
            playerSprite.sortingOrder = targetLayer;

        currentStep = targetStep;

        // Разблокируем движение
        if (movement != null) movement.enabled = true;
        isMoving = false;
    }

    IEnumerator MoveToGround()
    {
        isMoving = true;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Vector3 startPos = player.position;
        Vector3 targetPos = startPos;
        targetPos.y = transform.position.y; // На пол

        Debug.Log("→ На пол");

        float moveTime = 0.2f;
        float elapsed = 0;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveTime;

            player.position = Vector3.Lerp(startPos, targetPos, t);

            if (t > 0.4f && playerSprite != null)
            {
                playerSprite.sortingOrder = groundLayer;
            }

            yield return null;
        }

        player.position = targetPos;
        if (playerSprite != null)
            playerSprite.sortingOrder = groundLayer;

        currentStep = 0;
        playerInStairZone = false;

        if (movement != null) movement.enabled = true;
        isMoving = false;
    }

    void OnDrawGizmos()
    {
        Vector3 pos = transform.position;

        // Рисуем ступени
        float width = 1f;

        // 1 ступень
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(new Vector3(pos.x, pos.y + step1Y, 0), new Vector3(width, 0.05f, 0));

        // 2 ступень
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(new Vector3(pos.x, pos.y + step2Y, 0), new Vector3(width, 0.05f, 0));

        // 3 ступень
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawCube(new Vector3(pos.x, pos.y + step3Y, 0), new Vector3(width, 0.05f, 0));

        // Зона входа
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawCube(new Vector3(pos.x, pos.y - 0.4f, 0), new Vector3(0.8f, 0.2f, 0));

        // Подписи
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(new Vector3(pos.x + 0.6f, pos.y + step1Y, 0), "1");
        UnityEditor.Handles.Label(new Vector3(pos.x + 0.6f, pos.y + step2Y, 0), "2");
        UnityEditor.Handles.Label(new Vector3(pos.x + 0.6f, pos.y + step3Y, 0), "3");
#endif
    }
}

// Скрипт для триггера входа
public class StairEntry : MonoBehaviour
{
    public ThreeStepStair parentStair;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && parentStair != null)
        {
            parentStair.PlayerEnteredStair();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && parentStair != null)
        {
            parentStair.PlayerExitedStair();
        }
    }
}