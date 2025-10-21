using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static readonly Dictionary<Type, object> _pendingRegistrations = new Dictionary<Type, object>();
    private static bool _isInitializing = false;

    /// <summary>
    /// Регистрирует сервис в локаторе
    /// </summary>
    public static void Register<T>(T service) where T : class
    {
        if (service == null)
        {
            Debug.LogError($"Attempted to register null service of type {typeof(T)}");
            return;
        }

        Type serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"Service of type {serviceType} already registered. Replacing with new instance.");
            _services[serviceType] = service;
        }
        else
        {
            _services.Add(serviceType, service);
            Debug.Log($"Service registered: {serviceType.Name}");
        }

        // Проверяем есть ли ожидающие регистрации для этого типа
        if (_pendingRegistrations.ContainsKey(serviceType))
        {
            _pendingRegistrations.Remove(serviceType);
        }
    }

    /// <summary>
    /// Регистрирует сервис асинхронно (для случаев когда сервис может быть еще не создан)
    /// </summary>
    public static void RegisterAsync<T>(T service) where T : class
    {
        if (_isInitializing)
        {
            _pendingRegistrations[typeof(T)] = service;
        }
        else
        {
            Register(service);
        }
    }

    /// <summary>
    /// Получает сервис из локатора
    /// </summary>
    public static T Get<T>() where T : class
    {
        Type serviceType = typeof(T);

        if (_services.TryGetValue(serviceType, out object service))
        {
            return (T)service;
        }

        // Проверяем ожидающие регистрации
        if (_pendingRegistrations.TryGetValue(serviceType, out object pendingService))
        {
            return (T)pendingService;
        }

        // Пытаемся найти сервис в сцене
        T sceneService = FindServiceInScene<T>();
        if (sceneService != null)
        {
            Register(sceneService);
            return sceneService;
        }

        throw new ServiceNotFoundException($"Service of type {serviceType} not registered in ServiceLocator");
    }

    /// <summary>
    /// Пытается получить сервис без выброса исключения
    /// </summary>
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

    /// <summary>
    /// Проверяет зарегистрирован ли сервис
    /// </summary>
    public static bool IsRegistered<T>() where T : class
    {
        Type serviceType = typeof(T);
        return _services.ContainsKey(serviceType) || _pendingRegistrations.ContainsKey(serviceType);
    }

    /// <summary>
    /// Удаляет сервис из локатора
    /// </summary>
    public static void Unregister<T>() where T : class
    {
        Type serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
        {
            _services.Remove(serviceType);
            Debug.Log($"Service unregistered: {serviceType.Name}");
        }

        if (_pendingRegistrations.ContainsKey(serviceType))
        {
            _pendingRegistrations.Remove(serviceType);
        }
    }

    /// <summary>
    /// Очищает все зарегистрированные сервисы
    /// </summary>
    public static void Clear()
    {
        Debug.Log($"Clearing ServiceLocator: {_services.Count} services, {_pendingRegistrations.Count} pending");
        _services.Clear();
        _pendingRegistrations.Clear();
    }

    /// <summary>
    /// Начинает процесс инициализации (для асинхронной регистрации)
    /// </summary>
    public static void BeginInitialization()
    {
        _isInitializing = true;
        _pendingRegistrations.Clear();
    }

    /// <summary>
    /// Завершает процесс инициализации и регистрирует все ожидающие сервисы
    /// </summary>
    public static void EndInitialization()
    {
        _isInitializing = false;

        foreach (var kvp in _pendingRegistrations)
        {
            Register(kvp.Value);
        }
        _pendingRegistrations.Clear();
    }

    /// <summary>
    /// Получает информацию о всех зарегистрированных сервисах (для отладки)
    /// </summary>
    public static string GetServicesInfo()
    {
        var info = "=== ServiceLocator Info ===\n";
        info += $"Registered Services: {_services.Count}\n";

        foreach (var kvp in _services)
        {
            info += $"- {kvp.Key.Name}: {kvp.Value}\n";
        }

        info += $"\nPending Registrations: {_pendingRegistrations.Count}\n";
        foreach (var kvp in _pendingRegistrations)
        {
            info += $"- {kvp.Key.Name}: {kvp.Value}\n";
        }

        return info;
    }

    private static T FindServiceInScene<T>() where T : class
    {
        // Ищем MonoBehaviour реализации в сцене
        MonoBehaviour[] sceneObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

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

/// <summary>
/// Исключение когда сервис не найден
/// </summary>
public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(string message) : base(message) { }
    public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}