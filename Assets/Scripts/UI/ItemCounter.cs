using System;
using UnityEngine;

public class ItemCounter : Counter
{
    public event Action<int> ResourceSpent; // Событие при трате ресурсов
    public event Action<int> ResourceAdded; // Событие при добавлении ресурсов

    public override void Add(int increment)
    {
        if (increment <= 0)
        {
            Debug.LogWarning($"Attempt to add non-positive value: {increment}");
            return;
        }

        CurrentValue += increment;
        OnChanged();
        // CounterChanged?.Invoke();
        ResourceAdded?.Invoke(increment); // Отдельное событие для добавления
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
            OnChanged(); // Вызываем метод вместо прямого вызова события
            //Changed?.Invoke(CurrentValue);
            ResourceSpent?.Invoke(value);
            Debug.Log($"Resources subtracted: {value}. Remaining: {CurrentValue}");
            return true;
        }

        Debug.Log($"Not enough resources. Required: {value}, Available: {CurrentValue}");
        return false;
    }

    public bool CanAfford(int cost)
    {
        return CurrentValue >= cost;
    }

    public int GetMissingAmount(int cost)
    {
        return Mathf.Max(0, cost - CurrentValue);
    }
}