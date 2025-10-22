//using UnityEngine;
//using System.Collections.Generic;

//public class BotPool : MonoBehaviour
//{//после рефакторинга УДАЛИТЬ!
//    [Header("Pool Settings")]
//    [SerializeField] private GameObject _botPrefab;
//    [SerializeField] private int _initialPoolSize = 10;
//    [SerializeField] private int _maxPoolSize = 20;
//    [SerializeField] private Transform _poolContainer;

//    private Queue<GameObject> _availableBots = new Queue<GameObject>();
//    private List<GameObject> _activeBots = new List<GameObject>();
//    private int _createdBotsCount = 0;

//    private void Start()
//    {
//        if (_botPrefab != null)
//            InitializePool();
//    }

//    public void Initialize(GameObject botPrefab, int initialSize, int maxSize, Transform container = null)
//    {
//        _botPrefab = botPrefab;
//        _initialPoolSize = initialSize;
//        _maxPoolSize = maxSize;
//        _poolContainer = container;

//        InitializePool();
//    }

//    public GameObject GetBot(Vector3 position)
//    {
//        GameObject bot = null;

//        if (_availableBots.Count > 0)
//        {
//            bot = _availableBots.Dequeue();
//        }
//        else if (_createdBotsCount < _maxPoolSize)
//        {
//            bot = CreateNewBot();
//        }
//        else
//        {
//            Debug.LogWarning("No available bots in pool!");
//            return null;
//        }

//        if (bot != null)
//        {
//            bot.transform.position = position;
//            bot.SetActive(true);
//            _activeBots.Add(bot);

//            BotController botController = bot.GetComponent<BotController>();

//            if (botController != null)
//                botController.SetAIEnabled(true);
//        }

//        return bot;
//    }

//    public void ReturnBot(GameObject bot)
//    {
//        if (bot == null)
//            return;

//        bot.SetActive(false);
//        bot.transform.SetParent(_poolContainer);

//        BotController botController = bot.GetComponent<BotController>();

//        if (botController != null)
//        {
//            botController.StopMovement();
//            botController.SetAIEnabled(false);
//            botController.ClearTargetItem();
//        }

//        _activeBots.Remove(bot);
//        _availableBots.Enqueue(bot);
//    }

//    public void ReturnAllBots()// зачем нужен ? если не используется - удалить !
//    {
//        for (int i = _activeBots.Count - 1; i >= 0; i--)
//            ReturnBot(_activeBots[i]);
//    }

//    public int GetActiveBotsCount() =>
//        _activeBots.Count;

//    public int GetAvailableBotsCount() =>
//        _availableBots.Count;

//    public int GetTotalBotsCount() =>
//        _createdBotsCount;

//    public string GetPoolInfo() =>
//         $"Bots: {GetActiveBotsCount()} active, {GetAvailableBotsCount()} available," +
//        $" {GetTotalBotsCount()} total";

//    private void InitializePool()
//    {
//        if (_botPrefab == null)
//        {
//            Debug.LogError("Bot prefab is not assigned!");
//            return;
//        }

//        if (_poolContainer == null)
//        {
//            GameObject container = new GameObject("BotPool");
//            _poolContainer = container.transform;
//        }

//        for (int i = 0; i < _initialPoolSize; i++)
//            CreateNewBot();
//    }

//    private GameObject CreateNewBot()
//    {
//        if (_createdBotsCount >= _maxPoolSize)
//            return null;

//        GameObject bot = Instantiate(_botPrefab, _poolContainer);
//        bot.name = $"Bot_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
//        bot.SetActive(false);

//        _availableBots.Enqueue(bot);
//        _createdBotsCount++;

//        return bot;
//    }
//}