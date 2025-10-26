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
    public Item GetCarriedItem => _carriedItem;

    public bool TryAddItem(Item item)
    {
        if (IsFull || item == null)
            return false;

        _carriedItem = item;
        item.AttachToBot(transform);
        ItemAdded?.Invoke();

        return true;
    }

    public void ClearInventory(bool prepareForRespawn = false)
    {
        if (_carriedItem != null)
        {
            if (prepareForRespawn)
                _carriedItem.PrepareForRespawn();

            _carriedItem.transform.SetParent(null);
            _carriedItem = null;
            InventoryCleared?.Invoke();
        }
    }
}