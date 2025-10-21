using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Settings")]
    public GameObject itemPrefab;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public List<ItemData> availableItems;// модификаторДоступа+именаСчертойИлиБольшойБуквы

    [Header("Spawn Settings")]
    public int initialItemsCount = 10;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public Vector3 spawnArea = new Vector3(10f, 0f, 10f);// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public LayerMask spawnLayerMask = 1;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    //.layer = LayerMask.NameToLayer("Items");
    [Header("Respawn Overrides")]
    [SerializeField] private bool _overrideRespawnTimes = false;
    [SerializeField] private float _globalRespawnTime = 10f;

    private List<Item> _spawnedItems = new List<Item>();
    private List<Transform> _spawnPoints = new List<Transform>();

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
        { // НОВАЯ ЛОГИКА: Выбираем предмет с учетом точки спавна
            ItemData itemData = GetItemDataForSpawn(spawnPosition);
            if (itemData != null)
            {
                item.ItemData = itemData;
                itemObj.name = $"Item_{itemData.itemName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                _spawnedItems.Add(item);

                Debug.Log($"Spawned item: {itemObj.name} at {spawnPosition}");
            }
            else
            {
                Destroy(itemObj);
                return null;
            }
        }

        return item;
    }

    public float GetRespawnTime(ItemData itemData)
    {
        if (_overrideRespawnTimes)
            return _globalRespawnTime;
        else
            return itemData.respawnTime;
    }

    public List<Item> GetAvailableItems()
    {
        List<Item> available = new List<Item>();

        foreach (var item in _spawnedItems)
            if (item != null && item.CanBeCollected)
                available.Add(item);
        
        return available;
    }

    public Item GetNearestItem(Vector3 position)
    {
        List<Item> availableItems = GetAvailableItems();
        Item nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (var item in availableItems)
        {// remake Vector3.Distance !
            float distance = Vector3.Distance(position, item.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    private void FindSpawnPoints()
    { // Ищем все точки спавна на сцене
        ItemSpawnPoint[] points = FindObjectsOfType<ItemSpawnPoint>();// todo
        foreach (var point in points)
            _spawnPoints.Add(point.transform);

        Debug.Log($"Found {_spawnPoints.Count} spawn points");
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < initialItemsCount; i++)
            SpawnItem();
    }

    private ItemData GetItemDataForSpawn(Vector3 spawnPosition)
    {
        // Проверяем есть ли точка спавна в этой позиции
        ItemSpawnPoint spawnPoint = FindSpawnPointAtPosition(spawnPosition);

        if (spawnPoint != null)
        {
            // Если точка спавна требует конкретный ItemData
            ItemData preferredItem = spawnPoint.GetPreferredItemData();
            if (preferredItem != null)
                return preferredItem;

            // Ищем предмет соответствующий требованиям точки спавна
            List<ItemData> matchingItems = new List<ItemData>();

            foreach (var itemData in availableItems)
                if (spawnPoint.CanSpawnItem(itemData))
                    //return itemData;
                    matchingItems.Add(itemData);

            if (matchingItems.Count > 0)
                return matchingItems[Random.Range(0, matchingItems.Count)];

            Debug.LogWarning($"No matching items found for spawn point at {spawnPosition}");
        }

        // Старая логика - случайный предмет
        return availableItems[Random.Range(0, availableItems.Count)];
    }

    private ItemSpawnPoint FindSpawnPointAtPosition(Vector3 position)
    {
        foreach (var spawnPointTransform in _spawnPoints)
        {
            ItemSpawnPoint spawnPoint = spawnPointTransform.GetComponent<ItemSpawnPoint>();
            if (spawnPoint != null && Vector3.Distance(spawnPoint.Position, position) < 0.1f)
                return spawnPoint;//todo Vector3.Distance()
        }
        return null;
    }

    private Vector3 GetSpawnPosition()
    { // Пытаемся использовать точки спавна
        if (_spawnPoints.Count > 0)
        {
            Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
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

    // Для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 0.1f, spawnArea.z * 2));
    }
}