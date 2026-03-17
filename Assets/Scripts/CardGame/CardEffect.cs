using System.Collections.Generic;
using UnityEngine;
using static CardGameManager;

// Условия срабатывания эффекта
public enum EffectTrigger
{
    OnPlay,        // При разыгрывании
    OnDeath,       // При смерти
    OnTurnStart,   // В начале хода
    OnTurnEnd,     // В конце хода
    Continuous,    // Постоянный (аура)
    OnAttack,      // При атаке
    OnDefend       // При защите
}

// Контекст эффекта
public class EffectContext
{
    public CardInstance sourceCard;
    public IDamageable sourcePlayer;
    public List<CardInstance> targetCards = new List<CardInstance>();
    public FieldManager field;
    public EffectTrigger trigger;
}

// Базовый класс эффекта
public abstract class CardEffect : ScriptableObject
{
    public string effectName;
    [TextArea] public string description;
    public abstract void Execute(EffectContext context);
}

// Эффект урона
[CreateAssetMenu(fileName = "DamageEffect", menuName = "CardEffects/Damage")]
public class DamageEffect : CardEffect
{
    public int damageAmount = 3;

    public override void Execute(EffectContext context)
    {
        foreach (var card in context.targetCards)
        {
            if (card != null)
                card.TakeDamage(damageAmount);
        }
    }
}

// Эффект лечения
[CreateAssetMenu(fileName = "HealEffect", menuName = "CardEffects/Heal")]
public class HealEffect : CardEffect
{
    public int healAmount = 3;

    public override void Execute(EffectContext context)
    {
        foreach (var card in context.targetCards)
        {
            if (card != null)
            {
                card.currentHealth += healAmount;
                // Проверка на максимум
                int maxHealth = GetMaxHealth(card);
                if (card.currentHealth > maxHealth)
                    card.currentHealth = maxHealth;
            }
        }
    }

    private int GetMaxHealth(CardInstance card)
    {
        if (card.cardData is AttackingCardData attacking)
            return attacking.defensePoints;
        if (card.cardData is ProtectingCardData protecting)
            return protecting.defensePoints;
        return 0;
    }
}

// Эффект добора карт
[CreateAssetMenu(fileName = "DrawCardEffect", menuName = "CardEffects/DrawCard")]
public class DrawCardEffect : CardEffect
{
    public int cardsToDraw = 1;

    public override void Execute(EffectContext context)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            if (HandManager.Instance != null)
            {
                // Вызываем метод без параметров
                // HandManager.Instance.DrawCard(); 
                Debug.Log($"Добор карты {i + 1}");
            }
        }
    }
}

// Эффект связаный с типами карт
[CreateAssetMenu(fileName = "BuffSameSpeciesEffect", menuName = "CardEffects/BuffSameSpecies")]
public class BuffSameSpeciesEffect : CardEffect
{
    public int attackBuff = 1;
    public EffectTrigger trigger = EffectTrigger.OnPlay; // когда срабатывает

    public override void Execute(EffectContext context)
    {
        // Проверяем, подходит ли условие
        if (context.trigger != trigger) return;

        // Логика эффекта
        foreach (var target in context.targetCards)
        {
            if (target != null && target.IsSameSpeciesAs(context.sourceCard))
            {
                target.currentAttack += attackBuff;
            }
        }
    }
}