using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ItemService : MonoBehaviour, IItemService
{
    [Header("Item Settings")]
    [SerializeField] private float _cacheUpdateInterval = 1f;

    private HashSet<Item> _reservedItems = new HashSet<Item>();
    private Item[] _cachedItems = new Item[0];
    private float _lastCacheTime;
    private bool _isInitialized = false;

    // События
    public event Action<Item> OnItemReserved;
    public event Action<Item> OnItemReleased;
    public event Action<Item> OnItemCollected;
    public event Action<Item> OnItemSpawned;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        UpdateItemCache();
        ServiceLocator.Register<IItemService>(this);
        _isInitialized = true;

        Debug.Log("ItemService initialized and registered");
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // Обновляем кэш с интервалом
        if (Time.time - _lastCacheTime >= _cacheUpdateInterval)
        {
            UpdateItemCache();
        }
    }

    private void UpdateItemCache()
    {
        _cachedItems = FindObjectsOfType<Item>();
        _lastCacheTime = Time.time;

        Debug.Log($"Item cache updated: {_cachedItems.Length} items total");
    }

    #region IItemService Implementation

    public Item[] GetAllItems() => _cachedItems;

    public Item[] GetAvailableItems() =>
        _cachedItems.Where(item =>
            item != null &&
            item.isActiveAndEnabled &&
            item.CanBeCollected &&
            !_reservedItems.Contains(item)
        ).ToArray();

    public Item FindNearestAvailableItem(Vector3 position, float radius)
    {
        var availableItems = GetAvailableItems();
        Item nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (var item in availableItems)
        {
            float distance = Vector3.Distance(position, item.transform.position);
            if (distance <= radius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    public Item FindBestItemForBot(Vector3 position, float radius, BotController bot)
    {
        var availableItems = GetAvailableItems();
        Item bestItem = null;
        float bestScore = float.MinValue;

        foreach (var item in availableItems)
        {
            float distance = Vector3.Distance(position, item.transform.position);
            if (distance > radius) continue;

            // Более сложная система оценки приоритета
            float score = CalculateItemScore(item, distance, bot);
            if (score > bestScore)
            {
                bestScore = score;
                bestItem = item;
            }
        }

        return bestItem;
    }

    public bool TryReserveItem(Item item, BotController requester)
    {
        if (item == null || _reservedItems.Contains(item))
            return false;

        _reservedItems.Add(item);
        OnItemReserved?.Invoke(item);

        Debug.Log($"Item reserved: {item.ItemName} by {requester.gameObject.name}");
        return true;
    }

    public void ReleaseItem(Item item)
    {
        if (item != null && _reservedItems.Contains(item))
        {
            _reservedItems.Remove(item);
            OnItemReleased?.Invoke(item);

            Debug.Log($"Item released: {item.ItemName}");
        }
    }

    public bool IsItemReserved(Item item) => item != null && _reservedItems.Contains(item);

    public void ReleaseAllReservations()
    {
        _reservedItems.Clear();
        Debug.Log("All item reservations released");
    }

    public int GetTotalItemsCount() => _cachedItems.Length;

    public int GetAvailableItemsCount() => GetAvailableItems().Length;

    public int GetReservedItemsCount() => _reservedItems.Count;

    #endregion

    #region Utility Methods

    private float CalculateItemScore(Item item, float distance, BotController bot)
    {
        // Базовая оценка на основе расстояния
        float score = 1f / (distance + 0.1f);

        // Можно добавить дополнительные факторы:
        // - Приоритет по типу предмета
        // - Направление движения бота
        // - "Свежесть" предмета и т.д.

        return score;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        OnItemReserved = null;
        OnItemReleased = null;
        OnItemCollected = null;
        OnItemSpawned = null;
    }

    #endregion
}