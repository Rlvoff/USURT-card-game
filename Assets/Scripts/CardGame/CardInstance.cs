using System.Collections.Generic;
using UnityEngine;
using static CardGameManager;

public class CardInstance : MonoBehaviour
{
    [HideInInspector] public CardData cardData;

    // Динамическое состояние
    public int currentHealth;
    public int currentAttack;
    public bool canAttackThisTurn = false;
    public bool isProtecting = false;
    public int currentRow; // 0-1 (передний/задний ряд)
    public int currentColumn; // 0-3 (колонка)

    [HideInInspector] public CardSlot currentSlot;

    public void Initialize(CardData data, int row, int column)
    {
        cardData = data;
        currentRow = row;
        currentColumn = column;

        if (data is AttackingCardData attackingCard)
        {
            currentHealth = attackingCard.defensePoints;
            currentAttack = attackingCard.attackPoints;
            isProtecting = false;
        }
        else if (data is ProtectingCardData protectingCard)
        {
            currentHealth = protectingCard.defensePoints;
            currentAttack = 0;
            isProtecting = true;
        }
        // EffectCardData не имеет здоровья/атаки
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (cardData is ProtectingCardData protectingCard)
            ActivateDeathEffect(protectingCard);
        if (currentSlot != null) currentSlot.RemoveCard();
        else Destroy(gameObject);
    }

    private void ActivateDeathEffect(ProtectingCardData card)
    {
        // Логика эффекта смерти
    }

    // Проверяет, одинаковый ли вид у этой карты и другой карты
    public bool IsSameSpeciesAs(CardInstance other)
    {
        if (cardData == null || other.cardData == null) return false;
        return cardData.species == other.cardData.species;
    }

    // Проверяет, принадлежит ли карта конкретному виду
    public bool IsSpecies(CardSpecies species)
    {
        return cardData != null && cardData.species == species;
    }

    private IDamageable GetPlayer()
    {
        if (currentSlot != null)
        {
            return currentSlot.IsPlayerSlot() ?
                (IDamageable)PlayerController.Instance :
                (IDamageable)EnemyController.Instance;
        }
        return null;
    }

    private List<CardInstance> GetTargets(EffectTrigger trigger)
    {
        List<CardInstance> targets = new List<CardInstance>();

        // Базовая логика: все карты противника
        if (FieldManager.Instance != null)
        {
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    var card = FieldManager.Instance.GetCard(false, r, c);
                    if (card != null)
                        targets.Add(card);
                }
            }
        }

        return targets;
    }

    public void ExecuteEffects(EffectTrigger trigger)
    {
        if (cardData?.effects == null) return;

        foreach (var effect in cardData.effects)
        {
            if (effect == null) continue;

            var context = new EffectContext
            {
                sourceCard = this,
                sourcePlayer = GetPlayer(), // нужен метод GetPlayer()
                field = FieldManager.Instance,
                targetCards = GetTargets(trigger), // нужен метод GetTargets()
                trigger = trigger
            };
            effect.Execute(context);
        }
    }

    // Вызывать в нужных местах:
    public void OnPlay() => ExecuteEffects(EffectTrigger.OnPlay);
    public void OnDeath() => ExecuteEffects(EffectTrigger.OnDeath);
    public void OnTurnStart() => ExecuteEffects(EffectTrigger.OnTurnStart);
    public void OnTurnEnd() => ExecuteEffects(EffectTrigger.OnTurnEnd);
}