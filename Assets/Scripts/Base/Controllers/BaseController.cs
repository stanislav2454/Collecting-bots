using UnityEngine;

public class BaseController : MonoBehaviour, IBaseController
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ResourceManager _resourceManager;
    [Space(5)]
    [SerializeField] private BaseSelectionController _selectionController;
    [SerializeField] private BaseFlagController _flagController;
    [SerializeField] private BasePriorityController _priorityController;
    [SerializeField] private BaseResourceController _resourceController;

    public BasePriority CurrentPriority => _priorityController != null ?
            _priorityController.CurrentPriority : BasePriority.CollectForBots;
    public bool IsSelected => _selectionController != null ?
            _selectionController.IsSelected : false;
    public int CollectedResources => _resourceController != null ?
            _resourceController.CollectedResources : 0;
    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;

    //todo: 
    //1. "Magic numbers" => раньше значение было из _resourcesForBot, 
    //      сейчас эти поли в class "BasePriorityController"
    //2. зачем ? - 0 ссылок
    public bool CanAffordBot => _resourceController != null ? _resourceController.CanAfford(3) : false;
    //todo: 
    //1. "Magic numbers" => раньше значение было из _resourcesForBot, 
    //      сейчас эти поли в class "BasePriorityController"
    //2. зачем ? - 0 ссылок
    public bool CanAffordNewBase => _resourceController != null ? _resourceController.CanAfford(5) : false;

    public bool HasActiveFlag => _flagController != null ? _flagController.HasActiveFlag : false;
    public bool CanBuildNewBase => _botManager != null && _botManager.BotCount > 1;
    public Vector3 BasePosition => transform.position;

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

    BotManager IBaseController.GetBotManager() =>// почему нельзя указывать модификатор доступа ?
        GetBotManager();

    public void CollectResourceFromBot(Bot bot) =>
        _resourceController?.CollectResourceFromBot(bot);

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

    public void OnBaseConstructionCompleted()
    {
        RemoveFlag();
        _priorityController?.SetPriority(BasePriority.CollectForBots);
        Debug.Log("Base construction completed successfully!");
    }

    public void SelectBase() =>
        _selectionController?.SelectBase();

    public void DeselectBase() =>
        _selectionController?.DeselectBase();

    public void SetSelected(bool selected, bool notifyOthers = true) =>
        _selectionController?.SetSelected(selected, notifyOthers);

    public bool TrySetFlag(Vector3 worldPosition) =>
          _flagController != null ? _flagController.TrySetFlag(worldPosition) : false;

    public void RemoveFlag() =>
        _flagController?.RemoveFlag();

    private void RegisterWithSelectionManager()
    {
        if (BaseSelectionManager.Instance != null)
            BaseSelectionManager.Instance.RegisterBase(this);
    }

    private void InitializeAndValidateDependencies()
    {
        if (TryGetComponent(out _selectionController) == false)
            Debug.LogError("BaseSelectionController not found in BaseController!");

        if (TryGetComponent(out _flagController) == false)
            Debug.LogError("BaseFlagController not found in BaseController!");

        if (TryGetComponent(out _priorityController) == false)
            Debug.LogError("BasePriorityController not found in BaseController!");

        if (TryGetComponent(out _resourceController) == false)
            Debug.LogError("BaseResourceController not found in BaseController!");

        _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();
        if (_zoneVisualizer == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        _botManager = GetComponentInChildren<BotManager>();
        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

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
    }

    private void StartBaseConstruction(Bot builderBot, Vector3 buildPosition)//todo: не используется ?
    {
        if (builderBot == null)
            return;

        Debug.Log($"Starting base construction at {buildPosition} with bot {builderBot.name}");

        builderBot.BuildBase(buildPosition, this);
    }
}