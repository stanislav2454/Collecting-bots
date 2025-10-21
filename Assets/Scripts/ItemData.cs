using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Bot Collector/Item Data")]
[System.Serializable]
public class ItemData : ScriptableObject
{
    public string itemId;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public string itemName;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public ItemType itemType;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public int value = 1;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public float respawnTime = 5f;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public GameObject visualPrefab;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public Color itemColor = Color.white;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !
}

public enum ItemType
{
    Resource,
    Treasure,
    Special
}