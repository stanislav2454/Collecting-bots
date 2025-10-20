using System.Collections.Generic;
using UnityEngine;

public class BotInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxCapacity = 5;

    private List<IItem> collectedItems = new List<IItem>();

    public int CurrentCount => collectedItems.Count;
    public bool IsFull => CurrentCount >= maxCapacity;
    public bool IsEmpty => CurrentCount == 0;

    public bool TryAddItem(IItem item)
    {
        if (item == null || IsFull)
            return false;

        collectedItems.Add(item);
        item.OnCollected();

        Debug.Log($"Item added to inventory. Current count: {CurrentCount}/{maxCapacity}");
        return true;
    }

    public void ClearInventory()
    {
        collectedItems.Clear();
        Debug.Log("Inventory cleared");
    }

    public int GetItemCount()
    {
        return collectedItems.Count;
    }

    public bool HasItems()
    {
        return IsEmpty == false;
    }
}