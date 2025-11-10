using UnityEngine;

[RequireComponent(typeof(BasePriorityController), typeof(ItemCounter), typeof(FlagController))]
[RequireComponent(typeof(SelectableVisual))]
public class BaseController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BasePriorityController _priorityController;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private FlagController _flagController;
    [SerializeField] private SelectableVisual _selectableVisual;
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotController _botController;

    private ItemSpawner _itemSpawner;
    private bool _isInitialized = false;

    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public bool CanBuildNewBase => _botController != null && _botController.BotCount > 1;// Проверка "_botController != null" т.к _botController дочерний объект

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

    public void SetSelected(bool selected) =>
        _selectableVisual.SetSelected(selected);

    public void SetFlag(Vector3 worldPosition) =>
        _flagController.TrySetFlag(worldPosition);

    private void InitializeComponents()
    {
        _priorityController = GetComponent<BasePriorityController>();
        _flagController = GetComponent<FlagController>();
        _itemCounter = GetComponent<ItemCounter>();
        _selectableVisual = GetComponent<SelectableVisual>();

        if (_botController == null)
            _botController = GetComponentInChildren<BotController>();

        if (_zoneVisualizer == null)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();
    }

    private void OnItemCounterChanged() =>
        _priorityController?.OnResourcesChanged();
}
