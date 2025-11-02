using UnityEngine;
using System;

public class BasePriorityController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;
    [SerializeField] private BaseResourceController _resourceController;
    [SerializeField] private BaseFlagController _flagController;

    [Header("Price Settings")]
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    public event Action<BasePriority> PriorityChanged;

    public BasePriority CurrentPriority { get; private set; } = BasePriority.CollectForBots;

    private void OnValidate()
    {
        if (_botManager == null)
            Debug.LogWarning("BotManager not assigned in BasePriorityController!");

        if (_resourceController == null)
            Debug.LogWarning("BaseResourceController not assigned in BasePriorityController!");

        if (_flagController == null)
            Debug.LogWarning("BaseFlagController not assigned in BasePriorityController!");
    }

    private void OnDestroy()
    {
        PriorityChanged = null;
    }

    public void SetPriority(BasePriority newPriority)
    {
        if (CurrentPriority != newPriority)
        {
            CurrentPriority = newPriority;
            Debug.Log($"Base priority changed to: {newPriority}");
            PriorityChanged?.Invoke(newPriority);
            CheckResourceSpending();
        }
    }

    public void OnResourcesChanged() =>
        CheckResourceSpending();

    private void CheckResourceSpending()
    {
        switch (CurrentPriority)
        {
            case BasePriority.CollectForBots when
            _resourceController.CanAfford(_resourcesForBot):
                CreateBotFromResources();
                break;

            case BasePriority.CollectForNewBase when
            _resourceController.CanAfford(_resourcesForNewBase) && _flagController.HasActiveFlag:
                CreateNewBaseFromResources();
                break;
        }
    }

    private void CreateBotFromResources()
    {
        if (_botManager == null || CurrentPriority != BasePriority.CollectForBots)
            return;

        if (_resourceController.TrySpendResources(_resourcesForBot))
            _botManager.CreateNewBot();
    }

    private void CreateNewBaseFromResources()
    {
        if (_flagController == null || _botManager == null)
            return;

        if (_resourceController.TrySpendResources(_resourcesForNewBase))
        {
            var builderBot = _botManager.GetAvailableBotForTransfer();
            if (builderBot != null)
            {
                _flagController.StartBaseConstruction(builderBot);
            }
            else
            {
                Debug.LogWarning("No available bots for base construction");
                _resourceController.AddResource(_resourcesForNewBase);
            }
        }
    }
}