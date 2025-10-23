using System;
using UnityEngine;

public class BotInventory : MonoBehaviour
{
    [SerializeField] private int _maxCapacity = 1;

    private Item _carriedItem;

    public event Action ItemAdded;
    public event Action InventoryCleared;

    public bool HasItem => _carriedItem != null;
    public bool IsFull => HasItem;
    public Item CarriedItem => _carriedItem;

    public bool TryAddItem(Item item)
    {
        if (IsFull || item == null)
            return false;

        _carriedItem = item;
        item.AttachToBot(transform);
        ItemAdded?.Invoke();

        return true;
    }

    public void ClearInventory()
    {
        if (_carriedItem != null)
        {
            _carriedItem.DetachFromBot();
            _carriedItem = null;
            InventoryCleared?.Invoke();
        }
    }

    /// <summary>
    /// Освобождает ресурс без доставки на базу (при неудачной миссии)
    /// </summary>
    public void ReleaseItem()
    {
        if (_carriedItem != null)
        {
            // Возвращаем ресурс на карту
            _carriedItem.transform.SetParent(null);
            if (_carriedItem.TryGetComponent<Collider>(out var collider))
                collider.enabled = true;
            _carriedItem = null;
        }
    }
}