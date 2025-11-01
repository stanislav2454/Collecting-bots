using UnityEngine;

public class MissionControl : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ResourceManager _resourceManager;

    private void Start()
    {
        // ЗАМЕНА: если ResourceManager не назначен в инспекторе, получаем через GameDependencies
        if (_resourceManager == null)
        {
            var gameDependencies = GameDependencies.Instance;
            if (gameDependencies != null)
                _resourceManager = gameDependencies.ResourceManager;
        }

        if (_resourceManager != null)
            _resourceManager.ResourceBecameAvailable += OnResourceBecameAvailable;
        else
            Debug.LogError("ResourceManager not found in MissionControl!");
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
