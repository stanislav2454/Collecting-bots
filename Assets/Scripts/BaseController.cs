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
    private float _debugTimer;

    public event System.Action<int> ResourceCollected;
    public event System.Action<Bot> BotCreated;

    public Vector3 BasePosition => transform.position;// если не нужен - удалить !
    public float UnloadZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.UnloadZoneRadius : 1.5f;
    public float SpawnZoneRadius => _zoneVisualizer != null ? _zoneVisualizer.SpawnZoneRadius : 3f;

    private void Start()
    {
        InitializeDependencies();
    }

    private void Update()
    {
        {// temporarily (временно)
            _debugTimer += Time.deltaTime;
            if (_debugTimer >= 5f)
            {
                // DebugLogBaseState();
                _debugTimer = 0f;
            }
        }
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
            _zoneVisualizer = GetComponentInChildren<BaseZoneVisualizer>();// дочернийний объект

        if (TryGetComponent(out _botManager) == false)
        {
            _botManager = GetComponentInChildren<BotManager>();// дочернийний объект
            _botManager.BotCreated += OnBotCreated;
        }
        else
            Debug.LogError("BotManager not found in BaseController!");

        if (_itemSpawner == null)// внешний объект
            Debug.LogError("ItemSpawner not assigned in BotManager!");

        if (_resourceManager == null)// внешний объект
            Debug.LogError("ResourceManager not assigned in BotManager!");
    }

    private void OnBotCreated(Bot bot)
    {
        BotCreated?.Invoke(bot);
    }

    private void DebugLogBaseState()
    { // ✅ СОКРАЩАЕМ: Только самая важная информация
        Debug.Log($"Resources: {_collectedResources} | " +
            $"Bots: {_botManager?.AvailableBotsCount}/{_botManager?.BotCount}");
        //if (_botManager != null)
        //{
        //    //Debug.Log($"=== BASE CONTROLLER STATE ===");
        //    //Debug.Log($"Collected Resources: {_collectedResources}");
        //    //Debug.Log($"Unload Zone Radius: {UnloadZoneRadius}");
        //    //Debug.Log($"Spawn Zone Radius: {SpawnZoneRadius}");
        //    //Debug.Log($"=============================");

        //    //_botManager.DebugLogBotState();
        //}
    }
}