using UnityEngine;

public class MissionControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BotController _botManager;
    [SerializeField] private ResourceAllocator _resourceAllocator;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (_resourceAllocator != null)
            _resourceAllocator.ResourceBecameAvailable -= OnResourceBecameAvailable;
    }

    public void OnResourceBecameAvailable(Item resource)
    {
        if (_botManager != null)
            _botManager.AssignBotToResource(resource);
    }

    public void SetDependencies(BotController botManager, ResourceAllocator resourceManager)
    {
        _botManager = botManager;
        _resourceAllocator = resourceManager;

        if (_resourceAllocator != null)
        {
            _resourceAllocator.ResourceBecameAvailable -= OnResourceBecameAvailable;
            _resourceAllocator.ResourceBecameAvailable += OnResourceBecameAvailable;
        }
    }

    private void Initialize()
    {
        if (_resourceAllocator != null)
            _resourceAllocator.ResourceBecameAvailable += OnResourceBecameAvailable;
    }
}
