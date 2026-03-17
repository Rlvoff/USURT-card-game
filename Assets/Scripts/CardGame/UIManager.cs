using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI deckText;

    private void Update()
    {
        if (TurnManager.Instance != null)
        {
            if (manaText != null)
                manaText.text = $"Мана: {TurnManager.Instance.playerCurrentMana}/{TurnManager.Instance.playerMaxMana}";

            if (turnText != null)
                turnText.text = TurnManager.Instance.CurrentTurn == TurnManager.TurnOwner.Player ? "Ход Игрока" : "Ход Врага";
        }
    }
}