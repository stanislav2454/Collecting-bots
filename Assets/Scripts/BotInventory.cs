using System.Collections.Generic;
using UnityEngine;

public class BotInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxCapacity = 5;

    private List<IItem> collectedItems = new List<IItem>();

    public int MaxCapacity => maxCapacity;
    public int CurrentCount => collectedItems.Count;
    public bool IsFull => CurrentCount >= maxCapacity;
    public bool IsEmpty => CurrentCount == 0;

    public bool TryAddItem(IItem item)
    {
        if (item == null || IsFull)
            return false;

        collectedItems.Add(item);
        item.OnCollected();

        return true;
    }

    public void ClearInventory() =>
        collectedItems.Clear();

    public int GetItemCount() =>// зачем нужен ? если не используется - удалить !
         collectedItems.Count;

    public bool HasItems() =>// зачем нужен ? если не используется - удалить !
         IsEmpty == false;
}