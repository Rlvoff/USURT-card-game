using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Inventory/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<AttackingCardData> attackingCards;
    public List<ProtectingCardData> protectingCards;
    public List<EffectCardData> effectCards;
}

public enum CardSpecies
{
    Neutral,     // Нейтральные
    Student,     // Студенты
    Teacher      // Преподаватели
}

[System.Serializable]
public abstract class CardData
{
    public string cardName;
    public int id;
    public Sprite icon;
    public int manaCost;
    [TextArea] public string description;
    public string phraze;
    public CardSpecies species;
    public List<CardEffect> effects;
}

[System.Serializable]
public class AttackingCardData : CardData
{
    public int attackPoints;
    public int defensePoints;

    public AttackingCardData(string name, int id, Sprite icon, int mana, int attack, int defense, string description, string phraze, CardSpecies species, List<CardEffect> effects = null)
    {
        this.cardName = name;
        this.id = id;
        this.icon = icon;
        this.manaCost = mana;
        this.attackPoints = attack;
        this.defensePoints = defense;
        this.description = description;
        this.phraze = phraze;
        this.species = species;
        this.effects = effects ?? new List<CardEffect>();
    }
}

[System.Serializable]
public class ProtectingCardData : CardData
{
    public int defensePoints;

    public ProtectingCardData(string name, int id, Sprite icon, int mana, int defense, string description, string phraze, CardSpecies species, List<CardEffect> effects = null)
    {
        this.cardName = name;
        this.id = id;
        this.icon = icon;
        this.manaCost = mana;
        this.defensePoints = defense;
        this.description = description;
        this.phraze = phraze;
        this.species = species;
        this.effects = effects ?? new List<CardEffect>();
    }
}

[System.Serializable]
public class EffectCardData : CardData
{
    public EffectCardData(string name, int id, Sprite icon, int mana, string description, string phraze, CardSpecies species, List<CardEffect> effects = null)
    {
        this.cardName = name;
        this.id = id;
        this.icon = icon;
        this.manaCost = mana;
        this.description = description;
        this.phraze = phraze;
        this.species = species;
        this.effects = effects ?? new List<CardEffect>();
    }
}