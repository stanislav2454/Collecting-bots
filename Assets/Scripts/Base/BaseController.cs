using UnityEngine;

public class BaseController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotController _botController;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BasePriorityController _priorityController;
    [SerializeField] private FlagController _flagController;
    [SerializeField] private SelectableVisual _selectableVisual;

    private ItemSpawner _itemSpawner;
    private bool _isInitialized = false;

    public bool IsSelected => _selectableVisual != null ? _selectableVisual.IsSelected : false;
    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public bool CanBuildNewBase => _botController != null && _botController.BotCount > 1;// Проверка "_botController != null" т.к _botController дочерний объект
    public BasePriority CurrentPriority => _priorityController?.CurrentPriority ?? BasePriority.CollectForBots;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if (_isInitialized == false)
            Debug.LogWarning("BaseController not initialized! Call Initialize() method.");
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

    public void Initialize(ItemSpawner itemSpawner, ResourceAllocator resourceAllocator)
    {
        _itemSpawner = itemSpawner;

        _priorityController.Initialize(this, _itemCounter, _botController, _flagController);
        _botController.Initialize(this, resourceAllocator);
        _flagController.Initialize(this, _priorityController);

        _isInitialized = true;
    }

    public void SetSelected(bool selected)
    {
        _selectableVisual?.SetSelected(selected);
    }

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;
        if (item != null)
            _itemCounter.Add(item.Value);

        bot.Inventory.ClearInventory();
        _itemSpawner.ReturnItemToPool(item);
    }

    public bool TrySetFlag(Vector3 worldPosition)
    {
        return _flagController?.TrySetFlag(worldPosition) ?? false;
    }

    private void InitializeComponents()
    {
        if (_priorityController == null)// TODO:
            _priorityController = GetComponent<BasePriorityController>();

        if (_flagController == null)// TODO:
            _flagController = GetComponent<FlagController>();

        if (_botController == null)// TODO:
            _botController = GetComponentInChildren<BotController>();

        if (_itemCounter == null)// TODO:
            _itemCounter = GetComponentInChildren<ItemCounter>();

        if (_zoneVisualizer == null)// TODO:
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();

        if (_selectableVisual == null)// TODO:
            _selectableVisual = GetComponent<SelectableVisual>();
    }

    private void OnItemCounterChanged()
    {
        _priorityController?.OnResourcesChanged();
    }
}

