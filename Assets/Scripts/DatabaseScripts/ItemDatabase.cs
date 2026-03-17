using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;
}

// Перечисление типов (можно оставить для универсальности)
public enum ItemType
{
    Regular,
    Card
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public Sprite icon;
    [TextArea] public string description;
}