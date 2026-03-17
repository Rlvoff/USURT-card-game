using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;

    public void Setup(string message, Sprite icon)
    {
        if (messageText != null)
            messageText.text = message;

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }

            // если иконка не передаётся, то оставляем iconimage из префаба
        }
    }
}