using System;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public int CurrentValue { get; protected set; }

    //public event Action<int> Changed;
    public event Action CounterChanged; // Изменяем сигнатуру события!

    public virtual void Reset()
    {
        CurrentValue = 0;
        CounterChanged?.Invoke();
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
        CounterChanged?.Invoke();
        Debug.Log($"Resources added: {increment}. Total: {CurrentValue}");
    }

    // Новый метод для удобства
    public void AddOne()
    {
        Add(1);
    }

    // Защищенный метод для вызова события из наследников
    protected void OnChanged()
    {
        CounterChanged?.Invoke();
    }
}

//using System;
//using UnityEngine;

//public class Counter : MonoBehaviour
//{
//    [Header("Dependencies")]
//    [SerializeField] private BaseController _baseController;

//    public event Action CounterChanged;

//    public int CurrentValue { get; private set; } = 0;

//    private void OnEnable()
//    {
//        if (_baseController != null)
//            _baseController.AmountResourcesChanged += IncreaseCounter;
//    }

//    private void OnDisable()
//    {
//        if (_baseController != null)
//            _baseController.AmountResourcesChanged -= IncreaseCounter;
//    }

//    private void IncreaseCounter(int totalResources)
//    {
//        CurrentValue = totalResources;
//        CounterChanged?.Invoke();
//    }
//}