using UnityEngine;
using System;
using System.Collections.Generic;

public class BotService : MonoBehaviour, IBotService
{
    [Header("Bot Settings")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private Transform _botSpawnContainer;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _spawnArea = new Vector3(10, 0, 10);
    [SerializeField] private bool _autoSpawnInitialBots = true;

    private BotPool _botPool;
    private List<BotController> _allBots = new List<BotController>();
    private BotController _selectedBot;
    private HashSet<Item> _reservedItems = new HashSet<Item>();

    // Реализация событий интерфейса
    public event Action<BotController> OnBotSelected;
    public event Action<BotController> OnBotDeselected;
    public event Action<BotController> OnBotSpawned;
    public event Action<BotController> OnBotDespawned;
    public event Action OnAllBotsReset;

    private void Start()
    {
        InitializeBotPool();

        // Регистрируем сервис в ServiceLocator
        ServiceLocator.Register<IBotService>(this);

        if (_autoSpawnInitialBots)
        {
            SpawnInitialBots();
        }

        Debug.Log("BotService initialized and registered");
    }

    private void InitializeBotPool()
    {
        _botPool = gameObject.AddComponent<BotPool>();
        _botPool.Initialize(_botPrefab, _initialBotsCount, 50, _botSpawnContainer);
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
        {
            SpawnBot(GetRandomSpawnPosition());
        }
    }

    #region IBotService Implementation

    public BotController[] GetAllBots() => _allBots.ToArray();

    public BotController GetSelectedBot() => _selectedBot;

    public void SelectBot(BotController bot)
    {
        if (bot == null) return;

        DeselectAllBots();
        _selectedBot = bot;
        OnBotSelected?.Invoke(bot);

        Debug.Log($"Bot selected: {bot.gameObject.name}");
    }

    public void DeselectAllBots()
    {
        if (_selectedBot != null)
        {
            var previousBot = _selectedBot;
            _selectedBot = null;
            OnBotDeselected?.Invoke(previousBot);

            Debug.Log($"Bot deselected: {previousBot.gameObject.name}");
        }
    }

    public GameObject SpawnBot(Vector3 position)
    {
        GameObject newBot = _botPool.GetBot(position);

        if (newBot != null)
        {
            BotController botController = newBot.GetComponent<BotController>();
            if (botController != null)
            {
                _allBots.Add(botController);
                OnBotSpawned?.Invoke(botController);

                Debug.Log($"Bot spawned: {newBot.name} at {position}");
            }
        }

        return newBot;
    }

    public void DespawnBot(GameObject bot)
    {
        if (bot != null)
        {
            BotController botController = bot.GetComponent<BotController>();
            if (botController != null)
            {
                _allBots.Remove(botController);
                OnBotDespawned?.Invoke(botController);
            }

            _botPool.ReturnBot(bot);
            Debug.Log($"Bot despawned: {bot.name}");
        }
    }

    public void ResetAllBots()
    {
        foreach (var bot in _allBots)
        {
            bot.SetAIEnabled(true);
            bot.StopMovement();
            bot.ClearTargetItem();
        }

        _reservedItems.Clear();
        OnAllBotsReset?.Invoke();

        Debug.Log("All bots reset to default state");
    }

    public int GetActiveBotsCount() => _allBots.Count;

    public int GetTotalBotsCount() => _botPool?.GetTotalBotsCount() ?? 0;

    public string GetBotPoolInfo() => _botPool?.GetPoolInfo() ?? "BotPool not initialized";

    #endregion

    #region Item Reservation System (для обратной совместимости)

    public bool TryReserveItem(Item item, BotController requester)
    {
        if (item == null || _reservedItems.Contains(item))
            return false;

        _reservedItems.Add(item);
        Debug.Log($"Item {item.ItemName} reserved by {requester.gameObject.name}");
        return true;
    }

    public void ReleaseItem(Item item)
    {
        if (item != null)
        {
            _reservedItems.Remove(item);
        }
    }

    public bool IsItemReserved(Item item) => item != null && _reservedItems.Contains(item);

    public Item FindAvailableItemForBot(BotController bot, Vector3 botPosition, float searchRadius)
    {
        // Используем ItemService если доступен
        if (ServiceLocator.TryGet<IItemService>(out var itemService))
        {
            return itemService.FindBestItemForBot(botPosition, searchRadius, bot);
        }

        // Fallback на старую систему
        return FindAvailableItemFallback(botPosition, searchRadius);
    }

    private Item FindAvailableItemFallback(Vector3 botPosition, float searchRadius)
    {
        Item[] allItems = FindObjectsOfType<Item>();
        Item bestItem = null;
        float bestScore = float.MinValue;

        foreach (var item in allItems)
        {
            if (item == null || !item.CanBeCollected || IsItemReserved(item))
                continue;

            float distance = Vector3.Distance(botPosition, item.transform.position);
            if (distance > searchRadius)
                continue;

            float score = 1f / (distance + 0.1f);
            if (score > bestScore)
            {
                bestScore = score;
                bestItem = item;
            }
        }

        return bestItem;
    }

    #endregion

    #region Utility Methods

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = transform.position;
        Vector3 randomPoint = center + new Vector3(
            UnityEngine.Random.Range(-_spawnArea.x / 2, _spawnArea.x / 2),
            0,
            UnityEngine.Random.Range(-_spawnArea.z / 2, _spawnArea.z / 2));

        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Отменяем подписки на события
        OnBotSelected = null;
        OnBotDeselected = null;
        OnBotSpawned = null;
        OnBotDespawned = null;
        OnAllBotsReset = null;
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        // Визуализация зоны спавна
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, _spawnArea);

        // Визуализация зарезервированных предметов
        Gizmos.color = Color.red;
        foreach (var item in _reservedItems)
        {
            if (item != null)
            {
                Gizmos.DrawWireSphere(item.transform.position, 0.5f);
            }
        }
    }

    #endregion
}