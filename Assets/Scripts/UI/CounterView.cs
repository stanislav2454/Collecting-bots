using TMPro;
using UnityEngine;

public class CounterView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private Counter _counter;

    private void Start()
    {
        ShowCounterValue();
    }

    private void OnEnable() =>
        _counter.Changed += ShowCounterValue;

    private void OnDisable() =>
        _counter.Changed -= ShowCounterValue;

    private void ShowCounterValue() =>
        _counterText.text = $"Собранные ресурсы: {_counter.CurrentValue}";
}