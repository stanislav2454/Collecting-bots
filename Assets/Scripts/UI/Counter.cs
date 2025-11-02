using System;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public int CurrentValue { get; protected set; }

    public event Action Changed; 

    public virtual void Reset()
    {
        CurrentValue = 0;
        Changed?.Invoke();
        Debug.Log("Counter reset to 0");
    }

    public virtual void Add(int increment)
    {
        if (increment <= 0)
        {
            Debug.LogWarning($"Attempt to add non-positive value: {increment}");
            return;
        }

        CurrentValue += increment;
        Changed?.Invoke();
        Debug.Log($"Resources added: {increment}. Total: {CurrentValue}");
    }

    protected void OnChanged() =>
        Changed?.Invoke();
}