using System;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public int CurrentValue { get; protected set; }

    public event Action CounterChanged; 

    public virtual void Reset()
    {
        CurrentValue = 0;
        CounterChanged?.Invoke();
    }

    public virtual void Add(int increment)
    {
        if (increment <= 0)        
            return;        

        CurrentValue += increment;
        CounterChanged?.Invoke();
    }

    protected void OnChanged()
    {
        CounterChanged?.Invoke();
    }
}
