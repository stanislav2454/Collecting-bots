using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static readonly Dictionary<Type, object> _pendingRegistrations = new Dictionary<Type, object>();
    private static bool _isInitializing = false;

    public static void Register<T>(T service) where T : class
    {
        if (service == null)
        {
            Debug.LogError($"Attempted to register null service of type {typeof(T)}");
            return;
        }

        Type serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
            _services[serviceType] = service;
        else
            _services.Add(serviceType, service);

        if (_pendingRegistrations.ContainsKey(serviceType))
            _pendingRegistrations.Remove(serviceType);
    }

    public static void RegisterAsync<T>(T service) where T : class// зачем нужен ? если не используется - удалить !
    {
        if (_isInitializing)
            _pendingRegistrations[typeof(T)] = service;
        else
            Register(service);
    }

    public static T Get<T>() where T : class
    {
        Type serviceType = typeof(T);

        if (_services.TryGetValue(serviceType, out object service))
            return (T)service;

        if (_pendingRegistrations.TryGetValue(serviceType, out object pendingService))
            return (T)pendingService;

        T sceneService = FindServiceInScene<T>();

        if (sceneService != null)
        {
            Register(sceneService);
            return sceneService;
        }

        throw new ServiceNotFoundException($"Service of type {serviceType} not registered in ServiceLocator");
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        try
        {
            service = Get<T>();
            return true;
        }
        catch (ServiceNotFoundException)
        {
            service = null;
            return false;
        }
    }

    public static bool IsRegistered<T>() where T : class// зачем нужен ? если не используется - удалить !
    {
        Type serviceType = typeof(T);
        return _services.ContainsKey(serviceType) || _pendingRegistrations.ContainsKey(serviceType);
    }

    public static void Unregister<T>() where T : class// зачем нужен ? если не используется - удалить !
    {
        Type serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
            _services.Remove(serviceType);

        if (_pendingRegistrations.ContainsKey(serviceType))
            _pendingRegistrations.Remove(serviceType);
    }

    public static void Clear()// зачем нужен ? если не используется - удалить !
    {
        _services.Clear();
        _pendingRegistrations.Clear();
    }

    public static void BeginInitialization()// зачем нужен ? если не используется - удалить !
    {
        _isInitializing = true;
        _pendingRegistrations.Clear();
    }

    public static void EndInitialization()// зачем нужен ? если не используется - удалить !
    {
        _isInitializing = false;

        foreach (var kvp in _pendingRegistrations)
            Register(kvp.Value);

        _pendingRegistrations.Clear();
    }

    public static string GetServicesInfo()// зачем нужен ? если не используется - удалить !
    {
        var info = "=== ServiceLocator Info ===\n";
        info += $"Registered Services: {_services.Count}\n";

        foreach (var kvp in _services)
            info += $"- {kvp.Key.Name}: {kvp.Value}\n";

        info += $"\nPending Registrations: {_pendingRegistrations.Count}\n";

        foreach (var kvp in _pendingRegistrations)
            info += $"- {kvp.Key.Name}: {kvp.Value}\n";

        return info;
    }

    private static T FindServiceInScene<T>() where T : class
    {
        MonoBehaviour[] sceneObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();//ресурсозатратно и ненадежно => переделать на передачу ссылки 

        foreach (var obj in sceneObjects)
        {
            if (obj is T service)
            {
                Debug.LogWarning($"Found service {typeof(T).Name} in scene: {obj.name}. Auto-registering.");
                return service;
            }
        }

        return null;
    }
}

public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(string message) : base(message) { }
    public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}