using UnityEngine;

public class BaseController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private int _initialBotsCount = 3;

    [Header("Dependencies")]
    [SerializeField] private BaseZoneVisualizer _zoneVisualizer;
    [SerializeField] private BotManager _botManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ResourceManager _resourceManager;

    private int _collectedResources;

    public event System.Action<int> ResourceCollected;
    public event System.Action<Bot> BotCreated;

    public float UnloadZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.SpawnZoneRadius : 3f;

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
        if (bot.IsCarryingResource)
        {
            var item = bot.Inventory.GetCarriedItem;
            if (item != null)
            {
                _collectedResources += item.GetValue;
                ResourceCollected?.Invoke(_collectedResources);
                _resourceManager?.MarkAsCollected(item);
            }

            bot.Inventory.ClearInventory();
            _itemSpawner?.ReturnItemToPool(item);
        }
    }

    private void InitializeDependencies()
    {
        if (TryGetComponent(out _zoneVisualizer) == false)
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();

        if (TryGetComponent(out _botManager) == false)
        {
            _botManager = GetComponentInChildren<BotManager>();
            _botManager.BotCreated += OnBotCreated;
        }
        else
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemSpawner == null)
            Debug.LogError("ItemSpawner not assigned in BotManager!");

        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in BotManager!");
    }

    private void OnBotCreated(Bot bot)
    {
        BotCreated?.Invoke(bot);
    }
}