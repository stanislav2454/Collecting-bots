using TMPro;
using UnityEngine;

public class CounterView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private Counter _counter;

    private void OnEnable() =>
        _counter.CounterChanged += ShowCounterValue;

    private void OnDisable() =>
        _counter.CounterChanged -= ShowCounterValue;

    private void ShowCounterValue() =>
        _counterText.text = $"Собранные ресурсы: {_counter.CurrentCounterValue}";
}