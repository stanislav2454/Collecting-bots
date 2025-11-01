using UnityEngine;

public class BaseController : MonoBehaviour, IBaseController
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ItemCounter _itemCounter;


    [SerializeField] private BaseSelectionController _selectionController;
    [SerializeField] private BaseFlagController _flagController;
    [SerializeField] private BasePriorityController _priorityController;

    public BasePriority CurrentPriority => _priorityController != null ? _priorityController.CurrentPriority : BasePriority.CollectForBots;

    public bool IsSelected => _selectionController != null ? _selectionController.IsSelected : false;
    public int CollectedResources => _itemCounter.CurrentValue;
    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public bool CanAffordBot => _itemCounter.CanAfford(3);// раньше значение было из _resourcesForBot, сейчас эти поли в class "BasePriorityController"
    public bool CanAffordNewBase => _itemCounter.CanAfford(5);// раньше значение было из _resourcesForBot, сейчас эти поли в class "BasePriorityController"
    public bool HasActiveFlag => _flagController != null ? _flagController.HasActiveFlag : false;
    public bool CanBuildNewBase => _botManager != null && _botManager.BotCount > 1;
    public Vector3 BasePosition => transform.position;

    void IBaseController.CollectResourceFromBot(Bot bot) =>
        CollectResourceFromBot(bot);

    BotManager IBaseController.GetBotManager() =>
        GetBotManager();

    public bool TransferBotToNewBase(IBaseController newBase)
    {
        if (newBase is BaseController concreteBase)
            return TransferBotToNewBase(concreteBase);
        return false;
    }

    public BotManager GetBotManager()
        => _botManager;

    public bool HasAvailableBotForTransfer() =>
        _botManager?.GetAvailableBotForTransfer() != null;

    public bool TransferBotToNewBase(BaseController newBase)
    {
        if (newBase == null || newBase._botManager == null)
        {
            Debug.LogError("Cannot transfer bot: newBase or its BotManager is null");
            return false;
        }

        var availableBot = _botManager.GetAvailableBotForTransfer();
        if (availableBot == null)
        {
            Debug.LogWarning("No available bots for transfer");
            return false;
        }

        if (_botManager.TransferBotToNewBase(availableBot, newBase))
        {
            newBase._botManager.AddExistingBot(availableBot);
            return true;
        }

        return false;
    }

    private void StartBaseConstruction(Bot builderBot, Vector3 buildPosition)// не используется ?
    {
        if (builderBot == null)
            return;

        Debug.Log($"Starting base construction at {buildPosition} with bot {builderBot.name}");

        builderBot.BuildBase(buildPosition, this);
    }
    public void OnBaseConstructionCompleted()
    {
        RemoveFlag();
        _priorityController?.SetPriority(BasePriority.CollectForBots);
        Debug.Log("Base construction completed successfully!");
    }

    private void Start()
    {
        RegisterWithSelectionManager();
        InitializeAndValidateDependencies();
    }

    private void OnValidate()
    {
        if (GetComponentInChildren<BaseZoneVisualizer>() == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemCounter == null)
            Debug.LogError("ItemCounter not assigned in BaseController!");
    }

    private void OnEnable()
    {
        if (_itemCounter != null && _priorityController != null)
            _itemCounter.CounterChanged += _priorityController.OnResourcesChanged;
    }

    private void OnDisable()
    {
        if (_itemCounter != null && _priorityController != null)
            _itemCounter.CounterChanged -= _priorityController.OnResourcesChanged;
    }

    private void OnDestroy()
    {
        if (BaseSelectionManager.Instance != null)
            BaseSelectionManager.Instance.UnregisterBase(this);
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

    public void SelectBase() =>
        _selectionController?.SelectBase();

    public void DeselectBase() =>
        _selectionController?.DeselectBase();

    public void SetSelected(bool selected, bool notifyOthers = true) =>
        _selectionController?.SetSelected(selected, notifyOthers);

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;
        if (item != null)
            _itemCounter.Add(item.Value);

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    private void RegisterWithSelectionManager()
    {
        if (BaseSelectionManager.Instance != null)
            BaseSelectionManager.Instance.RegisterBase(this);
    }

    private void InitializeAndValidateDependencies()
    {
        _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();
        if (_zoneVisualizer == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        if (TryGetComponent(out _selectionController) == false)
            Debug.LogError("BaseSelectionController not found in BaseController!");

        if (TryGetComponent(out _flagController) == false)
            Debug.LogError("BaseFlagController not found in BaseController!");

        if (TryGetComponent(out _priorityController) == false)
            Debug.LogError("BasePriorityController not found in BaseController!");

        _botManager = GetComponentInChildren<BotManager>();
        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

        _itemCounter = GetComponentInChildren<ItemCounter>();
        if (_itemCounter == null)
            Debug.LogError("ItemCounter not found in BaseController!");

        if (_botManager != null)
        {
            _botManager.SetBaseController(this);

            if (_resourceManager == null && GameDependencies.Instance != null)
            {
                _resourceManager = GameDependencies.Instance.ResourceManager;
                if (_resourceManager != null)
                {
                    _botManager.SetResourceManager(_resourceManager);
                    Debug.Log("ResourceManager successfully assigned to BotManager via BaseController");
                }
            }
        }

        if (_itemSpawner == null && GameDependencies.Instance != null)
        {
            _itemSpawner = GameDependencies.Instance.ItemSpawner;
        }
    }

    public bool TrySetFlag(Vector3 worldPosition) =>
          _flagController != null ? _flagController.TrySetFlag(worldPosition) : false;

    public void RemoveFlag() =>
        _flagController?.RemoveFlag();
}