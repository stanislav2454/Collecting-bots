using UnityEngine;
using System;

public class BaseFlagController : MonoBehaviour
{
    [Header("Flag System")]
    [SerializeField] private SimpleFlag _flagPrefab;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private BasePriorityController _priorityController;

    private SimpleFlag _currentFlag;

    public event Action<Vector3> FlagSettled;
    public event Action FlagRemoved;

    public bool HasActiveFlag => _currentFlag != null && _currentFlag.CurrentState != FlagState.Hide;
    public Vector3 FlagPosition => _currentFlag != null ? _currentFlag.Position : Vector3.zero;

    public bool TrySetFlag(Vector3 worldPosition)
    {
        if (_botManager == null || _botManager.BotCount <= 1 || IsValidFlagPosition(worldPosition) == false)
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
        _priorityController?.SetPriority(BasePriority.CollectForNewBase);

        return true;
    }

    public void RemoveFlag()
    {
        _currentFlag?.RemoveFlag();
    }

    public void StartBaseConstruction(Bot builderBot)
    {
        if (builderBot == null || _currentFlag == null)
            return;

        Debug.Log($"Starting base construction at {_currentFlag.Position}");
        builderBot.BuildBase(_currentFlag.Position, GetComponent<BaseController>());
        _currentFlag.SetDeliveryState();
    }

    public void OnBaseConstructionCompleted()
    {
        RemoveFlag();
        Debug.Log("Base construction completed successfully!");
    }

    private void CreateFlag(Vector3 position)
    {
        _currentFlag = Instantiate(_flagPrefab);
        if (_currentFlag == null) return;

        _currentFlag.Initialize(GetComponent<BaseController>());
        _currentFlag.FlagSettled += OnFlagSettled;
        _currentFlag.FlagRemoved += OnFlagRemoved;
        _currentFlag.PlaceFlagDirectly(position);
    }

    private void OnFlagSettled(Vector3 position)
    {
        Debug.Log($"Flag settled at: {position}");
        FlagSettled?.Invoke(position);
    }

    private void OnFlagRemoved()
    {
        _priorityController?.SetPriority(BasePriority.CollectForBots);
        FlagRemoved?.Invoke();
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
            if (collider.isTrigger == false && !collider.TryGetComponent<Ground>(out _))
                return false;
        }

        return true;
    }

    private void OnValidate()
    {
        if (_flagPrefab == null)
            Debug.LogError("FlagPrefab not assigned in BaseFlagController!");

        if (_botManager == null)
            Debug.LogWarning("BotManager not assigned in BaseFlagController!");

        if (_itemCounter == null)
            Debug.LogWarning("ItemCounter not assigned in BaseFlagController!");

        if (_priorityController == null)
            Debug.LogWarning("BasePriorityController not assigned in BaseFlagController!");
    }

    private void OnDestroy()
    {
        if (_currentFlag != null)
        {
            _currentFlag.FlagSettled -= OnFlagSettled;
            _currentFlag.FlagRemoved -= OnFlagRemoved;
        }

        FlagSettled = null;
        FlagRemoved = null;
    }
}