using System;
using UnityEngine;

public class BotInventory : MonoBehaviour
{
    [SerializeField] private int _maxCapacity = 1;

    public event Action ItemAdded;
    public event Action InventoryCleared;

    public bool HasItem => CarriedItem != null;
    public bool IsFull => HasItem;
    public Item CarriedItem { get; private set; }

    public bool TryAddItem(Item item)
    {
        if (IsFull || item == null)
            return false;

        CarriedItem = item;
        item.AttachToBot(transform);
        ItemAdded?.Invoke();

        return true;
    }

    public void ClearInventory(bool prepareForRespawn = false)
    {
        if (CarriedItem != null)
        {
            if (prepareForRespawn)
                CarriedItem.PrepareForRespawn();

            CarriedItem.transform.SetParent(null);
            CarriedItem = null;
            InventoryCleared?.Invoke();
        }
    }
}