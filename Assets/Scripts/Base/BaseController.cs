using System;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BasePriorityController _priorityController;
    [SerializeField] private FlagController _flagController;
    [SerializeField] private BaseSelectionManager _selectionManager;

    [Header("Selection Settings")]
    [SerializeField] private MaterialChanger _materialChanger;
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;
    private bool _isSelected = false;

    public event Action<BaseController> BaseSelected;
    public event Action<BaseController> BaseDeselected;

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
            _botManager = GetComponentInChildren<BotManager>();

        if (_itemCounter == null)
            _itemCounter = GetComponentInChildren<ItemCounter>();

        if (_zoneVisualizer == null)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();
    }

    private void Start()
    {
        InitializeSelection();

        if (_selectionManager != null)
            _selectionManager.RegisterBase(this);
        else
            Debug.LogWarning($"BaseSelectionManager not assigned for {name}");

        InitializeAndValidateDependencies();
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

    private void OnDestroy()
    {
        if (_selectionManager != null)
            _selectionManager.UnregisterBase(this);
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

    private void OnMouseDown() =>
        ToggleSelection();

    public void SetSelected(bool selected, bool notifyOthers = true)
    {
        if (_isSelected == selected)
            return;

        _isSelected = selected;

        if (selected)
        {
            _viewTransform.localScale = _originalViewScale * _selectedScaleMultiplier;
            _materialChanger?.SetSelected(true);

            if (notifyOthers)
                BaseSelected?.Invoke(this);
        }
        else
        {
            _viewTransform.localScale = _originalViewScale;
            _materialChanger?.SetSelected(false);

            if (notifyOthers)
                BaseDeselected?.Invoke(this);
        }
    }

    public void SelectBase() =>
        SetSelected(true);

    public void DeselectBase() =>
        SetSelected(false);

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

    public bool TrySetFlag(Vector3 worldPosition) =>
        _flagController?.TrySetFlag(worldPosition) ?? false;

    private void InitializeSelection() =>
        SetSelected(false, false);

    private void ToggleSelection() =>
        SetSelected(IsSelected ? false : true);

    private void OnItemCounterChanged() =>
        _priorityController?.OnResourcesChanged();

    private void InitializeAndValidateDependencies()
    {
        if (_zoneVisualizer == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemCounter == null)
            Debug.LogError("ItemCounter not assigned in BotManager!");

        if (_materialChanger == null)
            Debug.LogError("MaterialChanger not assigned in BotManager!");

        if (_viewTransform == null)
            Debug.LogError("ViewTransform not assigned in BaseController!");
        else
            _originalViewScale = _viewTransform.localScale;

        if (_priorityController == null)
            Debug.LogError("_PriorityController not assigned in BaseController!");

        if (_flagController == null)
            Debug.LogError("FlagController not assigned in BaseController!");
    }
}