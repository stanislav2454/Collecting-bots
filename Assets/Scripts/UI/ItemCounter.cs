using System;
using UnityEngine;

public class ItemCounter : Counter
{
    public event Action<int> ResourceSpent;
    public event Action<int> ResourceAdded;

    public override void Add(int increment)
    {
        if (increment <= 0)
        {
            Debug.LogWarning($"Attempt to add non-positive value: {increment}");
            return;
        }

        CurrentValue += increment;
        OnChanged();
        ResourceAdded?.Invoke(increment);
    }

    public bool TrySubtract(int value)
    {
        if (value <= 0)
        {
            Debug.LogWarning($"Attempt to subtract invalid value: {value}");
            return false;
        }

        if (CurrentValue >= value)
        {
            CurrentValue -= value;
            OnChanged();
            ResourceSpent?.Invoke(value);
            Debug.Log($"Resources subtracted: {value}. Remaining: {CurrentValue}");
            return true;
        }

        Debug.Log($"Not enough resources. Required: {value}, Available: {CurrentValue}");
        return false;
    }

    public bool CanAfford(int cost) =>
         CurrentValue >= cost;

    public int GetMissingAmount(int cost) =>
         Mathf.Max(0, cost - CurrentValue);
}