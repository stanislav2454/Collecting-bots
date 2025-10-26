using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class ItemSpawner : ZoneVisualizer
{
    [Header("Spawn Settings")]
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private int _initialItemsCount = 5;
    [SerializeField] [Range(0.1f, 60)] private float _respawnDelay = 5f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(20f, 0.1f, 20f);

    [Header("Dependencies")]
    [SerializeField] private ItemPool _itemPool;
    [SerializeField] private ResourceManager _resourceManager;

    private int _maxActiveItems = 15;
    private int _maxSize = 50;
    private List<Item> _activeItems = new List<Item>();
    private Coroutine _respawnItemAfterDelayCoroutine;

    public event Action<Item> ItemSpawned;

    private void Start()
    {
        InitializeItemPool();
        SpawnInitialItems();
        UpdateZoneVisualization();
    }

    private void OnDestroy()
    {
        if (_respawnItemAfterDelayCoroutine != null)
            StopCoroutine(_respawnItemAfterDelayCoroutine);
    }

    public void ReturnItemToPool(Item item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(false);
        _activeItems.Remove(item);
        _respawnItemAfterDelayCoroutine = StartCoroutine(RespawnItemAfterDelay(item, _respawnDelay));
    }

    public int GetSpawnedItemsCount() =>
        _activeItems.Count;

    public int GetMaxActiveItems() =>
        _maxActiveItems;

    private IEnumerator RespawnItemAfterDelay(Item item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item != null && _itemPool != null && _activeItems.Count < _maxActiveItems)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            item.transform.position = spawnPosition;
            item.transform.rotation = Quaternion.identity;
            item.PrepareForRespawn();

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
        _itemPool.Initialize(_itemPrefab, _initialItemsCount, _maxSize, transform);
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
        if (_activeItems.Count >= _maxActiveItems)
            return;

        SpawnItemAtPosition(GetRandomSpawnPosition());
    }

    private void SpawnItemAtPosition(Vector3 position)
    {
        Item item = _itemPool.GetItem(position);

        if (item != null && _activeItems.Contains(item) == false)
        {
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