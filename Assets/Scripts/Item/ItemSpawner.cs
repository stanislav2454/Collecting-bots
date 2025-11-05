using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class ItemSpawner : ZoneVisualizer
{
    [Header("Spawn Settings")]
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private Transform _itemContainer;
    [SerializeField] [Range(1, 10)] private int _initialItemsCount = 3;
    [SerializeField] [Range(0.2f, 60)] private float _respawnDelay = 5f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(20f, 0.1f, 20f);

    [Header("Dependencies")]
    [SerializeField] private ItemPool _itemPool;
    [SerializeField] private ResourceManager _resourceManager;

    private int _maxSize = 50;
    private List<Item> _activeItems = new List<Item>();
    private Dictionary<Item, Coroutine> _activeRespawnCoroutines = new Dictionary<Item, Coroutine>();

    public event Action<Item> ItemSpawned;

    public int MaxActiveItems { get; private set; } = 15;
    public int SpawnedItemsCount => _activeItems.Count;

    private void Start()
    {
        InitializeItemPool();
        SpawnInitialItems();
        UpdateZoneVisualization();
    }

    private void OnDestroy()
    {
        foreach (var coroutinePair in _activeRespawnCoroutines)
            if (coroutinePair.Value != null)
                StopCoroutine(coroutinePair.Value);

        _activeRespawnCoroutines.Clear();
    }

    public void ReturnItemToPool(Item item)
    {
        if (item == null)
            return;

        if (_activeRespawnCoroutines.ContainsKey(item))
        {
            StopCoroutine(_activeRespawnCoroutines[item]);
            _activeRespawnCoroutines.Remove(item);
        }

        item.gameObject.SetActive(false);
        _activeItems.Remove(item);

        var coroutine = StartCoroutine(RespawnItemAfterDelay(item, _respawnDelay));
        _activeRespawnCoroutines[item] = coroutine;
    }

    private IEnumerator RespawnItemAfterDelay(Item item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_activeRespawnCoroutines.ContainsKey(item))
            _activeRespawnCoroutines.Remove(item);

        if (item != null && _itemPool != null && _activeItems.Count < MaxActiveItems)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            item.PrepareForRespawn();

            if (_itemContainer != null)
                item.transform.SetParent(_itemContainer);

            item.transform.position = spawnPosition;
            item.transform.rotation = Quaternion.identity;

            _activeItems.Add(item);
            _resourceManager?.RegisterResource(item);
        }
        else if (item != null && _itemPool != null)
        {
            _itemPool.ReturnItem(item);
        }
    }

    private void InitializeItemPool()
    {
        _itemPool.Initialize(_itemPrefab, _initialItemsCount, _maxSize, _itemContainer);
        _itemPool.ItemReturned += HandleItemReturnedToPool;
    }

    private void HandleItemReturnedToPool(Item item)
    {
        if (item.gameObject.activeInHierarchy == false)
            item.gameObject.SetActive(true);

        _resourceManager?.RegisterResource(item);
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < _initialItemsCount; i++)
            TrySpawnItem();
    }

    private void TrySpawnItem()
    {
        if (_activeItems.Count >= MaxActiveItems)
            return;

        SpawnItemAtPosition(GetRandomSpawnPosition());
    }

    private void SpawnItemAtPosition(Vector3 position)
    {
        Item item = _itemPool.GetItem(position);

        if (item != null && _activeItems.Contains(item) == false)
        {
            if (_itemContainer != null && item.transform.parent != _itemContainer)
                item.transform.SetParent(_itemContainer);

            item.transform.rotation = Quaternion.identity;
            item.PrepareForRespawn();
            _activeItems.Add(item);
            _resourceManager?.RegisterResource(item);
            ItemSpawned?.Invoke(item);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = transform.position + new Vector3(
            UnityEngine.Random.Range(-_spawnAreaSize.x / BotConstants.Divider, _spawnAreaSize.x / BotConstants.Divider),
            BotConstants.ItemYOffset,
            UnityEngine.Random.Range(-_spawnAreaSize.z / BotConstants.Divider, _spawnAreaSize.z / BotConstants.Divider));

        return randomPoint;
    }

    private void UpdateZoneVisualization() =>
        CreateOrUpdateZone(_spawnAreaSize, Vector3.zero);
}