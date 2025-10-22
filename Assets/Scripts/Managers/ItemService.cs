//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//public class ItemService : MonoBehaviour, IItemService
//{//после рефакторинга УДАЛИТЬ!
//    [Header("Item Settings")]
//    [SerializeField] private float _cacheUpdateInterval = 1f;

//    private HashSet<Item> _reservedItems = new HashSet<Item>();
//    private Item[] _cachedItems = new Item[0];
//    private float _lastCacheTime;
//    private bool _isInitialized = false;

//    public event Action<Item> ItemReserved;
//    public event Action<Item> ItemReleased;
//    public event Action<Item> ItemCollected;
//    public event Action<Item> ItemSpawned;

//    private void Start()
//    {
//        Initialize();
//    }

//    private void Update()
//    {
//        if (_isInitialized == false)
//            return;

//        if (Time.time - _lastCacheTime >= _cacheUpdateInterval)
//            UpdateItemCache();
//    }

//    private void Initialize()
//    {
//        UpdateItemCache();
//        ServiceLocator.Register<IItemService>(this);
//        _isInitialized = true;
//    }

//    private void UpdateItemCache()
//    {
//        _cachedItems = FindObjectsOfType<Item>();
//        _lastCacheTime = Time.time;
//    }

//    public Item[] GetAllItems() =>
//        _cachedItems;

//    public Item[] GetAvailableItems() =>
//        _cachedItems.Where(item =>
//            item != null &&
//            item.isActiveAndEnabled &&
//            item.CanBeCollected &&
//            _reservedItems.Contains(item) == false
//        ).ToArray();

//    public Item FindNearestAvailableItem(Vector3 position, float radius)
//    {
//        var availableItems = GetAvailableItems();
//        Item nearestItem = null;
//        float nearestDistance = float.MaxValue;

//        foreach (var item in availableItems)
//        {
//            float distance = Vector3.Distance(position, item.transform.position); // Vector3.Distance - ресурсозатратно => переделать

//            if (distance <= radius && distance < nearestDistance)
//            {
//                nearestDistance = distance;
//                nearestItem = item;
//            }
//        }

//        return nearestItem;
//    }

//    public Item FindBestItemForBot(Vector3 position, float radius, BotController bot)
//    {
//        var availableItems = GetAvailableItems();
//        Item bestItem = null;
//        float bestScore = float.MinValue;

//        foreach (var item in availableItems)
//        {
//            float distance = Vector3.Distance(position, item.transform.position); // Vector3.Distance - ресурсозатратно => переделать

//            if (distance > radius)
//                continue;

//            float score = CalculateItemScore(item, distance, bot);

//            if (score > bestScore)
//            {
//                bestScore = score;
//                bestItem = item;
//            }
//        }

//        return bestItem;
//    }

//    public bool TryReserveItem(Item item, BotController requester)
//    {
//        if (item == null || _reservedItems.Contains(item))
//            return false;

//        _reservedItems.Add(item);
//        ItemReserved?.Invoke(item);

//        return true;
//    }

//    public void ReleaseItem(Item item)
//    {
//        if (item != null && _reservedItems.Contains(item))
//        {
//            _reservedItems.Remove(item);
//            ItemReleased?.Invoke(item);
//        }
//    }

//    public bool IsItemReserved(Item item) =>
//        item != null && _reservedItems.Contains(item);

//    public void ReleaseAllReservations() =>
//        _reservedItems.Clear();

//    public int GetTotalItemsCount() =>
//        _cachedItems.Length;

//    public int GetAvailableItemsCount() =>
//        GetAvailableItems().Length;

//    public int GetReservedItemsCount() =>
//        _reservedItems.Count;

//    private float CalculateItemScore(Item item, float distance, BotController bot)
//    {
//        float score = 1f / (distance + 0.1f);

//        // Можно добавить дополнительные факторы:
//        // - Приоритет по типу предмета
//        // - Направление движения бота
//        // - "Свежесть" предмета и т.д.

//        return score;
//    }

//    private void OnDestroy()
//    {
//        ItemReserved = null;
//        ItemReleased = null;
//        ItemCollected = null;
//        ItemSpawned = null;
//    }
//}