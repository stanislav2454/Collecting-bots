using UnityEngine;
using System;

public class BasePriorityController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseController _baseController;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BotManager _botManager;

    [Header("Price Settings")]
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    private bool _isProcessingConstruction = false;

    public event Action<BasePriority> PriorityChanged;

    public BasePriority CurrentPriority { get; private set; } = BasePriority.CollectForBots;
    public bool CanAffordBot => _itemCounter.CanAfford(_resourcesForBot);
    public bool CanAffordNewBase => _itemCounter.CanAfford(_resourcesForNewBase);

    public void SetPriority(BasePriority newPriority)
    {
        if (CurrentPriority != newPriority)
        {
            CurrentPriority = newPriority;
            PriorityChanged?.Invoke(newPriority);
            CheckResourceSpending();
        }
    }

    public void OnResourcesChanged()
    {
        if (_isProcessingConstruction)
            return;

        CheckResourceSpending();
    }

    public void ResetConstructionFlag() =>
        _isProcessingConstruction = false;

    private void CheckResourceSpending()
    {
        if (_isProcessingConstruction)
            return;

        switch (CurrentPriority)
        {
            case BasePriority.CollectForBots when CanAffordBot:
                CreateBotFromResources();
                break;

            case BasePriority.CollectForNewBase when CanAffordNewBase:
                _isProcessingConstruction = true;
                StartBaseConstruction();
                break;
        }
    }

    private void CreateBotFromResources()
    {
        if (_botManager == null || CurrentPriority != BasePriority.CollectForBots)
            return;

        if (_itemCounter.TrySubtract(_resourcesForBot))
            _botManager.CreateNewBot();
    }

    private void StartBaseConstruction()
    {
        // 1. Списываем ресурсы
        if (_itemCounter.TrySubtract(_resourcesForNewBase))
        {
            // 2. Находим строителя
            var builderBot = _botManager.GetAvailableBotForConstruction() ?? _botManager.ForceGetBotForConstruction();

            if (builderBot != null)
            {
                // 3. Запускаем строительство
                var constructionManager = FindAnyObjectByType<BaseConstructionManager>();//TODO: прямую ссылку
                var flagController = GetComponent<FlagController>();//TODO: TryGetComponent

                if (constructionManager != null && flagController != null)
                {
                    Debug.Log($"[PriorityController] Starting construction process at {flagController.FlagPosition}");

                    constructionManager.StartBaseConstruction(
                        _baseController,
                        flagController.FlagPosition,
                        builderBot);

                    flagController.RemoveFlag();
                }
                else
                {
                    _itemCounter.Add(_resourcesForNewBase);
                }
            }
            else
            {
                _itemCounter.Add(_resourcesForNewBase);
            }
        }
    }
}