using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemSpawner : MonoBehaviour
{
    public event Action<Item> ItemSpawned;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private int _initialItemsCount = 5;
    [SerializeField] private float _spawnRadius = 15f;
    [SerializeField] private float _spawnInterval = 3f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 _spawnAreaSize = new Vector3(20f, 0f, 20f);

    private List<Item> _spawnedItems = new List<Item>();
    private float _spawnTimer;

    private const float GroundYPosition = 0f;

    private void Start()
    {
        SpawnInitialItems();
    }

    private void Update()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            TrySpawnItem();
            _spawnTimer = 0f;
        }
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < _initialItemsCount; i++)
        {
            TrySpawnItem();
        }
    }

    private void TrySpawnItem()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        SpawnItemAtPosition(spawnPosition);
    }

    private void SpawnItemAtPosition(Vector3 position)
    {

        if (_itemPrefab == null)
            return;

        //Vector3? nullablePos = null;        // ✅ Работает
        //Vector3 nonNullablePos = null;      // ❌ Ошибка компиляции
        //Vector3 nonNullablePos = default;   // ✅ (0,0,0)
        Vector3 adjustPosition = new Vector3(0, 0.5f, 0);

        GameObject itemObject = Instantiate(_itemPrefab, position + adjustPosition, Quaternion.identity, transform);
       
        if (itemObject.TryGetComponent(out Item item))
        {
            _spawnedItems.Add(item);
            item.Collected += HandleItemCollected;
            ItemSpawned?.Invoke(item);
        }
    }

    private void HandleItemCollected(Item item)
    {
        item.Collected -= HandleItemCollected;
        _spawnedItems.Remove(item);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = transform.position + new Vector3(
            UnityEngine.Random.Range(-_spawnAreaSize.x / 2f, _spawnAreaSize.x / 2f),
            GroundYPosition,
            UnityEngine.Random.Range(-_spawnAreaSize.z / 2f, _spawnAreaSize.z / 2f));

        return randomPoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, _spawnAreaSize);

        Gizmos.color = Color.yellow;
        foreach (var item in _spawnedItems)
        {
            if (item != null)
            {
                Gizmos.DrawWireSphere(item.transform.position, 0.5f);
            }
        }
    }
}