using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BotDispatcher))]
public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] [Range(0, 10)] private int _initialBotsCount = 3;
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private Transform _spawnContainer;

    [Header("Dependencies")]
    [SerializeField] private ResourceAllocator _resourceAllocator;

    private BaseController _baseController;
    private BotFactory _botFactory;
    private BotDispatcher _botDispatcher;
    private List<Bot> _allBots = new List<Bot>();
    private bool _isInitialized = false;

    public Vector3 BasePosition => _baseController != null ? _baseController.transform.position : transform.position;
    public float UnloadZoneRadius => _baseController != null ? _baseController.UnloadZoneRadius : 1.5f;
    public int BotCount => _allBots.Count;

    private void OnDestroy()
    {
        if (_botFactory != null)
            _botFactory.BotCreated -= OnBotCreated;
    }

    public void Initialize(BaseController baseController, ResourceAllocator resourceAllocator)
    {
        _baseController = baseController;
        _resourceAllocator = resourceAllocator;

        InitializeBotFactory();
        InitializeBotDispatcher();

        _isInitialized = true;

        if (_allBots.Count == 0 && _initialBotsCount > 0)
            SpawnInitialBots();
    }

    public void CreateNewBot()
    {
        if (_isInitialized == false)
            return;

        _botFactory?.CreateBot(BasePosition);
    }

    public Bot GetAvailableBotForConstruction()
    {
        return _botDispatcher.GetAvailableBotForConstruction();
    }

    public Bot ForceGetBotForConstruction()
    {
        if (_allBots.Count > 0)
        {
            var bot = _allBots[0];
            return _botDispatcher.GetAvailableBotForConstruction() ?? bot;
        }

        return null;
    }

    public void TransferBotToNewController(Bot bot, BotController newController)
    {
        if (bot == null || newController == null)
            return;

        if (_allBots.Contains(bot))
        {
            _allBots.Remove(bot);
            _botDispatcher.UnregisterBot(bot);
            newController.ReceiveTransferredBot(bot);
        }
    }

    public void ReceiveTransferredBot(Bot bot)
    {
        if (bot != null && !_allBots.Contains(bot))
        {
            _allBots.Add(bot);
            bot.transform.SetParent(_spawnContainer ?? transform);
            _botDispatcher.RegisterBot(bot);
        }
    }

    public void AssignBotToResource(Item resource)
    {
        if (resource != null && _botDispatcher != null)
        {
            var availableBot = _botDispatcher.GetAvailableBotForConstruction();
            if (availableBot != null)
                _botDispatcher.RegisterBot(availableBot);
        }
    }

    private void InitializeBotFactory()
    {
        if (_baseController == null)
            return;

        float spawnRadius = _baseController.SpawnZoneRadius;
        _botFactory = new BotFactory(_botPrefab, _spawnContainer, spawnRadius);
        _botFactory.BotCreated += OnBotCreated;
    }

    private void InitializeBotDispatcher()
    {
        _botDispatcher = gameObject.AddComponent<BotDispatcher>();
        _botDispatcher.Initialize(_resourceAllocator);
    }

    private void OnBotCreated(Bot bot)
    {
        if (bot != null)
        {
            _allBots.Add(bot);
            _botDispatcher.RegisterBot(bot);
        }
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            CreateNewBot();
    }
}
