using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [Header("UI элементы")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float fadeSpeed = 5f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private bool isVisible = false;
    private bool isHiding = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvasGroup = promptPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = promptPanel.AddComponent<CanvasGroup>();

        promptPanel.SetActive(false);
        canvasGroup.alpha = 1f;
    }

    public void ShowPrompt(string text)
    {
        if (promptPanel == null) return;

        isVisible = true;
        isHiding = false;

        // Обновляем текст
        if (promptText != null)
            promptText.text = text;

        // Останавливаем текущую анимацию
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (gameObject.activeInHierarchy)
        {
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        else
        {
            // Если объект неактивен, просто сразу показываем
            promptPanel.SetActive(true);
            canvasGroup.alpha = 1f;
        }
    }

    public void HidePrompt()
    {
        if (promptPanel == null) return;

        isVisible = false;
        isHiding = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (gameObject.activeInHierarchy)
        {
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            // Если объект неактивен, просто сразу скрываем
            promptPanel.SetActive(false);
            canvasGroup.alpha = 1f;
        }
    }

    public void SetVisible(bool visible)
    {
        if (promptPanel == null) return;

        // Отменяем текущую анимацию
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (visible)
        {
            isVisible = true;
            isHiding = false;
            promptPanel.SetActive(true);
            canvasGroup.alpha = 1f;
        }
        else
        {
            isVisible = false;
            isHiding = false;
            promptPanel.SetActive(false);
            canvasGroup.alpha = 1f;
        }
    }

    private IEnumerator FadeIn()
    {
        promptPanel.SetActive(true);

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }

        promptPanel.SetActive(false);
        canvasGroup.alpha = 1f;
    }
}