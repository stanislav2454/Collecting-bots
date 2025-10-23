using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class ItemSpawner : ZoneVisualizer
{
    [Header("Spawn Settings")]
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private int _initialItemsCount = 5;
    [SerializeField] private float _spawnInterval = 3f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(20f, 0.1f, 20f);

    [Header("Dependencies")]
    [SerializeField] private ItemPool _itemPool;

    private int _maxSize = 50;
    private List<Item> _spawnedItems = new List<Item>();
    private Coroutine _spawnCoroutine;

    public event Action<Item> ItemSpawned;

    private void Start()
    {
        InitializeItemPool();
        SpawnInitialItems();
        UpdateZoneVisualization();
    }

    private void OnDestroy()
    {
        StopAutoSpawning();
    }

    public void ReturnItemToPool(Item item)
    {
        if (item != null && _itemPool != null)
            _itemPool.ReturnItem(item);
    }

    private void InitializeItemPool()
    {
        _itemPool.Initialize(_itemPrefab, _initialItemsCount, _maxSize, transform);
        _itemPool.ItemReturned += HandleItemReturnedToPool;
    }

    private void HandleItemReturnedToPool(Item item)
    {
        item.PrepareForRespawn();
        _spawnedItems.Remove(item);
    }

    private void StartAutoSpawning()
    {
        StopAutoSpawning();
        _spawnCoroutine = StartCoroutine(AutoSpawnCoroutine());
    }

    private void StopAutoSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private IEnumerator AutoSpawnCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_spawnInterval);

        while (true)
        {
            yield return waitForSeconds;

            if (_spawnedItems.Count < _maxSize)
                TrySpawnItem();
        }
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < _initialItemsCount; i++)
            TrySpawnItem();

        StartAutoSpawning();
    }

    private void TrySpawnItem()
    {
        if (_spawnedItems.Count >= _maxSize)
            return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        SpawnItemAtPosition(spawnPosition);
    }

    private void SpawnItemAtPosition(Vector3 position)
    {
        Item item = _itemPool.GetItem(position);

        if (item != null)
        {
            if (_spawnedItems.Contains(item) == false)
            {
                item.transform.rotation = Quaternion.identity;
                _spawnedItems.Add(item);

                item.Collected += HandleItemCollected;
                ItemSpawned?.Invoke(item);
            }
        }
    }

    private void HandleItemCollected(Item item)
    {
        if (item != null)
        {
            item.Collected -= HandleItemCollected;

            _itemPool?.ReturnItem(item);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = transform.position + new Vector3(
            UnityEngine.Random.Range(-_spawnAreaSize.x / BotConstants.Divider, _spawnAreaSize.x / BotConstants.Divider),
            BotConstants.GroundYPosition,
            UnityEngine.Random.Range(-_spawnAreaSize.z / BotConstants.Divider, _spawnAreaSize.z / BotConstants.Divider));

        return randomPoint + Vector3.up * BotConstants.ItemYOffset;
    }

    private void UpdateZoneVisualization() =>
        CreateOrUpdateZone(_spawnAreaSize, Vector3.zero);
}