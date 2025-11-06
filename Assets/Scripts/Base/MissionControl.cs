using UnityEngine;

public class MissionControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ResourceManager _resourceManager;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (_resourceManager != null)
            _resourceManager.ResourceBecameAvailable -= OnResourceBecameAvailable;
    }

    public void OnResourceBecameAvailable(Item resource)
    {
        if (_botManager != null)
            _botManager.AssignBotToResource(resource);
    }

    public void SetDependencies(BotManager botManager, ResourceManager resourceManager)
    {
        _botManager = botManager;
        _resourceManager = resourceManager;

        if (_resourceManager != null)
        {
            _resourceManager.ResourceBecameAvailable -= OnResourceBecameAvailable;
            _resourceManager.ResourceBecameAvailable += OnResourceBecameAvailable;
        }
    }

    private void Initialize()
    {
        if (_resourceManager != null)
            _resourceManager.ResourceBecameAvailable += OnResourceBecameAvailable;
    }
}
