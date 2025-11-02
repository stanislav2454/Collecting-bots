using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BotManager : MonoBehaviour, IBotManager// класс перегружен
{
    [Header("Bot Settings")]
    [SerializeField] [Range(1, 10)] private int _initialBotsCount = 3;
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private Transform _spawnContainer;

    [Header("Dependencies")]
    [SerializeField] private BaseController _baseController;
    [SerializeField] private ResourceManager _resourceManager;

    private List<Bot> _bots = new List<Bot>();
    private Dictionary<Item, Bot> _resourceAssignments = new Dictionary<Item, Bot>();
    private BotFactory _botFactory;

    public Vector3 BasePosition => _baseController != null ? _baseController.transform.position : transform.position;
    public float UnloadZoneRadius => _baseController != null ? _baseController.UnloadZoneRadius : 1.5f;
    public int BotCount => _bots.Count;
    public int AvailableBotsCount => _bots.Count(b => b.IsAvailable);

    private void Awake()
    {
        InitializeBotFactory();
    }

    private void Start()
    {
        if (_resourceManager == null)
        {
            var gameDependencies = GameDependencies.Instance;
            if (gameDependencies != null)
            {
                _resourceManager = gameDependencies.ResourceManager;

                if (_resourceManager != null)
                    Debug.Log("ResourceManager successfully assigned to BotManager via GameDependencies");
            }
        }

        if (_resourceManager == null)
        {
            Debug.LogError("ResourceManager not found and not assigned in BotManager!");
            return;
        }

        ValidateDependencies();
        SpawnInitialBots();
    }

    public void SetBaseController(BaseController baseController)
    {
        _baseController = baseController;
        if (_baseController == null)
            Debug.LogWarning("BaseController set to null in BotManager!");
    }

    public void SetResourceManager(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
        if (_resourceManager == null)
            Debug.LogWarning("ResourceManager set to null in BotManager!");
        else
            Debug.Log("ResourceManager successfully assigned to BotManager");
    }

    public void AssignResourceToBot(Bot bot)
    {
        if (bot.IsAvailable && IsBotAssigned(bot) == false)
        {
            Item resource = _resourceManager?.GetNearestAvailableResource(bot.transform.position);

            if (resource != null && TryAssignResourceToBot(resource, bot))
                bot.AssignResource(resource, BasePosition, UnloadZoneRadius);
        }
    }

    public void AssignBotToResource(Item resource)
    {
        if (resource == null || IsResourceAssigned(resource))
            return;

        var nearestBot = _bots
            .Where(b => b.IsAvailable && IsBotAssigned(b) == false)
            .OrderBy(b => (b.transform.position - resource.transform.position).sqrMagnitude)
            .FirstOrDefault();

        if (nearestBot != null && TryAssignResourceToBot(resource, nearestBot))
            nearestBot.AssignResource(resource, BasePosition, UnloadZoneRadius);
    }

    public void CreateNewBot()
    {
        if (_botFactory != null)
            _botFactory.CreateBot(BasePosition);
    }

    public void CompleteAssignment(Bot bot, bool success)
    {
        var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == bot);
        if (assignment.Key != null)
        {
            _resourceAssignments.Remove(assignment.Key);

            if (success)
                _resourceManager?.MarkAsCollected(assignment.Key);
            else
                _resourceManager?.ReleaseResource(assignment.Key);
        }
    }

    public bool IsBotAssigned(Bot bot) =>
        _resourceAssignments.Values.Contains(bot);

    public bool IsResourceAssigned(Item resource) =>
        _resourceAssignments.ContainsKey(resource);

    public Bot GetAvailableBotForTransfer()
    {
        return _bots
            .Where(b => b.IsAvailable && !IsBotAssigned(b))
            .OrderBy(b => (b.transform.position - transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    public bool TransferBotToNewBase(Bot bot, BaseController newBase)
    {
        if (bot == null || newBase == null)
        {
            Debug.LogError("Cannot transfer bot: bot or newBase is null");
            return false;
        }

        if (_bots.Contains(bot) == false)
        {
            Debug.LogError($"Cannot transfer bot: bot {bot.name} not found in this manager");
            return false;
        }

        var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == bot);
        if (assignment.Key != null)
        {
            _resourceAssignments.Remove(assignment.Key);
            _resourceManager?.ReleaseResource(assignment.Key);
        }

        _bots.Remove(bot);
        bot.MissionCompleted -= HandleBotMissionCompleted;
        bot.SetWaiting();

        Debug.Log($"Bot {bot.name} transferred to new base");
        return true;
    }

    public void AddExistingBot(Bot bot)
    {
        if (bot == null || _bots.Contains(bot))
            return;

        _bots.Add(bot);
        bot.MissionCompleted += HandleBotMissionCompleted;

        bot.transform.SetParent(_spawnContainer);

        Debug.Log($"Bot {bot.name} added to base manager");
    }

    private void InitializeBotFactory()
    {
        float spawnRadius = _baseController != null ? _baseController.SpawnZoneRadius : 3f;
        _botFactory = new BotFactory(_botPrefab, _spawnContainer, spawnRadius);
        _botFactory.BotCreated += OnBotCreated;
    }

    private void OnBotCreated(Bot newBot)
    {
        InitializeBot(newBot);
    }

    private bool TryAssignResourceToBot(Item resource, Bot bot)
    {
        if (resource == null || bot == null || _resourceManager == null)
            return false;

        if (_resourceAssignments.ContainsKey(resource) || IsBotAssigned(bot))
            return false;

        if (_resourceManager.TryReserveResource(resource))
        {
            _resourceAssignments[resource] = bot;
            return true;
        }

        return false;
    }

    private void ValidateDependencies()
    {
        if (_botPrefab == null)
            Debug.LogError("Prefab Bot not assigned in BotManager!");

        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in BotManager!");

        if (_botFactory == null)
            Debug.LogError("ResourceManager not Created in BotManager!");
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            CreateNewBot();
    }

    private void InitializeBot(Bot bot)
    {
        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);

        if (_resourceManager != null && _resourceManager.HasAvailableResources)
            AssignResourceToBot(bot);
    }

    private void HandleBotMissionCompleted(Bot bot, bool success)
    {
        CompleteAssignment(bot, success);

        if (success && _baseController != null)
            _baseController.CollectResourceFromBot(bot);

        StartCoroutine(AssignNewResourceAfterDelay(bot, 0.5f));
    }

    private IEnumerator AssignNewResourceAfterDelay(Bot bot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_resourceManager?.FreeResourcesCount > 0)
            AssignResourceToBot(bot);
        else
            bot.SetWaiting();
    }

    private void OnDestroy()
    {
        if (_botFactory != null)
            _botFactory.BotCreated -= OnBotCreated;

        foreach (var bot in _bots)
            if (bot != null)
                bot.MissionCompleted -= HandleBotMissionCompleted;
    }

    public void SetBaseController(IBaseController baseController)// todo: зачем, может удалить ?
    {
        //_baseController = baseController;
    }

    bool IBotManager.TransferBotToNewBase(Bot bot, IBaseController newBase)// todo: модификатор доступа ?
    {
        if (newBase is BaseController concreteBase)
            return TransferBotToNewBase(bot, concreteBase);

        return false;
    }
}