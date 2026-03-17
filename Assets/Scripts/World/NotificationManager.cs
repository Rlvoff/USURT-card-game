using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Настройки уведомлений")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private int maxNotifications = 4;
    [SerializeField] private float fadeOutTime = 0.5f;

    private Queue<GameObject> activeNotifications = new Queue<GameObject>();

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

    // метод создания уведомлений
    private void ShowNotification(string message, Sprite icon)
    {
        if (notificationPrefab == null) return;

        GameObject notif = Instantiate(notificationPrefab, notificationContainer);
        NotificationItem item = notif.GetComponent<NotificationItem>();
        if (item != null)
            item.Setup(message, icon);

        // Добавляем в очередь
        activeNotifications.Enqueue(notif);

        // Если превышен лимит - удаляем самое старое с анимацией
        if (activeNotifications.Count > maxNotifications)
        {
            GameObject oldest = activeNotifications.Dequeue();
            if (oldest != null)
            {
                StartCoroutine(FadeOutAndDestroy(oldest));
            }
        }

        // Запускаем автоудаление с анимацией
        StartCoroutine(AutoRemove(notif));
    }

    private IEnumerator AutoRemove(GameObject notif)
    {
        yield return new WaitForSeconds(notificationDuration);

        if (notif != null && activeNotifications.Contains(notif))
        {
            // Удаляем из очереди
            var tempList = new List<GameObject>(activeNotifications);
            tempList.Remove(notif);
            activeNotifications = new Queue<GameObject>(tempList);

            // Запускаем анимацию затухания
            yield return StartCoroutine(FadeOutAndDestroy(notif));
        }
    }

    private IEnumerator FadeOutAndDestroy(GameObject notif)
    {
        if (notif == null) yield break;

        CanvasGroup canvasGroup = notif.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = notif.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            yield return null;
        }

        Destroy(notif);
    }

    // Публичные методы - все используют ShowNotification
    public void ShowItemMessage(string message, Sprite itemIcon)
    {
        ShowNotification(message, itemIcon);
    }

    public void ShowCardMessage(string message, Sprite cardIcon)
    {
        ShowNotification(message, cardIcon);
    }

    public void ShowQuestMessage(string message)
    {
        ShowNotification(message, null);
    }

    public void SetNotificationsVisible(bool visible)
    {
        if (notificationContainer != null)
            notificationContainer.gameObject.SetActive(visible);
    }
}