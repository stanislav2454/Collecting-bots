using UnityEngine;
using System.Collections.Generic;
using System;

public class BaseController : ZoneVisualizer
{
    [Header("Base Settings")]
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private BotController _botPrefab;

    [Header("Dependencies")]
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ResourceScanner _resourceScanner;

    [Header("Base Zones - ЛОГИКА")]
    [SerializeField] private float _spawnZoneRadius = 3f;
    [SerializeField] private float _unloadZoneRadius = 1.5f;

    [Header("Base Zones - ВИЗУАЛИЗАЦИЯ")]
    [SerializeField] private bool _showZones = true;
    [SerializeField] private Color _spawnZoneColor = Color.blue;
    [SerializeField] private Color _unloadZoneColor = Color.yellow;

    private List<BotController> _bots = new List<BotController>();
    private List<Item> _availableResources = new List<Item>();
    private int _collectedResources;
    private ZoneVisualizer _spawnZoneVisualizer;
    private ZoneVisualizer _unloadZoneVisualizer;
    private Dictionary<Item, BotController> _reservedItems = new Dictionary<Item, BotController>();

    public event Action<Item> ResourceScanned;
    public event Action<int> ResourceCollected;
    public event Action<BotController> BotAssigned;

    public Vector3 SpawnZoneSize => new Vector3(_spawnZoneRadius * 2f, 0.1f, _spawnZoneRadius * 2f);
    public Vector3 UnloadZoneSize => new Vector3(_unloadZoneRadius * 2f, 0.1f, _unloadZoneRadius * 2f);

    private void Start()
    {
        CreateZoneVisuals();
        SpawnInitialBots();

        if (_itemSpawner != null)
            _itemSpawner.ItemSpawned += HandleNewResource;

        if (_resourceScanner != null)
        {
            _resourceScanner.ResourceFound += HandleNewResource;
            _resourceScanner.ResourceLost += HandleResourceLost;
            _resourceScanner.StartScanning();
        }
    }

    private void OnDestroy()
    {
        if (_itemSpawner != null)
            _itemSpawner.ItemSpawned -= HandleNewResource;

        if (_resourceScanner != null)
        {
            _resourceScanner.ResourceFound -= HandleNewResource;
            _resourceScanner.StopScanning();
        }
    }

    public Vector3 GetRandomUnloadPosition()
    {
        Vector3 randomPoint = transform.position + new Vector3(
                       UnityEngine.Random.Range(-_unloadZoneRadius, _unloadZoneRadius),
                       0,
                       UnityEngine.Random.Range(-_unloadZoneRadius, _unloadZoneRadius));

        return randomPoint;
    }

    public void ReleaseResource(Item resource)
    {
        if (_reservedItems.ContainsKey(resource))
        {
            var bot = _reservedItems[resource];
            _reservedItems.Remove(resource);

            if (bot != null && bot.AssignedResource == resource && bot.IsCarryingResource == false)
                bot.CompleteMission(false);
        }

        if (_availableResources.Contains(resource))
            _availableResources.Remove(resource);
    }

    private void HandleResourceLost(Item resource) =>
        ReleaseResource(resource);

    public void ReturnResourceToPool(Item resource)
    {
        if (resource != null)
        {
            if (_reservedItems.ContainsKey(resource))
            {
                var bot = _reservedItems[resource];
                _reservedItems.Remove(resource);

                if (bot != null && bot.AssignedResource == resource)
                    bot.CompleteMission(false);
            }

            if (_availableResources.Contains(resource))
                _availableResources.Remove(resource);

            _itemSpawner?.ReturnItemToPool(resource);
        }
    }

    private void HandleNewResource(Item item)
    {
        if (_availableResources.Contains(item) == false)
        {
            _availableResources.Add(item);
            ResourceScanned?.Invoke(item);
            AssignBotToResource(item);
        }
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            BotController botObject = Instantiate(_botPrefab, spawnPosition, Quaternion.identity);

            if (botObject.TryGetComponent(out BotController bot))
                InitializeBot(bot);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return transform.position + new Vector3(
            UnityEngine.Random.Range(-_spawnZoneRadius, _spawnZoneRadius),
            0,
            UnityEngine.Random.Range(-_spawnZoneRadius, _spawnZoneRadius));
    }

    private void InitializeBot(BotController bot)
    {
        bot.AssignBase(this);
        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);
        BotAssigned?.Invoke(bot);
    }

    private void AssignBotToResource(Item resource)
    {
        if (IsResourceAssignable(resource) == false)
            return;

        for (int i = 0; i < _bots.Count; i++)
        {
            var bot = _bots[i];
            if (bot.IsAvailable)
            {
                _reservedItems[resource] = bot;
                bot.AssignResource(resource);
                return;
            }
        }
    }

    private bool IsResourceAssignable(Item resource)
    {
        if (resource == null)
            return false;

        if (resource.CanBeCollected == false)
            return false;

        if (resource.gameObject.activeInHierarchy == false)
            return false;

        if (_reservedItems.ContainsKey(resource))
            return false;


        return true;
    }

    private void HandleBotMissionCompleted(BotController bot, bool success)
    {
        if (success)
        {
            _collectedResources++;
            ResourceCollected?.Invoke(_collectedResources);
        }

        var reservedItems = new List<Item>();

        foreach (var kvp in _reservedItems)
            if (kvp.Value == bot)
                reservedItems.Add(kvp.Key);

        foreach (var item in reservedItems)
            _reservedItems.Remove(item);

        AssignNextResourceToBot(bot);
    }

    private void AssignNextResourceToBot(BotController bot)
    {
        foreach (var resource in _availableResources)
        {
            if (resource != null && resource.CanBeCollected &&
                _reservedItems.ContainsKey(resource) == false)
            {
                _reservedItems[resource] = bot;
                bot.AssignResource(resource);
                return;
            }
        }

        bot.SetWaiting();
    }

    private void CreateZoneVisuals()
    {
        _spawnZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        SetupZoneVisualizer(_spawnZoneVisualizer, _spawnZoneColor, SpawnZoneSize, Vector3.zero);

        _unloadZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        SetupZoneVisualizer(_unloadZoneVisualizer, _unloadZoneColor, UnloadZoneSize, Vector3.zero);

        UpdateZoneVisibility();
    }

    private void SetupZoneVisualizer(ZoneVisualizer visualizer, Color color,
                                Vector3 size, Vector3 offset) =>
        visualizer.CreateOrUpdateZone(size, offset);

    private void UpdateZoneVisibility()
    {
        if (_spawnZoneVisualizer != null)
            _spawnZoneVisualizer.SetZoneVisible(_showZones);

        if (_unloadZoneVisualizer != null)
            _unloadZoneVisualizer.SetZoneVisible(_showZones);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && _spawnZoneVisualizer != null)
        {
            _spawnZoneVisualizer.CreateOrUpdateZone(
                new Vector3(_spawnZoneRadius * 2f, 0.1f, _spawnZoneRadius * 2f), Vector3.zero);

            _unloadZoneVisualizer.CreateOrUpdateZone(
                new Vector3(_unloadZoneRadius * 2f, 0.1f, _unloadZoneRadius * 2f), Vector3.zero);
        }
    }
#endif
}