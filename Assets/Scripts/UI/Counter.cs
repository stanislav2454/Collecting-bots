using System;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseController _baseController;

    public event Action CounterChanged;

    public int CurrentValue { get; private set; } = 0;

    private void OnEnable()
    {
        if (_baseController != null)
            _baseController.AmountResourcesChanged += IncreaseCounter;
    }

    private void OnDisable()
    {
        if (_baseController != null)
            _baseController.AmountResourcesChanged -= IncreaseCounter;
    }

    private void IncreaseCounter(int totalResources)
    {
        CurrentValue = totalResources;
        CounterChanged?.Invoke();
    }
}