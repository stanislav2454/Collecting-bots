using System;
using UnityEngine;

public class ItemCounter : Counter
{
    public event Action<int> ResourceSpent;

    public bool TrySubtract(int value)
    {
        if (value <= 0)
            return false;

        if (CurrentValue >= value)
        {
            CurrentValue -= value;
            OnChanged();
            ResourceSpent?.Invoke(value);
            return true;
        }

        return false;
    }

    public bool CanAfford(int cost)
    {
        return CurrentValue >= cost;
    }
}