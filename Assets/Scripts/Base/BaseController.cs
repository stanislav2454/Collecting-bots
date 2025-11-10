using UnityEngine;

public class BaseController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotController _botManager;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BasePriorityController _priorityController;
    [SerializeField] private FlagController _flagController;

    [Header("Selection Settings")]
    [SerializeField] private MaterialChanger _materialChanger;
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;
    private bool _isSelected = false;
    private ItemSpawner _itemSpawner;
    private bool _isInitialized = false;

    public bool IsSelected => _isSelected;
    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;

    public bool CanBuildNewBase => _botManager != null && _botManager.BotCount > 1;
    public BasePriority CurrentPriority => _priorityController?.CurrentPriority ?? BasePriority.CollectForBots;

    private void Awake()
    {
        if (_priorityController == null)
            _priorityController = GetComponent<BasePriorityController>();

        if (_flagController == null)
            _flagController = GetComponent<FlagController>();

        if (_botManager == null)
            _botManager = GetComponentInChildren<BotController>();

        if (_itemCounter == null)
            _itemCounter = GetComponentInChildren<ItemCounter>();

        if (_zoneVisualizer == null)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();
    }

    private void Start()
    {
        InitializeSelection();
        InitializeAndValidateDependencies();

        if (_isInitialized == false)
            Debug.LogWarning("BaseController not initialized! Call Initialize() method.");
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_viewTransform != null)
            _originalViewScale = _viewTransform.localScale;
#endif
    }

    private void OnEnable()
    {
        if (_itemCounter != null)
            _itemCounter.CounterChanged += OnItemCounterChanged;
    }

    private void OnDisable()
    {
        if (_itemCounter != null)
            _itemCounter.CounterChanged -= OnItemCounterChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInitialized == false)
            return;

        if (other.TryGetComponent<Bot>(out var bot) && bot.IsCarryingResource)
        {
            CollectResourceFromBot(bot);
            bot.CompleteMission(true);
        }
    }

    public void Initialize(ItemSpawner itemSpawner, ResourceAllocator resourceManager)
    {
        _itemSpawner = itemSpawner;

        if (_priorityController != null)
            _priorityController.Initialize(this, _itemCounter, _botManager, _flagController);
        else
            Debug.LogError(" PriorityController not found!");

        if (_botManager != null)
            _botManager.Initialize(this, resourceManager);
        else
            Debug.LogError("   BotManager not found!");

        if (_flagController != null)
            _flagController.Initialize(this, _priorityController);
        else
            Debug.LogError(" FlagController not found!");

        _isInitialized = true;
    }

    public void SetSelected(bool selected)
    {
        if (_isSelected == selected)
            return;

        _isSelected = selected;

        if (selected)
        {
            _viewTransform.localScale = _originalViewScale * _selectedScaleMultiplier;
            _materialChanger?.SetSelected(true);
        }
        else
        {
            _viewTransform.localScale = _originalViewScale;
            _materialChanger?.SetSelected(false);
        }
    }

    public void CollectResourceFromBot(Bot bot)
    {
        if (_isInitialized == false)
        {
            Debug.LogError("BaseController not initialized!");
            return;
        }

        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;

        if (item != null)
            _itemCounter.Add(item.Value);

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    public bool TrySetFlag(Vector3 worldPosition)
    {
        if (_isInitialized == false)
        {
            Debug.LogError("BaseController not initialized!");
            return false;
        }

        return _flagController?.TrySetFlag(worldPosition) ?? false;
    }

    private void InitializeSelection() =>
        SetSelected(false);

    private void OnItemCounterChanged() =>
        _priorityController?.OnResourcesChanged();

    private void InitializeAndValidateDependencies()
    {
        if (_zoneVisualizer == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemCounter == null)
            Debug.LogError("ItemCounter not found in BaseController!");

        if (_materialChanger == null)
            Debug.LogError("MaterialChanger not found in BaseController!");

        if (_viewTransform == null)
            Debug.LogError("ViewTransform not assigned in BaseController!");
        else
            _originalViewScale = _viewTransform.localScale;

        if (_priorityController == null)
            Debug.LogError("BasePriorityController not found in BaseController!");

        if (_flagController == null)
            Debug.LogError("FlagController not found in BaseController!");
    }
}
