using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Settings")]
    public GameObject itemPrefab;
    public List<ItemData> availableItems;

    [Header("Spawn Settings")]
    public int initialItemsCount = 10;
    public Vector3 spawnArea = new Vector3(10f, 0f, 10f);
    public LayerMask spawnLayerMask = 1;

    private List<Item> spawnedItems = new List<Item>();
    private List<Transform> spawnPoints = new List<Transform>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        FindSpawnPoints();
        SpawnInitialItems();
    }

    private void FindSpawnPoints()
    { // Ищем все точки спавна на сцене
        ItemSpawnPoint[] points = FindObjectsOfType<ItemSpawnPoint>();
        foreach (var point in points)
            spawnPoints.Add(point.transform);

        Debug.Log($"Found {spawnPoints.Count} spawn points");
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < initialItemsCount; i++)
            SpawnItem();
    }

    public Item SpawnItem()
    {
        if (itemPrefab == null || availableItems == null || availableItems.Count == 0)
        {
            Debug.LogError("Item prefab or available items not configured!");
            return null;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector3.zero)
            return null;

        GameObject itemObj = Instantiate(itemPrefab, spawnPosition, Quaternion.identity, transform);
        Item item = itemObj.GetComponent<Item>();

        if (item != null)
        { // Выбираем случайные данные предмета
            ItemData randomData = availableItems[Random.Range(0, availableItems.Count)];
            item.itemData = randomData;

            itemObj.name = $"Item_{randomData.itemName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            spawnedItems.Add(item);

            Debug.Log($"Spawned item: {itemObj.name} at {spawnPosition}");
        }

        return item;
    }

    private Vector3 GetSpawnPosition()
    { // Пытаемся использовать точки спавна
        if (spawnPoints.Count > 0)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            return randomSpawnPoint.position;
        }

        // Или генерируем случайную позицию
        Vector3 randomPoint = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.z, spawnArea.z));

        // Проверяем валидность позиции через NavMesh
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 10f, spawnLayerMask))
            return hit.position;

        return Vector3.zero;
    }

    public List<Item> GetAvailableItems()
    {
        List<Item> available = new List<Item>();
        foreach (var item in spawnedItems)
        {
            if (item != null && item.CanBeCollected)
                available.Add(item);
        }
        return available;
    }

    public Item GetNearestItem(Vector3 position)
    {
        List<Item> availableItems = GetAvailableItems();
        Item nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (var item in availableItems)
        {
            float distance = Vector3.Distance(position, item.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    // Для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 0.1f, spawnArea.z * 2));
    }
}