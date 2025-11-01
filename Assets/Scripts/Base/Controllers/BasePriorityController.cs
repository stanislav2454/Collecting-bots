using UnityEngine;
using System;

public class BasePriorityController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BaseFlagController _flagController;

    [Header("Price Settings")]
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    private BasePriority _currentPriority = BasePriority.CollectForBots;

    public event Action<BasePriority> PriorityChanged;
    public BasePriority CurrentPriority => _currentPriority;

    public void SetPriority(BasePriority newPriority)
    {
        if (_currentPriority != newPriority)
        {
            _currentPriority = newPriority;
            Debug.Log($"Base priority changed to: {newPriority}");
            PriorityChanged?.Invoke(newPriority);
            CheckResourceSpending();
        }
    }

    public void OnResourcesChanged()
    {
        CheckResourceSpending();
    }

    private void CheckResourceSpending()
    {
        switch (_currentPriority)
        {
            case BasePriority.CollectForBots when _itemCounter.CanAfford(_resourcesForBot):
                CreateBotFromResources();
                break;

            case BasePriority.CollectForNewBase when _itemCounter.CanAfford(_resourcesForNewBase) && _flagController.HasActiveFlag:
                CreateNewBaseFromResources();
                break;
        }
    }

    private void CreateBotFromResources()
    {
        if (_botManager == null || _currentPriority != BasePriority.CollectForBots)
            return;

        if (_itemCounter.TrySubtract(_resourcesForBot))
            _botManager.CreateNewBot();
    }

    private void CreateNewBaseFromResources()
    {
        if (_flagController == null || _botManager == null)
            return;

        if (_itemCounter.TrySubtract(_resourcesForNewBase))
        {
            var builderBot = _botManager.GetAvailableBotForTransfer();
            if (builderBot != null)
            {
                _flagController.StartBaseConstruction(builderBot);
            }
            else
            {
                Debug.LogWarning("No available bots for base construction");
                _itemCounter.Add(_resourcesForNewBase);
            }
        }
    }

    private void OnValidate()
    {
        if (_botManager == null)
            Debug.LogWarning("BotManager not assigned in BasePriorityController!");

        if (_itemCounter == null)
            Debug.LogWarning("ItemCounter not assigned in BasePriorityController!");

        if (_flagController == null)
            Debug.LogWarning("BaseFlagController not assigned in BasePriorityController!");
    }

    private void OnDestroy()
    {
        PriorityChanged = null;
    }
}