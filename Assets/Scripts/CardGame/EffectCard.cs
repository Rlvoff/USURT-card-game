using System.Collections.Generic;
using UnityEngine;
using static CardGameManager;

public class EffectCard : MonoBehaviour
{
    private CardData cardData;

    public void Setup(CardData data)
    {
        cardData = data;
    }

    public void Play()
    {
        if (cardData == null || cardData.effects == null || cardData.effects.Count == 0)
        {
            Debug.LogError("Нет эффекта у карты!");
            Destroy(gameObject);
            return;
        }

        var cardInstance = GetComponent<CardInstance>();

        // Выполняем все эффекты карты
        foreach (var effect in cardData.effects)
        {
            if (effect == null) continue;

            var context = new EffectContext
            {
                sourceCard = cardInstance,
                sourcePlayer = GetSourcePlayer(),
                field = FieldManager.Instance,
                targetCards = GetTargets(),
                trigger = EffectTrigger.OnPlay
            };

            effect.Execute(context);
        }

        // Уничтожаем карту после использования
        Destroy(gameObject);
    }

    private IDamageable GetSourcePlayer()
    {
        if (TurnManager.Instance == null) return null;

        return TurnManager.Instance.currentTurn == TurnManager.TurnOwner.Player
            ? (IDamageable)PlayerController.Instance
            : (IDamageable)EnemyController.Instance;
    }

    private List<CardInstance> GetTargets()
    {
        var targets = new List<CardInstance>();

        if (FieldManager.Instance == null) return targets;

        // Получаем все карты противника
        for (int r = 0; r < 2; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                var card = FieldManager.Instance.GetCard(false, r, c);
                if (card != null)
                    targets.Add(card);
            }
        }

        return targets;
    }
}