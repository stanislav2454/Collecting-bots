using System;
using UnityEngine;

public class BotInventory : MonoBehaviour
{
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

            CarriedItem.transform.SetParent(null);//из-за этого рес может спавнится в корне сцены
            CarriedItem = null;
            InventoryCleared?.Invoke();
        }
    }
}