#if UNITY_EDITOR
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceDebugUI : MonoBehaviour
{
    [SerializeField] private ResourceManager _resourceManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _debugText;

    [Header("Update Settings")]
    [SerializeField] private float _updateInterval = 1f;

    private float _timer;
    private ItemSpawner _itemSpawner;
    private ItemPool _itemPool;

    private void Start()
    {
        FindDependencies();

        if (_debugText == null)
        {
            Debug.LogError("Debug Text not assigned in ResourceDebugUI!");
            return;
        }

        UpdateDisplay();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _updateInterval)
        {
            UpdateDisplay();
            _timer = 0f;
        }
    }

    public void LogCurrentState()
    {
        Debug.Log("=== ResourceDebugUI State ===");
        Debug.Log($"Ресурсов на сцене: {CountActiveResourcesOnScene()}");

        if (_resourceManager != null)
            Debug.Log($"ResourceManager - Свободные: {_resourceManager.FreeResourcesCount}, Занятые: {_resourceManager.ReservedResourcesCount}");

        if (_itemSpawner != null)
            Debug.Log($"Спавнер - Активные: {_itemSpawner.SpawnedItemsCount}/{_itemSpawner.MaxActiveItems}");
    }

    private void FindDependencies()
    {
        _itemSpawner = FindObjectOfType<ItemSpawner>();
        _itemPool = FindObjectOfType<ItemPool>();

        if (_resourceManager == null)
            Debug.LogWarning("ResourceManager not found!");

        if (_itemSpawner == null)
            Debug.LogWarning("ItemSpawner not found!");

        if (_itemPool == null)
            Debug.LogWarning("ItemPool not found!");
    }

    private void UpdateDisplay()
    {
        if (_debugText == null)
            return;

        string debugInfo = "=== СИСТЕМА РЕСУРСОВ ===\n\n";

        int activeOnScene = CountActiveResourcesOnScene();
        debugInfo += $"На сцене (активные): {activeOnScene}\n";

        if (_resourceManager != null)
        {
            debugInfo += $"ResourceManager:\n";
            debugInfo += $"  Свободные: {_resourceManager.FreeResourcesCount}\n";
            debugInfo += $"  Занятые: {_resourceManager.ReservedResourcesCount}\n";
            debugInfo += $"  Всего управляется: {_resourceManager.TotalResourcesCount}\n";
        }
        else
            debugInfo += "ResourceManager: НЕ НАЙДЕН\n";

        if (_itemSpawner != null)
            debugInfo += $"Спавнер: {_itemSpawner.SpawnedItemsCount}/{_itemSpawner.MaxActiveItems}\n";
        else
            debugInfo += "Спавнер: НЕ НАЙДЕН\n";

        if (_itemPool != null)
            debugInfo += $"В пуле: {CountItemsInPool()}\n";
        else
            debugInfo += "Пул: НЕ НАЙДЕН\n";

        debugInfo += "\n=== ВСЕ РЕСУРСЫ ===\n";
        Dictionary<Item, string> allResourcesWithStatus = GetAllResourcesWithStatus();

        foreach (var kvp in allResourcesWithStatus)
            debugInfo += $"{kvp.Value}\n";

        _debugText.text = debugInfo;
    }

    private Dictionary<Item, string> GetAllResourcesWithStatus()
    {
        Dictionary<Item, string> result = new Dictionary<Item, string>();

        Item[] allItems = FindObjectsOfType<Item>(true);
        foreach (var item in allItems)
        {
            if (item != null)
            {
                string status = "НЕИЗВЕСТНО";

                if (!item.gameObject.activeInHierarchy)
                    status = "В ПУЛЕ";
                else if (_resourceManager != null && _resourceManager.IsResourceReserved(item))
                    status = "ЗАНЯТ";
                else if (_resourceManager != null && _resourceManager.IsResourceFree(item))
                    status = "СВОБОДЕН";
                else
                    status = "АКТИВЕН";

                result[item] = $"{status}: {item.name} at {item.transform.position}";
            }
        }

        return result;
    }

    private int CountActiveResourcesOnScene()
    {
        int count = 0;
        Item[] allItems = FindObjectsOfType<Item>(true);

        foreach (var item in allItems)
            if (item != null && item.gameObject.activeInHierarchy)
                count++;

        return count;
    }

    private int CountItemsInPool()
    {
        if (_itemPool == null)
            return 0;

        return _itemPool.transform.childCount;
    }
}
#endif