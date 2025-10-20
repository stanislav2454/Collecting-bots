using UnityEngine;

public class Item : MonoBehaviour, IItem
{
    [Header("Item Settings")]
    public ItemData itemData;

    [Header("Visuals")]
    public Renderer itemRenderer;
    public Collider itemCollider;

    public string ItemId => itemData?.itemId ?? "unknown";
    public string ItemName => itemData?.itemName ?? "Unknown Item";
    public bool CanBeCollected { get; private set; } = true;

    private void Start()
    {
        InitializeItem();
    }

    private void InitializeItem()
    {
        if (itemRenderer == null)
            itemRenderer = GetComponent<Renderer>();

        if (itemCollider == null)
            itemCollider = GetComponent<Collider>();

        // Устанавливаем цвет предмета из ItemData
        if (itemData != null && itemRenderer != null)
        {
            Material material = itemRenderer.material;
            if (material != null)
                material.color = itemData.itemColor;
        }

        // Устанавливаем слой для предметов
        gameObject.layer = 7; // Item layer
    }

    public void OnCollected()
    {
        if (CanBeCollected == false)
            return;

        CanBeCollected = false;

        // Визуально скрываем предмет
        if (itemRenderer != null)
            itemRenderer.enabled = false;

        if (itemCollider != null)
            itemCollider.enabled = false;

        Debug.Log($"Item {ItemName} collected");

        // Запускаем респаун через некоторое время
        if (itemData != null && itemData.respawnTime > 0)
            Invoke(nameof(OnRespawn), itemData.respawnTime);
    }

    public void OnRespawn()
    {
        CanBeCollected = true;

        // Восстанавливаем визуал
        if (itemRenderer != null)
            itemRenderer.enabled = true;

        if (itemCollider != null)
            itemCollider.enabled = true;

        Debug.Log($"Item {ItemName} respawned");
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        if (CanBeCollected)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}