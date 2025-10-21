using UnityEngine;

public class ItemTestHandler : MonoBehaviour
{// пока не понятно где и для чего этот класс => todo: потом спросить у DeepSeek
    [Header("Test Settings")]
    [SerializeField] private KeyCode _spawnItemKey = KeyCode.I;
    [SerializeField] private KeyCode _spawnBulkItemsKey = KeyCode.O;
    [SerializeField] private KeyCode _checkInventoryKey = KeyCode.C;
    [SerializeField] private KeyCode _clearAllItemsKey = KeyCode.X;

    [Header("Spawn Settings")]
    [SerializeField] private int _bulkSpawnCount = 5;

    [Header("UI Settings")]
    [SerializeField] private int _headerFontSize = 14;
    [SerializeField] private int _normalFontSize = 12;

    private void Update()
    {
        HandleItemSpawning();
        HandleInventoryTesting();
        HandleCleanup();
    }

    private void HandleItemSpawning()
    {
        if (Input.GetKeyDown(_spawnItemKey))
            SpawnSingleItem();

        if (Input.GetKeyDown(_spawnBulkItemsKey))
            SpawnBulkItems(_bulkSpawnCount);
    }

    private void HandleInventoryTesting()
    {
        if (Input.GetKeyDown(_checkInventoryKey))
            CheckAllBotInventories();
    }

    private void HandleCleanup()
    {
        if (Input.GetKeyDown(_clearAllItemsKey))
            ClearAllItems();
    }

    private void SpawnSingleItem()
    {
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            Item newItem = itemManager.SpawnItem();

            if (newItem != null)
                Debug.Log($"Spawned item: {newItem.ItemName} at {newItem.transform.position}");
            else
                Debug.LogWarning("Failed to spawn item");
        }
    }

    private void SpawnBulkItems(int count)
    {
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                Item newItem = itemManager.SpawnItem();
                if (newItem != null) successCount++;
            }
            Debug.Log($"Spawned {successCount}/{count} items in bulk");
        }
    }

    private void CheckAllBotInventories()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//todo
        Debug.Log("=== BOT INVENTORIES ===");

        if (bots.Length == 0)
        {
            Debug.Log("No bots found in scene");
            return;
        }

        foreach (var bot in bots)
        {
            string status = bot.BotInventory.IsFull ? "FULL" : "HAS SPACE";
            Debug.Log($"Bot: {bot.gameObject.name} | {status} | Items: " +
                $"{bot.BotInventory.CurrentCount}/{bot.BotInventory.MaxCapacity}");
        }
    }

    private void ClearAllItems()
    {
        Item[] allItems = FindObjectsOfType<Item>();//todo
        int count = allItems.Length;

        foreach (var item in allItems)
            Destroy(item.gameObject);

        Debug.Log($"Cleared {count} items from scene");
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;
        GUI.color = Color.cyan;

        // Создаем стили с настраиваемыми размерами шрифта
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = _headerFontSize;

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = _normalFontSize;

        // ПРАВАЯ ЧАСТЬ ЭКРАНА - под панелью режима управления
        GUILayout.BeginArea(new Rect(Screen.width - 310, 100, 300, 250));

        GUILayout.Label("=== ITEM TEST CONTROLS ===");
        GUILayout.Label($"{_spawnItemKey}: Spawn single item");
        GUILayout.Label("1: Spawn multiple items");
        GUILayout.Label($"{_checkInventoryKey}: Check bot inventories");
        GUILayout.Label($"{_clearAllItemsKey}: Clear all items");

        GUILayout.Space(10);

        // Статистика предметов
        Item[] allItems = FindObjectsOfType<Item>();//todo
        int availableItems = 0;
        foreach (var item in allItems)
            if (item.CanBeCollected) availableItems++;

        GUILayout.Label("Item Statistics:", headerStyle);
        GUILayout.Label($"Total items: {allItems.Length}", normalStyle);
        GUILayout.Label($"Available for collection: {availableItems}", normalStyle);

        GUILayout.Space(5);

        // Статистика ботов
        BotController[] bots = FindObjectsOfType<BotController>();//todo
        int botsWithItems = 0;
        int fullBots = 0;

        foreach (var bot in bots)
        {
            if (bot.BotInventory.CurrentCount > 0) botsWithItems++;
            if (bot.BotInventory.IsFull) fullBots++;
        }

        GUILayout.Label("Bot Statistics:", headerStyle);
        GUILayout.Label($"Bots with items: {botsWithItems}/{bots.Length}", normalStyle);
        GUILayout.Label($"Full bots: {fullBots}", normalStyle);
        GUILayout.Label($"Empty bots: {bots.Length - botsWithItems}", normalStyle);

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}