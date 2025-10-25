using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class BaseController : ZoneVisualizer
{
    [Header("Base Settings")]
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private Bot _botPrefab;

    [Header("Dependencies")]
    [SerializeField] private ItemSpawner _itemSpawner;

    [Header("Base Zones")]
    [SerializeField] private float _spawnZoneRadius = 3f;
    [SerializeField] private float _unloadZoneRadius = 1.5f;
    [SerializeField] private bool _showZones = true;

    private List<Bot> _bots = new List<Bot>();
    private int _collectedResources;
    private ZoneVisualizer _spawnZoneVisualizer;
    private ZoneVisualizer _unloadZoneVisualizer;
    private ResourceAssignmentManager _assignmentManager = new ResourceAssignmentManager();
    private float _debugTimer;

    public event System.Action<int> ResourceCollected;
    public event System.Action<Bot> BotCreated;

    public Vector3 BasePosition => transform.position;
    public float UnloadZoneRadius => _unloadZoneRadius;

    private void Start()
    {
        CreateZoneVisuals();
        SpawnInitialBots();
        ResourceManager.Instance.ResourceBecameAvailable += OnResourceBecameAvailable;
    }

    private void Update()
    {
        _debugTimer += Time.deltaTime;
        if (_debugTimer >= 5f)
        {
            DebugLogBaseState();
            _debugTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.ResourceBecameAvailable -= OnResourceBecameAvailable;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Bot>(out var bot))
        {
            if (bot.IsCarryingResource)
            {
                Debug.Log($"Бот вошел в зону разгрузки с ресурсом");
                CollectResourceFromBot(bot);
                bot.CompleteMission(true);
            }
        }
    }

    public void OnResourceBecameAvailable(Item resource) =>
        AssignBotToResource(resource);

    public Vector3 GetRandomUnloadPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-_unloadZoneRadius, _unloadZoneRadius),
            0,
            Random.Range(-_unloadZoneRadius, _unloadZoneRadius));
    }

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource)
        {
            var item = bot.Inventory.GetCarriedItem;
            if (item != null)
            {
                _collectedResources += item.GetValue;
                ResourceCollected?.Invoke(_collectedResources);

                ResourceManager.Instance.MarkAsCollected(item);
                Debug.Log($"Собран ресурс. Всего: {_collectedResources}");
            }

            bot.Inventory.ClearInventory();
            _itemSpawner?.ReturnItemToPool(item);
        }
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            SpawnBot();
    }

    private void SpawnBot()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Bot bot = Instantiate(_botPrefab, spawnPosition, Quaternion.identity);
        InitializeBot(bot);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-_spawnZoneRadius, _spawnZoneRadius),
            0,
            Random.Range(-_spawnZoneRadius, _spawnZoneRadius));
    }

    private void InitializeBot(Bot bot)
    {
        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);
        BotCreated?.Invoke(bot);

        if (ResourceManager.Instance.HasAvailableResources)
            AssignResourceToBot(bot);
    }

    private void AssignResourceToBot(Bot bot)
    {
        if (bot.IsAvailable && _assignmentManager.IsBotAssigned(bot) == false)
        {
            Item resource = ResourceManager.Instance.GetNearestAvailableResource(bot.transform.position);
            if (resource != null && _assignmentManager.TryAssignResourceToBot(resource, bot))
            {
                Debug.Log($"Боту назначен ресурс: {resource.name} at {resource.transform.position}");
                bot.AssignResource(resource, BasePosition, _unloadZoneRadius);

            }
        }
    }

    private void AssignBotToResource(Item resource)
    {

        if (resource == null || _assignmentManager.IsResourceAssigned(resource))
        {
            Debug.Log($"❌ Ресурс {resource?.name ?? "NULL"} не может быть назначен");
            return;
        }


        var nearestBot = _bots
            .Where(b => b.IsAvailable && !_assignmentManager.IsBotAssigned(b))
            .OrderBy(b => (b.transform.position - resource.transform.position).sqrMagnitude)
            .FirstOrDefault();

        if (nearestBot != null && _assignmentManager.TryAssignResourceToBot(resource, nearestBot))
            nearestBot.AssignResource(resource, BasePosition, _unloadZoneRadius);
    }

    private void HandleBotMissionCompleted(Bot bot, bool success)
    {
        _assignmentManager.CompleteAssignment(bot, success);

        if (success)
            CollectResourceFromBot(bot);

        StartCoroutine(AssignNewResourceAfterDelay(bot, 0.5f));
    }

    private IEnumerator AssignNewResourceAfterDelay(Bot bot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ResourceManager.Instance?.FreeResourcesCount > 0)
            AssignResourceToBot(bot);
        else
            bot.SetWaiting();
    }

    private void DebugLogBaseState()
    {
        int availableBots = _bots.Count(b => b.IsAvailable);
    }

    private void CreateZoneVisuals()
    {
        _spawnZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        _unloadZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        UpdateZoneVisibility();
    }

    private void UpdateZoneVisibility()
    {
        if (_spawnZoneVisualizer != null)
            _spawnZoneVisualizer.SetZoneVisible(_showZones);
        if (_unloadZoneVisualizer != null)
            _unloadZoneVisualizer.SetZoneVisible(_showZones);
    }
}