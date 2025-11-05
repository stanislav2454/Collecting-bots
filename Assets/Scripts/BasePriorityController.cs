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

    // для защиты от рекурсии
    private bool _isProcessingConstruction = false;
    private BasePriority _currentPriority = BasePriority.CollectForBots;

    public event Action<BasePriority> PriorityChanged;

    public BasePriority CurrentPriority => _currentPriority;// переделать на автосвойство
    public bool CanAffordBot => _itemCounter.CanAfford(_resourcesForBot);
    public bool CanAffordNewBase => _itemCounter.CanAfford(_resourcesForNewBase);


    public void SetPriority(BasePriority newPriority)
    {
        if (_currentPriority != newPriority)
        {
            Debug.Log($"[PriorityController] Priority changing from {_currentPriority} to {newPriority}");

            _currentPriority = newPriority;
            PriorityChanged?.Invoke(newPriority);
            CheckResourceSpending();
        }
    }

    public void OnResourcesChanged()
    {
        // ЗАЩИТА: не обрабатываем изменения во время строительства
        if (_isProcessingConstruction)
            return;

        CheckResourceSpending();
    }

    public void ResetConstructionFlag()
    {
        _isProcessingConstruction = false;
        Debug.Log("[PriorityController] Construction flag reset");
    }

    private void CheckResourceSpending()
    {
        // ЗАЩИТА: не обрабатываем если уже в процессе строительства
        if (_isProcessingConstruction)
            return;

        switch (_currentPriority)
        {
            case BasePriority.CollectForBots when CanAffordBot:
                CreateBotFromResources();
                break;

            case BasePriority.CollectForNewBase when CanAffordNewBase:
                _isProcessingConstruction = true; // Блокируем повторные вызовы
                StartBaseConstruction();
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
                    // Возвращаем ресурсы если что-то пошло не так
                    _itemCounter.Add(_resourcesForNewBase);
                }
            }
            else
            {
                // Возвращаем ресурсы если нет доступного бота
                _itemCounter.Add(_resourcesForNewBase);
            }
        }
    }
}