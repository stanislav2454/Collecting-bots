using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private int _maxActiveResources = 50;

    private HashSet<Item> _allResources = new HashSet<Item>();
    private HashSet<Item> _freeResources = new HashSet<Item>();
    private HashSet<Item> _reservedResources = new HashSet<Item>();
    private Dictionary<Item, Vector3> _resourcePositions = new Dictionary<Item, Vector3>();

    public event System.Action<Item> ResourceBecameAvailable;
    public event System.Action<Item> ResourceReserved;

    public bool HasAvailableResources => _freeResources.Count > 0;
    public int FreeResourcesCount => _freeResources.Count;
    public int ReservedResourcesCount => _reservedResources.Count;
    public int TotalResourcesCount => _allResources.Count;

    public bool TryReserveResource(Item resource)
    {
        if (resource == null)
            return false;

        if (_freeResources.Contains(resource) == false)
            return false;

        _freeResources.Remove(resource);
        _reservedResources.Add(resource);
        ResourceReserved?.Invoke(resource);

        return true;
    }

    public void ReleaseResource(Item resource)
    {
        if (resource == null)
            return;

        _reservedResources.Remove(resource);

        if (resource.gameObject.activeInHierarchy && resource.CanBeCollected)
        {
            _freeResources.Add(resource);
            UpdateResourcePosition(resource);
            ResourceBecameAvailable?.Invoke(resource);
        }
    }

    public void RegisterResource(Item resource)
    {
        if (resource == null)
            return;

        _freeResources.Remove(resource);
        _reservedResources.Remove(resource);
        _allResources.Add(resource);

        if (resource.CanBeCollected && resource.gameObject.activeInHierarchy)
        {
            _freeResources.Add(resource);
            UpdateResourcePosition(resource);
            ResourceBecameAvailable?.Invoke(resource);
        }
    }

    public void MarkAsCollected(Item resource)
    {
        if (resource == null)
            return;

        _freeResources.Remove(resource);
        _reservedResources.Remove(resource);
        _resourcePositions.Remove(resource);
    }

    public Item GetNearestAvailableResource(Vector3 position)
    {
        Item nearestResource = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (var resource in _freeResources)
        {
            if (resource == null || resource.CanBeCollected == false)
                continue;

            if (_resourcePositions.TryGetValue(resource, out Vector3 resourcePosition))
            {
                float sqrDistance = (resourcePosition - position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestResource = resource;
                }
            }
        }

        return nearestResource;
    }

    private void UpdateResourcePosition(Item resource)
    {
        if (resource != null)
            _resourcePositions[resource] = resource.transform.position;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        foreach (var resource in _allResources)
        {
            if (resource == null)
                continue;

            Color color = _freeResources.Contains(resource) ? Color.green :
                         _reservedResources.Contains(resource) ? Color.yellow : Color.red;

            Gizmos.color = color;
            Gizmos.DrawWireSphere(resource.transform.position + Vector3.up * 0.5f, 0.3f);
        }
    }
}