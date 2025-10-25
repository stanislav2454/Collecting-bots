using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager _instance;
    public static ResourceManager Instance => _instance;

    [Header("Resource Settings")]
    [SerializeField] private int _maxActiveResources = 50;

    private HashSet<Item> _allResources = new HashSet<Item>();
    private HashSet<Item> _freeResources = new HashSet<Item>();
    private HashSet<Item> _reservedResources = new HashSet<Item>();

    public event System.Action<Item> ResourceBecameAvailable;
    public event System.Action<Item> ResourceReserved;

    public bool HasAvailableResources => _freeResources.Count > 0;
    public int FreeResourcesCount => _freeResources.Count;
    public int ReservedResourcesCount => _reservedResources.Count;
    public int TotalResourcesCount => _allResources.Count;
    public void DebugResourceState(Item resource)
    {
        Debug.Log($"=== RESOURCE STATE: {resource?.name} ===");
        Debug.Log($"В FreeResources: {_freeResources.Contains(resource)}");
        Debug.Log($"В ReservedResources: {_reservedResources.Contains(resource)}");
        Debug.Log($"CanBeCollected: {resource?.CanBeCollected}");
        Debug.Log($"ActiveInHierarchy: {resource?.gameObject.activeInHierarchy}");
        Debug.Log($"=====================================");
    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public bool TryReserveResource(Item resource)
    {
        Debug.Log($"🔒 ПЫТАЕМСЯ ЗАРЕЗЕРВИРОВАТЬ РЕСУРС: {resource?.name}");

        if (resource == null)
        {
            Debug.Log($"❌ Ресурс null");
            return false;
        }

        if (!_freeResources.Contains(resource))
        {
            Debug.Log($"❌ РЕСУРС {resource.name} НЕ В FREE_RESOURCES! " +
                     $"Свободных: {_freeResources.Count}, " +
                     $"Занятых: {_reservedResources.Count}");
            return false;
        }

        _freeResources.Remove(resource);
        _reservedResources.Add(resource);
        ResourceReserved?.Invoke(resource);

        Debug.Log($"✅ Ресурс зарезервирован: {resource.name}. Свободных: {_freeResources.Count}, Занятых: {_reservedResources.Count}");
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
            ResourceBecameAvailable?.Invoke(resource);
        }
    }

    public void RegisterResource(Item resource)
    {
        Debug.Log($"📝 РЕГИСТРИРУЕМ РЕСУРС: {resource?.name}");

        if (resource == null)
        {
            Debug.Log($"❌ Ресурс null при регистрации");
            return;
        }

        _freeResources.Remove(resource);
        _reservedResources.Remove(resource);

        _allResources.Add(resource);

        if (resource.CanBeCollected && resource.gameObject.activeInHierarchy)
        {
            _freeResources.Add(resource);
            ResourceBecameAvailable?.Invoke(resource);
            Debug.Log($"✅ Ресурс {resource.name} ЗАРЕГИСТРИРОВАН в FreeResources. Свободных: {_freeResources.Count}");
        }
        else
        {
            Debug.Log($"⚠️ Ресурс {resource.name} зарегистрирован, но НЕ добавлен в FreeResources: " +
                     $"CanBeCollected={resource.CanBeCollected}, Active={resource.gameObject.activeInHierarchy}");
        }
    }

    public void MarkAsCollected(Item resource)
    {
        if (resource == null)
            return;

        _freeResources.Remove(resource);
        _reservedResources.Remove(resource);
    }

    public Item GetNearestAvailableResource(Vector3 position)
    {
        Item nearestResource = null;
        float nearestSqrDistance = float.MaxValue;
        int resourcesChecked = 0;

        foreach (var resource in _freeResources)
        {
            if (resource == null || resource.CanBeCollected == false)
            {
                Debug.Log($"   ❌ Ресурс {resource?.name ?? "NULL"} пропущен: CanBeCollected={resource?.CanBeCollected}");
                continue;
            }

            resourcesChecked++;
            Vector3 resourcePos = resource.transform.position;

            float sqrDistance = (resource.transform.position - position).sqrMagnitude;
            Debug.Log($"   🔎 Проверяем ресурс {resource.name}:\n" +
                     $"      📍 Позиция: {resourcePos}\n" +
                     $"      📏 Расстояние до бота: {Mathf.Sqrt(sqrDistance):F2}");
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestResource = resource;
                Debug.Log($"   🎯 НОВЫЙ БЛИЖАЙШИЙ: {resource.name} at {resourcePos}");
            }
        }

        if (nearestResource != null)
            Debug.Log($"🎯 ИТОГ: Ближайший ресурс {nearestResource.name} at {nearestResource.transform.position}");
        else
            Debug.Log($"❌ Ближайший ресурс не найден. Проверено: {resourcesChecked}");

        return nearestResource;
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

#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            string status = _freeResources.Contains(resource) ? "FREE" :
                           _reservedResources.Contains(resource) ? "RESERVED" : "COLLECTED";
            UnityEditor.Handles.Label(resource.transform.position + Vector3.up * 1f,
                                    $"{resource.name}\n{status}\nCanCollect: {resource.CanBeCollected}");
#endif
        }
    }
}