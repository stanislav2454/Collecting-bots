//using UnityEngine;

//public class ItemTestHandler : MonoBehaviour
//{//после рефакторинга УДАЛИТЬ!
//    [Header("Test Settings")]
//    [SerializeField] private KeyCode _spawnItemKey = KeyCode.I;
//    [SerializeField] private KeyCode _spawnBulkItemsKey = KeyCode.O;
//    [SerializeField] private KeyCode _checkInventoryKey = KeyCode.C;
//    [SerializeField] private KeyCode _clearAllItemsKey = KeyCode.X;

//    [Header("Spawn Settings")]
//    [SerializeField] private int _bulkSpawnCount = 5;

//    [Header("UI Settings")]
//    [SerializeField] private int _headerFontSize = 14;
//    [SerializeField] private int _normalFontSize = 12;

//    private void Update()
//    {
//        HandleItemSpawning();
//        HandleCleanup();
//    }

//    private void HandleItemSpawning()
//    {
//        if (Input.GetKeyDown(_spawnItemKey))
//            SpawnSingleItem();

//        if (Input.GetKeyDown(_spawnBulkItemsKey))
//            SpawnBulkItems(_bulkSpawnCount);
//    }

//    private void HandleCleanup()
//    {
//        if (Input.GetKeyDown(_clearAllItemsKey))
//            ClearAllItems();
//    }

//    private void SpawnSingleItem()
//    {
//        ItemManager itemManager = ItemManager.Instance;

//        if (itemManager != null)
//        {
//            Item newItem = itemManager.SpawnItem();// зачем ?
//        }
//    }

//    private void SpawnBulkItems(int count)
//    {
//        ItemManager itemManager = ItemManager.Instance;

//        if (itemManager != null)
//        {
//            int successCount = 0;
//            for (int i = 0; i < count; i++)
//            {
//                Item newItem = itemManager.SpawnItem();
//                if (newItem != null) successCount++;
//            }
//        }
//    }

//    private void ClearAllItems()
//    {
//        Item[] allItems = FindObjectsOfType<Item>();//todo //ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую
//        int count = allItems.Length;

//        foreach (var item in allItems)
//            Destroy(item.gameObject);
//    }

//    private void OnGUI()
//    {
//        Color originalColor = GUI.color;
//        GUI.color = Color.cyan;

//        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
//        headerStyle.fontStyle = FontStyle.Bold;
//        headerStyle.fontSize = _headerFontSize;

//        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
//        normalStyle.fontSize = _normalFontSize;

//        GUILayout.BeginArea(new Rect(Screen.width - 310, 100, 300, 250));

//        GUILayout.Label("=== ITEM TEST CONTROLS ===");
//        GUILayout.Label($"{_spawnItemKey}: Spawn single item");
//        GUILayout.Label("1: Spawn multiple items");
//        GUILayout.Label($"{_checkInventoryKey}: Check bot inventories");
//        GUILayout.Label($"{_clearAllItemsKey}: Clear all items");

//        GUILayout.Space(10);

//        Item[] allItems = FindObjectsOfType<Item>();//todo //ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

//        int availableItems = 0;

//        foreach (var item in allItems)
//            if (item.CanBeCollected) availableItems++;

//        GUILayout.Label("Item Statistics:", headerStyle);
//        GUILayout.Label($"Total items: {allItems.Length}", normalStyle);
//        GUILayout.Label($"Available for collection: {availableItems}", normalStyle);

//        GUILayout.Space(5);

//        BotController[] bots = FindObjectsOfType<BotController>();//todo //ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

//        int botsWithItems = 0;
//        int fullBots = 0;

//        foreach (var bot in bots)
//        {
//            if (bot.BotInventory.CurrentCount > 0) botsWithItems++;
//            if (bot.BotInventory.IsFull) fullBots++;
//        }

//        GUILayout.Label("Bot Statistics:", headerStyle);
//        GUILayout.Label($"Bots with items: {botsWithItems}/{bots.Length}", normalStyle);
//        GUILayout.Label($"Full bots: {fullBots}", normalStyle);
//        GUILayout.Label($"Empty bots: {bots.Length - botsWithItems}", normalStyle);

//        GUILayout.EndArea();
//        GUI.color = originalColor;
//    }
//}