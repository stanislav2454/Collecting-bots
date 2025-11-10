using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    [Header("Base Prefab")]
    [SerializeField] private BaseController _basePrefab;
    [SerializeField] private Transform _basesContainer;

    [Header("Dependencies")]
    [SerializeField] private ResourceAllocator _resourceAllocator;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private BaseSelector _baseSelector;
    [SerializeField] private BaseConstructor _baseConstructor;
    [SerializeField] private Camera _mainCamera;

    public BaseController CreateBase(Vector3 position)
    {
        if (_basePrefab == null)
        {
            Debug.LogError("Base prefab is not assigned in BaseFactory!");
            return null;
        }

        BaseController newBase = Instantiate(_basePrefab, position, Quaternion.identity, _basesContainer);
        newBase.name = $"Base_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        SetupNewBaseDependencies(newBase);

        return newBase;
    }

    private void SetupNewBaseDependencies(BaseController newBase)
    {
        if (_resourceAllocator == null)
        {
            Debug.LogError("ResourceManager not assigned in BaseFactory!");
            return;
        }

        if (_itemSpawner == null)
        {
            Debug.LogError("ItemSpawner not assigned in BaseFactory!");
            return;
        }

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        _baseSelector.RegisterBase(newBase);

        newBase.Initialize(_itemSpawner, _resourceAllocator);

        var priorityController = newBase.GetComponent<BasePriorityController>();
        if (priorityController != null)
            priorityController.SetConstructionManager(_baseConstructor);

        var missionControl = newBase.GetComponent<MissionControl>();
        var botManager = newBase.GetComponentInChildren<BotController>();

        if (missionControl != null && botManager != null)
            missionControl.SetDependencies(botManager, _resourceAllocator);

        var canvasLookAt = newBase.GetComponentInChildren<CanvasLookAtCamera>();
        if (canvasLookAt != null && _mainCamera != null)
            canvasLookAt.SetTargetCamera(_mainCamera);
    }
}
