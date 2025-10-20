using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Bot Collector/Item Data")]
[System.Serializable]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public int value = 1;
    public float respawnTime = 5f;
    public GameObject visualPrefab;
    public Color itemColor = Color.white;
}

public enum ItemType
{
    Resource,
    Treasure,
    Special
}