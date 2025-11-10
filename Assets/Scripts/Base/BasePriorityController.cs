using UnityEngine;
using System;

public class BasePriorityController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseConstructor _baseConstructor;

    [Header("Price Settings")]
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    private BaseController _baseController;
    private ItemCounter _itemCounter;
    private BotController _botManager;
    private FlagController _flagController;
    private bool _isProcessingConstruction = false;

    public event Action<BasePriority> PriorityChanged;

    public BasePriority CurrentPriority { get; private set; } = BasePriority.CollectForBots;
    public bool CanAffordBot => _itemCounter.CanAfford(_resourcesForBot);
    public bool CanAffordNewBase => _itemCounter.CanAfford(_resourcesForNewBase);

    public void Initialize(BaseController baseController, ItemCounter itemCounter,
                BotController botManager, FlagController flagController)
    {
        _baseController = baseController;
        _itemCounter = itemCounter;
        _botManager = botManager;
        _flagController = flagController;
    }

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

    public void SetConstructionManager(BaseConstructor constructionManager) =>
        _baseConstructor = constructionManager;

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
        if (_flagController == null || _flagController.HasActiveFlag == false)
            return;

        var builderBot = _botManager.GetAvailableBotForConstruction() ?? _botManager.ForceGetBotForConstruction();

        if (builderBot == null)
            return;

        if (_itemCounter.TrySubtract(_resourcesForNewBase))
        {
            if (_baseConstructor != null)
            {
                _baseConstructor.StartBaseConstruction(
                    _baseController,
                    _flagController.FlagPosition,
                    builderBot);

                _flagController.RemoveFlag();
            }
            else
            {
                _itemCounter.Add(_resourcesForNewBase);
                _isProcessingConstruction = false;
            }
        }
    }
}