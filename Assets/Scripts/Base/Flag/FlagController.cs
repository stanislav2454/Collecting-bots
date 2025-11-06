using UnityEngine;
using System;

public class FlagController : MonoBehaviour
{
    [Header("Flag System")]
    [SerializeField] private Flag _flagPrefab;
    [SerializeField] private BaseController _baseController;
    [SerializeField] private BasePriorityController _priorityController;

    private Flag _currentFlag;

    public event Action<Vector3> FlagPlaced;
    public event Action FlagRemoved;

    public bool HasActiveFlag => _currentFlag != null && _currentFlag.CurrentState != FlagState.Hide;
    public Vector3 FlagPosition => _currentFlag != null ? _currentFlag.Position : Vector3.zero;

    private void Start()
    {
        if (_priorityController != null)
            _priorityController.PriorityChanged += OnPriorityChanged;
        else
            Debug.LogError("FlagController: PriorityController not assigned!");
    }

    private void OnDestroy()
    {
        if (_priorityController != null)
            _priorityController.PriorityChanged -= OnPriorityChanged;

        if (_currentFlag != null)
        {
            _currentFlag.FlagSettled -= OnFlagSettled;
            _currentFlag.FlagRemoved -= OnFlagRemoved;
        }
    }

    public bool TrySetFlag(Vector3 worldPosition)
    {
        if (_baseController == null || _baseController.CanBuildNewBase == false ||
                IsValidFlagPosition(worldPosition) == false)
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

        _priorityController.SetPriority(BasePriority.CollectForNewBase);

        return true;
    }

    public void RemoveFlag()
    {
        _currentFlag?.RemoveFlag();
    }

    private void CreateFlag(Vector3 position)
    {
        _currentFlag = Instantiate(_flagPrefab);

        if (_currentFlag == null)
            return;

        _currentFlag.Initialize(_baseController);
        _currentFlag.FlagSettled += OnFlagSettled;
        _currentFlag.FlagRemoved += OnFlagRemoved;
        _currentFlag.PlaceFlagDirectly(position);
    }

    private void OnFlagSettled(Vector3 position)
    {
        FlagPlaced?.Invoke(position);
    }

    private void OnFlagRemoved()
    {
        _priorityController.SetPriority(BasePriority.CollectForBots);
        FlagRemoved?.Invoke();
    }

    private void OnPriorityChanged(BasePriority newPriority)
    {
        if (newPriority == BasePriority.CollectForBots && _currentFlag != null)
            RemoveFlag();
    }

    private bool IsValidFlagPosition(Vector3 position)
    {
        if (position.y < 0)
            return false;

        float checkRadius = 1f;
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        foreach (var collider in colliders)
            if (collider.isTrigger == false && collider.TryGetComponent<Ground>(out _) == false)
                return false;

        return true;
    }
}