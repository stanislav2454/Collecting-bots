using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private ItemType _spawnItemType = ItemType.Resource;
    [SerializeField] private bool _useSpecificItem = false;
    [SerializeField] private ItemData _specificItemData;

    [Header("Visual Settings")]
    [SerializeField] private bool _debugVisual = true;
    [SerializeField] private Color _gizmoColor = Color.yellow;

    public ItemType SpawnItemType => _spawnItemType;// зачем нужен ? если не используется - удалить !
    public bool UseSpecificItem => _useSpecificItem;// зачем нужен ? если не используется - удалить !
    public ItemData SpecificItemData => _specificItemData;// зачем нужен ? если не используется - удалить !
    public Vector3 Position => transform.position;

    public bool CanSpawnItem(ItemData itemData)
    {
        if (_useSpecificItem && _specificItemData != null)
            return itemData.itemId == _specificItemData.itemId;
        else
            return itemData.itemType == _spawnItemType;
    }

    public ItemData GetPreferredItemData()
    {
        if (_useSpecificItem && _specificItemData != null)
            return _specificItemData;

        return null;
    }

    private string GetIconName()
    {
        if (_useSpecificItem && _specificItemData != null)
            return "ItemIcon";

        switch (_spawnItemType)
        {
            case ItemType.Resource: return "Prefab Icon";
            case ItemType.Treasure: return "Favorite Icon";
            case ItemType.Special: return "GameObject Icon";
            default: return "ItemIcon";
        }
    }

    private string GetSpawnPointLabel()
    {
        if (_useSpecificItem && _specificItemData != null)
            return _specificItemData.itemName;

        return _spawnItemType.ToString();
    }

    private void OnDrawGizmos()
    {
        if (_debugVisual == false)
            return;

        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        string iconName = GetIconName();
        Gizmos.DrawIcon(transform.position + Vector3.up * 4, iconName, true, _gizmoColor);

#if UNITY_EDITOR
        string label = GetSpawnPointLabel();

        Color originalColor = GUI.color;
        GUI.color = Color.blue;

        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, label);

        GUI.color = originalColor;
#endif
    }
}