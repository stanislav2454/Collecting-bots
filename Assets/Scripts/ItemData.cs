using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Bot Collector/Item Data")]
[System.Serializable]
public class ItemData : ScriptableObject
{
    public string itemId;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public string itemName;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public ItemType itemType;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public int value = 1;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public float respawnTime = 5f;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public GameObject visualPrefab;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public Color itemColor = Color.white;// модификаторДоступа+именаСчертойИлиБольшойБуквы
}

public enum ItemType
{
    Resource,
    Treasure,
    Special
}