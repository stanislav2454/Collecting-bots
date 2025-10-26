using UnityEngine;
using System.Collections.Generic;

public class ItemPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private int _maxPoolSize = 30;
    [SerializeField] private Transform _poolContainer;

    private Queue<Item> _availableItems = new Queue<Item>();
    private List<Item> _activeItems = new List<Item>();
    private int _createdItemsCount = 0;

    public event System.Action<Item> ItemCreated;
    public event System.Action<Item> ItemReturned;

    private void OnDestroy()
    {
        _availableItems.Clear();
        _activeItems.Clear();
    }

    public void Initialize(Item itemPrefab, int initialSize, int maxSize, Transform container = null)
    {
        _itemPrefab = itemPrefab;
        _initialPoolSize = initialSize;
        _maxPoolSize = maxSize;
        _poolContainer = container ?? CreatePoolContainer();

        InitializePool();
    }

    public Item GetItem(Vector3 position)
    {
        Item item = null;

        if (_availableItems.Count > 0)
            item = _availableItems.Dequeue();
        else if (_createdItemsCount < _maxPoolSize)
            item = CreateNewItem();


        if (item != null)
        {
            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;
            item.gameObject.SetActive(true);
            _activeItems.Add(item);
        }

        return item;
    }

    public void ReturnItem(Item item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(false);

        if (_poolContainer != null)
        {
            item.transform.position = _poolContainer.position;
            item.transform.SetParent(_poolContainer);
        }

        _activeItems.Remove(item);

        if (_availableItems.Contains(item) == false)
            _availableItems.Enqueue(item);

        ItemReturned?.Invoke(item);
    }

    private void InitializePool()
    {
        if (_itemPrefab == null)
            return;

        for (int i = 0; i < _initialPoolSize; i++)
            CreateNewItem();
    }

    private Item CreateNewItem()
    {
        if (_createdItemsCount >= _maxPoolSize)
            return null;

        if (_itemPrefab == null)
            return null;

        Item item = Instantiate(_itemPrefab, _poolContainer);

        if (item != null)
        {
            item.name = $"Item_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            item.gameObject.SetActive(false);
            _availableItems.Enqueue(item);
            _createdItemsCount++;
            ItemCreated?.Invoke(item);

            return item;
        }

        return null;
    }

    private Transform CreatePoolContainer()
    {
        GameObject container = new GameObject("ItemPool_Container");
        container.transform.SetParent(transform);
        return container.transform;
    }
}