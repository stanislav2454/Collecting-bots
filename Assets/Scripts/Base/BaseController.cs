using System;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ItemCounter _itemCounter;

    [Header("Price Settings")]
    [SerializeField] private int _resourcesForBot = 3;
    [SerializeField] private int _resourcesForNewBase = 5;

    [Header("Flag System")]
    [SerializeField] private SimpleFlag _flagPrefab;

    [Header("Selection Settings")]
    [SerializeField] private MaterialChanger _materialChanger;
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;
    [SerializeField] private Transform _viewTransform;

    private SimpleFlag _currentFlag;
    private Vector3 _originalViewScale;
    private bool _isSelected = false;

    public event Action<BaseController> BaseSelected;
    public event Action<BaseController> BaseDeselected;
    public event Action<BasePriority> PriorityChanged;

    public BasePriority CurrentPriority { get; private set; } = BasePriority.CollectForBots;
    public bool IsSelected => _isSelected; // ? 0 references !
    public int CollectedResources => _itemCounter.CurrentValue;// ? 0 references !
    public float UnloadZoneRadius => _zoneVisualizer ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public bool CanAffordBot => _itemCounter.CanAfford(_resourcesForBot);
    public bool CanAffordNewBase => _itemCounter.CanAfford(_resourcesForNewBase);
    public bool HasActiveFlag => _currentFlag != null && _currentFlag.CurrentState != FlagState.Hide;
    public bool CanBuildNewBase => _botManager != null && _botManager.BotCount > 1;

    private void Start()
    {
        InitializeSelection();
        RegisterWithSelectionManager();
        InitializeAndValidateDependencies();
    }

    private void OnValidate()
    {
        InitializeAndValidateDependencies();
    }

    private void OnEnable()
    {
        if (_itemCounter != null)
            _itemCounter.CounterChanged += OnResourcesChanged;
    }

    private void OnDisable()
    {
        if (_itemCounter != null)
            _itemCounter.CounterChanged -= OnResourcesChanged;
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
        {
            _itemCounter.Add(item.Value);
            CheckResourceSpending();
        }

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    private void InitializeSelection() =>
        SetSelected(false, false);

    private void ToggleSelection() =>
        SetSelected(!_isSelected);

    private void RegisterWithSelectionManager()
    {
        if (BaseSelectionManager.Instance != null)
            BaseSelectionManager.Instance.RegisterBase(this);
    }

    private void OnResourcesChanged() =>// зачем ? модификатор ведь private    
        CheckResourceSpending();

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

        if (_itemCounter.TrySubtract(_resourcesForBot))
            _botManager.CreateNewBot();
    }

    private void CreateNewBaseFromResources()
    {
        if (_currentFlag == null || _botManager == null)
            return;

        if (_itemCounter.TrySubtract(_resourcesForNewBase))
        {
            Debug.Log("Starting new base construction! Need to send builder bot.");
            // TODO: Реализовать отправку строителя
        }
    }

    private void InitializeAndValidateDependencies()
    {
        if (_zoneVisualizer == null)
            Debug.LogError("BaseZoneVisualizer not found in BaseController!");

        if (_botManager == null)
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemSpawner == null)
            Debug.LogError("ItemSpawner not assigned in BotManager!");

        if (_itemCounter == null)
            Debug.LogError("ItemCounter not assigned in BotManager!");

        if (_flagPrefab == null)
            Debug.LogError("FlagPrefab not assigned in BotManager!");

        if (_materialChanger == null)
            Debug.LogError("MaterialChanger not assigned in BotManager!");

        if (_viewTransform == null)
            Debug.LogError("ViewTransform not assigned in BaseController!");
        else
        {
            _originalViewScale = _viewTransform.localScale;
        }
    }

    // TODO вынести логику работы с флагом в отдельный класс
    #region Flag 
    public bool TrySetFlag(Vector3 worldPosition)
    {
        if (CanBuildNewBase == false || IsValidFlagPosition(worldPosition) == false)
            return false;

        if (_currentFlag == null)
        {
            CreateFlag(worldPosition);
        }
        else
        {
            _currentFlag.StartMoving();
            _currentFlag.SetPosition(worldPosition, true);
            _currentFlag.PlaceFlag();
        }

        SetPriority(BasePriority.CollectForNewBase);
        return true;
    }

    public void RemoveFlag() => // ?
        _currentFlag?.RemoveFlag();

    private void CreateFlag(Vector3 position)
    {
        _currentFlag = Instantiate(_flagPrefab);

        if (_currentFlag == null)
            return;

        _currentFlag.Initialize(this);
        _currentFlag.FlagSettled += OnFlagSettled;
        _currentFlag.FlagRemoved += OnFlagRemoved;
        _currentFlag.PlaceFlagDirectly(position);
    }

    private void OnFlagSettled(Vector3 position)//todo
    {
        Debug.Log($"Flag settled at: {position}");
        // Логика при установке флага
    }

    private void OnFlagRemoved()
    {
        SetPriority(BasePriority.CollectForBots);
        Debug.Log("Flag removed");
    }

    private bool IsValidFlagPosition(Vector3 position)
    {
        if (position.y < 0)
            return false;

        float checkRadius = 1f;
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        foreach (var collider in colliders)
        {
            if (collider.isTrigger == false && collider.TryGetComponent<Ground>(out _) == false)
                return false;
        }

        return true;
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