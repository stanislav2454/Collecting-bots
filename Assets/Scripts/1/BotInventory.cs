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

    public bool TryAddItem(Item item)
    {
        if (IsFull || item == null)
            return false;

        _carriedItem = item;
        ItemAdded?.Invoke();
        return true;
    }

    public void ClearInventory()
    {
        _carriedItem = null;
        InventoryCleared?.Invoke();
    }
}