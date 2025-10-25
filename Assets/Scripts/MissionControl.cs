using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionControl : MonoBehaviour
{//(бывший ResourceCoordinator/MissionDispatcher)
    [Header("Dependencies")]
    [SerializeField] private BotManager _botManager;

    private void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResourceBecameAvailable += OnResourceBecameAvailable;
        else
            Debug.LogError("ResourceManager Instance not found!");
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResourceBecameAvailable -= OnResourceBecameAvailable;
    }

    public void OnResourceBecameAvailable(Item resource)
    {
        if (_botManager != null)
            _botManager.AssignBotToResource(resource);
        else
            Debug.LogError("BotManager not assigned in ResourceCoordinator!");
    }
}
