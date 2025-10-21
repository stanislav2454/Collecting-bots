using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] public GameObject itemPrefab;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !
    [SerializeField] public List<ItemData> availableItems;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !

    [Header("Spawn Settings")]
    [SerializeField] public int initialItemsCount = 10;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !
    [SerializeField] public Vector3 spawnArea = new Vector3(10f, 0f, 10f);//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !
    [SerializeField] public LayerMask spawnLayerMask = 1;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькойи )и инкапсуляции !
    //.layer = LayerMask.NameToLayer("Items");// ОБРАЗЕЦ 
    [Header("Respawn Overrides")]
    [SerializeField] private bool _overrideRespawnTimes = false;
    [SerializeField] private float _globalRespawnTime = 10f;

    private List<Item> _spawnedItems = new List<Item>();
    private List<Transform> _spawnPoints = new List<Transform>();

    public static ItemManager Instance { get; private set; }

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
        {
            ItemData itemData = GetItemDataForSpawn(spawnPosition);

            if (itemData != null)
            {
                item.ItemData = itemData;
                itemObj.name = $"Item_{itemData.itemName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                _spawnedItems.Add(item);
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

    public Item GetNearestItem(Vector3 position)// зачем нужен ? если не используется - удалить !
    {
        List<Item> availableItems = GetAvailableItems();
        Item nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (var item in availableItems)
        {
            float distance = Vector3.Distance(position, item.transform.position);// Vector3.Distance - ресурсозатратно => переделать

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    private void FindSpawnPoints()
    {
        ItemSpawnPoint[] points = FindObjectsOfType<ItemSpawnPoint>();// todo//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        foreach (var point in points)
            _spawnPoints.Add(point.transform);
    }

    private void SpawnInitialItems()
    {
        for (int i = 0; i < initialItemsCount; i++)
            SpawnItem();
    }

    private ItemData GetItemDataForSpawn(Vector3 spawnPosition)
    {
        ItemSpawnPoint spawnPoint = FindSpawnPointAtPosition(spawnPosition);

        if (spawnPoint != null)
        {
            ItemData preferredItem = spawnPoint.GetPreferredItemData();

            if (preferredItem != null)
                return preferredItem;

            List<ItemData> matchingItems = new List<ItemData>();

            foreach (var itemData in availableItems)
                if (spawnPoint.CanSpawnItem(itemData))
                    matchingItems.Add(itemData);

            if (matchingItems.Count > 0)
                return matchingItems[Random.Range(0, matchingItems.Count)];
        }

        return availableItems[Random.Range(0, availableItems.Count)];
    }

    private ItemSpawnPoint FindSpawnPointAtPosition(Vector3 position)
    {
        foreach (var spawnPointTransform in _spawnPoints)
        {
            ItemSpawnPoint spawnPoint = spawnPointTransform.GetComponent<ItemSpawnPoint>();

            if (spawnPoint != null && Vector3.Distance(spawnPoint.Position, position) < 0.1f)// Vector3.Distance - ресурсозатратно => переделать
                return spawnPoint;//todo Vector3.Distance()
        }

        return null;
    }

    private Vector3 GetSpawnPosition()
    {
        if (_spawnPoints.Count > 0)
        {
            Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            return randomSpawnPoint.position;
        }

        Vector3 randomPoint = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.z, spawnArea.z));

        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 10f, spawnLayerMask))
            return hit.position;

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 0.1f, spawnArea.z * 2));
    }
}