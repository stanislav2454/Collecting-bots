using UnityEngine;

public class MissionControl : MonoBehaviour
{//(бывший ResourceCoordinator/MissionDispatcher)
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ResourceManager _resourceManager; // Внедренная зависимость

    private void Start()
    {
        if (_resourceManager != null)
            _resourceManager.ResourceBecameAvailable += OnResourceBecameAvailable;
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
}
