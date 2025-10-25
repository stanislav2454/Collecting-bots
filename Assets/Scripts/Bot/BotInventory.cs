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

    public void ClearInventory(Bot bot = null)
    {
        if (_carriedItem != null)
        {
            _carriedItem.transform.SetParent(null);
            _carriedItem = null;

            InventoryCleared?.Invoke();
        }
    }

    public void ReleaseItem()
    {
        if (_carriedItem != null)
        {
            _carriedItem.transform.SetParent(null);
            _carriedItem.PrepareForRespawn();
            _carriedItem = null;
        }
    }
}