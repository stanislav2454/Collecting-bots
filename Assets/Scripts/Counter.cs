using System;
using UnityEngine;

/// <summary>
/// Счетчик очков, который увеличивается при доставке ресурсов
/// </summary>
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
        {
            _baseController.ResourceCollected += IncreaseCounter;
        }
    }


    private void OnDisable()
    {
        if (_baseController != null)
        {
            _baseController.ResourceCollected -= IncreaseCounter;
        }
    }

    private void IncreaseCounter(int totalResources)
    {
        _score++;
        Debug.Log($"🎯 Счет увеличен: {_score}");
        CounterChanged?.Invoke();
    }

    public void ResetCounter()
    {
        _score = 0;
        CounterChanged?.Invoke();
        Debug.Log("🔄 Счетчик сброшен");
    }

    public void AddPoints(int points)
    {
        if (points > 0)
        {
            _score += points;
            CounterChanged?.Invoke();
            Debug.Log($"➕ Добавлено {points} очков. Всего: {_score}");
        }
    }
}