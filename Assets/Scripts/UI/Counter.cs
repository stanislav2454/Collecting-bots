using System;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseController _baseController;

    private int _score = 0;

    public event Action CounterChanged;

    public int CurrentCounterValue => _score;

    private void OnEnable()
    {
        if (_baseController != null)
            _baseController.ResourceCollected += IncreaseCounter;
    }

    private void OnDisable()
    {
        if (_baseController != null)
            _baseController.ResourceCollected -= IncreaseCounter;
    }

    private void IncreaseCounter(int totalResources)
    {
        _score = totalResources; 
        CounterChanged?.Invoke();
    }
}