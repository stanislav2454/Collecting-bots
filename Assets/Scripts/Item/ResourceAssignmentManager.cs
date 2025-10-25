using UnityEngine;
using System.Collections.Generic;

public class ResourceAssignmentManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ResourceManager _resourceManager;

    private Dictionary<Item, Bot> _itemToBotMap = new Dictionary<Item, Bot>();
    private Dictionary<Bot, Item> _botToItemMap = new Dictionary<Bot, Item>();

    private void Start()
    {
        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in ResourceAssignmentManager!");
    }

    public bool TryAssignResourceToBot(Item resource, Bot bot)
    {
        if (resource == null || bot == null)
            return false;

        if (_itemToBotMap.ContainsKey(resource))
            return false;

        if (_botToItemMap.ContainsKey(bot))
            return false;

        if (_resourceManager.TryReserveResource(resource))
        {
            _itemToBotMap[resource] = bot;
            _botToItemMap[bot] = resource;
            return true;
        }

        return false;
    }

    public void CompleteAssignment(Bot bot, bool success)
    {
        if (_botToItemMap.TryGetValue(bot, out Item resource))
        {
            _botToItemMap.Remove(bot);
            _itemToBotMap.Remove(resource);

            if (success)
                _resourceManager?.MarkAsCollected(resource);
            else
                _resourceManager?.ReleaseResource(resource);
        }
    }

    public void ForceReleaseBotAssignment(Bot bot)// где и для чего используются эти методы ?
    {
        if (_botToItemMap.TryGetValue(bot, out Item resource))
        {
            _botToItemMap.Remove(bot);
            _itemToBotMap.Remove(resource);
            _resourceManager?.ReleaseResource(resource);
        }
    }

    public Item GetAssignedResource(Bot bot)// где и для чего используются эти методы ?
    {
        _botToItemMap.TryGetValue(bot, out Item resource);
        return resource;
    }

    public bool IsBotAssigned(Bot bot) =>
        _botToItemMap.ContainsKey(bot);

    public bool IsResourceAssigned(Item resource) =>
        _itemToBotMap.ContainsKey(resource);
}