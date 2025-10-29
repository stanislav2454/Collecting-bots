using System;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ResourceManager _resourceManager;

    [field: SerializeField] public BaseFlag FlagPrefab { get; private set; }
    private BaseFlag _currentFlag;
    //private BasePriority CurrentPriority = BasePriority.CollectForBots;

    public event Action<int> AmountResourcesChanged;
    public event Action<BasePriority> PriorityChanged;

    public float UnloadZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public int CollectedResources { get; private set; }
    public bool CanAffordBot => CollectedResources >= _resourcesForBot;
    // 👇
    public bool CanAffordNewBase => CollectedResources >= _resourcesForNewBase;
    public BasePriority CurrentPriority { get; private set; } = BasePriority.CollectForBots;
    // public BasePriority CurrentPriority => _currentPriority;// 👈 // удалить и исп.авто св-во !!!
    public bool HasActiveFlag => _currentFlag != null;
    public bool CanBuildNewBase => _botManager != null && _botManager.BotCount > 1;

    private void Start()
    {
        InitializeDependencies();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Bot>(out var bot))
        {
            if (bot.IsCarryingResource)
            {
                CollectResourceFromBot(bot);
                bot.CompleteMission(true);
            }
        }
    }

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;
        if (item != null)
        {
            CollectedResources += item.Value;
            AmountResourcesChanged?.Invoke(CollectedResources);

            if (CanAffordBot)
                CheckResourceSpending();
            //CreateBotFromResources();
        }

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    private void CheckResourceSpending()
    {
        switch (CurrentPriority)
        {
            case BasePriority.CollectForBots when CanAffordBot:
                CreateBotFromResources();
                break;
            case BasePriority.CollectForNewBase when CanAffordNewBase && HasActiveFlag:
                CreateNewBaseFromResources();
                break;
        }
    }

    private void CreateBotFromResources()
    {
        if (_botManager == null || CurrentPriority != BasePriority.CollectForBots)
            return;

        CollectedResources -= _resourcesForBot;
        AmountResourcesChanged?.Invoke(CollectedResources);
        _botManager.CreateNewBot();
        //// После создания бота снова проверяем, может хватает на еще одного
        //CheckResourceSpending();
    }

    private void CreateNewBaseFromResources()
    {
        if (_currentFlag == null || _botManager == null) 
            return;

        CollectedResources -= _resourcesForNewBase;
        AmountResourcesChanged?.Invoke(CollectedResources);

        Debug.Log("Starting new base construction! Need to send builder bot.");
        // TODO: Реализовать отправку строителя
        // _botManager.SendBuilderToFlag(_currentFlag);
    }

    private void InitializeDependencies()
    {
        if (TryGetComponent(out _zoneVisualizer) == false)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();

        if (TryGetComponent(out _botManager) == false)
            _botManager = GetComponentInChildren<BotManager>();
        else
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemSpawner == null)
            Debug.LogError("ItemSpawner not assigned in BotManager!");

        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in BotManager!");

        if (FlagPrefab == null)
            Debug.LogError("FlagPrefab not assigned in BotManager!");
    }
    // TODO вынести логику работы с флагом в отдельный класс
    #region Flag 
    public bool TrySetFlag(Vector3 worldPosition)
    {
        if (CanBuildNewBase == false)// 👈
        {
            Debug.LogWarning("Cannot set flag: not enough bots. Need at least 2 bots.");
            return false;
        }

        if (IsValidFlagPosition(worldPosition) == false)
        {
            Debug.LogWarning("Invalid flag position");
            return false;
        }

        if (_currentFlag != null)
            _currentFlag.UpdatePosition(worldPosition, true);

        SetPriority(BasePriority.CollectForNewBase);
        return true;
    }

    private bool IsValidFlagPosition(Vector3 position)
    {
        if (position.y < 0) // Проверяем, что позиция в пределах карты (можно расширить)
            return false;

        float checkRadius = 1f;// Проверяем коллизии с непроходимыми объектами
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        foreach (var collider in colliders)
        {
            if (collider.isTrigger == false && collider.TryGetComponent<Ground>(out _) == false)
                return false;
        }

        return true;
    }

    public void RemoveFlag()// ?
    {
        if (_currentFlag != null)
        {
            _currentFlag.Remove();
            _currentFlag = null;
        }

        SetPriority(BasePriority.CollectForBots);
    }

    private void SetPriority(BasePriority newPriority)
    {
        if (CurrentPriority != newPriority)
        {
            CurrentPriority = newPriority;
            Debug.Log($"Base priority changed to: {newPriority}");
            PriorityChanged?.Invoke(newPriority);

            CheckResourceSpending();
        }
    }
    #endregion
}