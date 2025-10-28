using System;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] private int _resourcesForBot = 3;

    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ResourceManager _resourceManager;

    public event Action<int> AmountResourcesChanged;

    public float UnloadZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.SpawnZoneRadius : 3f;
    public int CollectedResources { get; private set; }
    public bool CanAffordBot => CollectedResources >= _resourcesForBot;

    private void Start()
    {
        InitializeDependencies();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Bot>(out var bot))
        {
            if (bot.IsCarryingResource)
            {
                CollectResourceFromBot(bot);
                bot.CompleteMission(true);
            }
        }
    }

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;
        if (item != null)
        {
            CollectedResources += item.Value;
            AmountResourcesChanged?.Invoke(CollectedResources);

            if (CanAffordBot)
                CreateBotFromResources();
        }

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    private void CreateBotFromResources()
    {
        if (_botManager == null)
            return;

        CollectedResources -= _resourcesForBot;
        AmountResourcesChanged?.Invoke(CollectedResources);
        _botManager.CreateNewBot();
    }

    private void InitializeDependencies()
    {
        if (TryGetComponent(out _zoneVisualizer) == false)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();

        if (TryGetComponent(out _botManager) == false)
            _botManager = GetComponentInChildren<BotManager>();
        else
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemSpawner == null)
            Debug.LogError("ItemSpawner not assigned in BotManager!");

        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in BotManager!");
    }
}