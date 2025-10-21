using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotManager : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private int _maxSize = 50;
    [SerializeField] private Transform _botSpawnContainer;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 _spawnArea = new Vector3(10, 0, 10);
    [SerializeField] private bool _autoSpawnInitialBots = true;

    private BotPool _botPool;
    private List<BotController> _allBots = new List<BotController>();
    private HashSet<Item> _reservedItems = new HashSet<Item>();

    private void Start()
    {
        InitializeBotPool();

        if (_autoSpawnInitialBots)
            SpawnInitialBots();
    }

    public GameObject SpawnBot()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject newBot = _botPool.GetBot(spawnPosition);

        if (_botPrefab == null)
        {
            Debug.LogError("Bot prefab not assigned in BotManager!");
            return null;
        }

        if (newBot != null)
        {
            BotController botController = newBot.GetComponent<BotController>();
            if (botController != null)
            {
                _allBots.Add(botController);
                Debug.Log($"Spawned bot: {newBot.name} at position {spawnPosition}");
            }
        }

        return newBot;

        //Vector3 spawnPosition = GetRandomSpawnPosition();
        //GameObject bot = Instantiate(_botPrefab, spawnPosition, Quaternion.identity, transform);
        //bot.name = $"Bot_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        //// Гарантируем что у бота есть коллайдер
        //if (bot.GetComponent<Collider>() == null)
        //{
        //    CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
        //    collider.height = 2f;
        //    collider.radius = 0.5f;
        //    collider.center = new Vector3(0, 1f, 0);
        //}
        ////.layer = LayerMask.NameToLayer("Items");
        //bot.layer = 6; // Bot layer// Устанавливаем слой бота

        //Debug.Log($"Spawned bot: {bot.name} at position {spawnPosition}");
        //return bot;
    }

    public void DespawnBot(GameObject bot)
    {
        if (bot != null)
        {
            BotController botController = bot.GetComponent<BotController>();
            if (botController != null)
                _allBots.Remove(botController);

            _botPool.ReturnBot(bot);
            Debug.Log($"Despawned bot: {bot.name}");
        }
    }

    // Координация между ботами - резервирование предметов
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
            _reservedItems.Remove(item);
    }

    public bool IsItemReserved(Item item) =>
         item != null && _reservedItems.Contains(item);

    // Получение непересекающихся целей для ботов
    public Item FindAvailableItemForBot(BotController bot, Vector3 botPosition, float searchRadius)
    {
        Item[] allItems = FindObjectsOfType<Item>();
        Item bestItem = null;
        float bestScore = float.MinValue;

        foreach (var item in allItems)
        {
            if (item == null || item.CanBeCollected == false || IsItemReserved(item))
                continue;
            //              todo => Vector3.Distance
            float distance = Vector3.Distance(botPosition, item.transform.position);

            if (distance > searchRadius)
                continue;

            // Оценка приоритета предмета
            float score = CalculateItemScore(item, distance, bot);

            if (score > bestScore)
            {
                bestScore = score;
                bestItem = item;
            }
        }

        // Резервируем найденный предмет
        if (bestItem != null && TryReserveItem(bestItem, bot))
            return bestItem;

        return null;
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
        Debug.Log("All bots reset");
    }

    public int GetActiveBotsCount() =>
        _allBots.Count;

    public BotController[] GetAllBots() =>
        _allBots.ToArray();

    private void InitializeBotPool()
    { // Создаем систему пула
        _botPool = gameObject.AddComponent<BotPool>();
        _botPool.Initialize(_botPrefab, _initialBotsCount, _maxSize, _botSpawnContainer);
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            SpawnBot();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = transform.position;
        Vector3 randomPoint = center + new Vector3(
            Random.Range(-_spawnArea.x / 2, _spawnArea.x / 2),
            0,
            Random.Range(-_spawnArea.z / 2, _spawnArea.z / 2));

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            return hit.position;

        return center;
    }

    private float CalculateItemScore(Item item, float distance, BotController bot)
    {
        float score = 1f / (distance + 0.1f); // Близость

        // Можно добавить приоритет по типу предмета
        // if (item.ItemType == ItemType.Rare) score *= 2f;

        return score;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, _spawnArea);
        // Gizmos.DrawWireCube(transform.position, new Vector3(_spawnArea.x * 2, 0.1f, _spawnArea.z * 2));
    }
}