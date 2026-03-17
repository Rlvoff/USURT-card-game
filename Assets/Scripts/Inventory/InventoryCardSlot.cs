using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCardSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler // ДОБАВЛЕНО IPointerClickHandler
{
    [Header("Элементы UI")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Image manaBackground;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image attackBackground;
    [SerializeField] private TextMeshProUGUI defenceText;
    [SerializeField] private Image defenceBackground;
    [SerializeField] private TextMeshProUGUI phrazeText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private Image speciesBackground;

    private CardData currentCardData;
    private bool isDeckCard = false; // true если карта в панели колоды

    private void Awake()
    {
        // Кэшируем компоненты, если они не заданы в инспекторе
        CacheComponents();
        ConfigureRaycastTargets();
    }

    private void CacheComponents()
    {
        // Если компоненты не заданы, ищем их на объекте или в детях
        if (cardBackground == null)
            cardBackground = GetComponent<Image>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();

        if (descriptionText == null)
            descriptionText = GetComponentInChildren<TextMeshProUGUI>();

        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (manaText == null)
            manaText = GetComponentInChildren<TextMeshProUGUI>();

        if (manaBackground == null)
            manaBackground = GetComponentInChildren<Image>();

        if (attackText == null)
            attackText = GetComponentInChildren<TextMeshProUGUI>();

        if (attackBackground == null)
            attackBackground = GetComponentInChildren<Image>();

        if (defenceText == null)
            defenceText = GetComponentInChildren<TextMeshProUGUI>();

        if (defenceBackground == null)
            defenceBackground = GetComponentInChildren<Image>();

        if (phrazeText == null)
            phrazeText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(CardData cardData)
    {
        currentCardData = cardData;

        if (cardData == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        // Базовая информация доступна у всех карт
        if (nameText != null)
            nameText.text = cardData.cardName;

        if (iconImage != null && cardData.icon != null)
            iconImage.sprite = cardData.icon;

        if (descriptionText != null && cardData.description != null)
            descriptionText.text = cardData.description;

        if (manaText != null)
            manaText.text = cardData.manaCost.ToString();

        if (phrazeText != null)
            phrazeText.text = cardData.phraze;

        // Устанавливаем текст и видимость вида
        bool showSpecies = cardData.species != CardSpecies.Neutral;

        if (speciesText != null)
        {
            speciesText.gameObject.SetActive(showSpecies);
            if (showSpecies)
            {
                switch (cardData.species)
                {
                    case CardSpecies.Student:
                        speciesText.text = "Студент";
                        break;
                    case CardSpecies.Teacher:
                        speciesText.text = "Преподаватель";
                        break;
                }
            }
        }

        if (speciesBackground != null)
            speciesBackground.gameObject.SetActive(showSpecies);

        // Сначала включаем/выключаем элементы в зависимости от типа карты
        bool showAttack = true;
        bool showDefense = true;

        // Проверяем тип карты и устанавливаем флаги
        if (cardData is AttackingCardData)
        {
            showAttack = true;
            showDefense = true;
        }
        else if (cardData is ProtectingCardData)
        {
            showAttack = false;
            showDefense = true;
        }
        else // EffectCardData
        {
            showAttack = false;
            showDefense = false;
        }

        // Управляем видимостью элементов
        if (attackBackground != null)
            attackBackground.gameObject.SetActive(showAttack);

        if (attackText != null)
            attackText.gameObject.SetActive(showAttack);

        if (defenceBackground != null)
            defenceBackground.gameObject.SetActive(showDefense);

        if (defenceText != null)
            defenceText.gameObject.SetActive(showDefense);

        // Устанавливаем значения
        if (cardData is AttackingCardData attackingCard)
        {
            if (attackText != null)
                attackText.text = attackingCard.attackPoints.ToString();
            if (defenceText != null)
                defenceText.text = attackingCard.defensePoints.ToString();
        }
        else if (cardData is ProtectingCardData protectingCard)
        {
            if (defenceText != null)
                defenceText.text = protectingCard.defensePoints.ToString();
        }

        // Устанавливаем цвета для фона карты
        if (cardBackground != null)
        {
            if (cardData is AttackingCardData)
            {
                cardBackground.color = new Color(0.9f, 0.5f, 0.5f, 1f);
            }
            else if (cardData is ProtectingCardData)
            {
                cardBackground.color = new Color(0.5f, 0.7f, 0.9f, 1f);
            }
            else if (cardData is EffectCardData)
            {
                cardBackground.color = new Color(0.8f, 0.8f, 0.5f, 1f);
            }

            // Устанавливаем цвет speciesBackground такой же как у cardBackground, если вид показывается
            if (speciesBackground != null && showSpecies)
                speciesBackground.color = cardBackground.color;
        }
    }

    // Установка флага принадлежности к колоде
    public void SetAsDeckCard(bool value)
    {
        isDeckCard = value;
    }

    private void ConfigureRaycastTargets()
    {
        // Отключаем raycast на всех текстах
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            text.raycastTarget = false;
        }
    }

    public void SetInteractable(bool interactable)
    {
        // Отключаем/включаем возможность клика
        enabled = interactable;

        // Можно также затемнить карту для визуального отличия
        if (cardBackground != null)
        {
            Color color = cardBackground.color;
            color.a = interactable ? 1f : 0.5f; // полупрозрачная если неактивна
            cardBackground.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentCardData != null && InventoryPreviewManager.Instance != null)
        {
            InventoryPreviewManager.Instance.ShowPreview(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryPreviewManager.Instance != null)
        {
            InventoryPreviewManager.Instance.HidePreview();
        }
    }

    // Обработка клика для добавления/удаления из колоды
    public void OnPointerClick(PointerEventData eventData)
    {
        // Если скрипт отключён - не реагируем на клики
        if (!enabled) return;

        if (currentCardData == null || InventoryManager.Instance == null) return;

        if (isDeckCard)
        {
            InventoryManager.Instance.RemoveCardFromDeck(currentCardData.id);
        }
        else
        {
            InventoryManager.Instance.AddCardToDeck(currentCardData.id);
        }
    }

    // Публичные свойства для доступа из менеджера превью
    public GameObject GameObject => gameObject;
    public Image CardBackground => cardBackground;
    public Image IconImage => iconImage;
    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI ManaText => manaText;
    public Image ManaBackground => manaBackground;
    public TextMeshProUGUI AttackText => attackText;
    public Image AttackBackground => attackBackground;
    public TextMeshProUGUI DefenceText => defenceText;
    public Image DefenceBackground => defenceBackground;
}