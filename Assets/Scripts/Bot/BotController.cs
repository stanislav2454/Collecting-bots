using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private List<Bot> _bots = new List<Bot>();
    private Dictionary<Item, Bot> _resourceAssignments = new Dictionary<Item, Bot>();
    private Dictionary<Bot, Coroutine> _botAssignmentCoroutines = new Dictionary<Bot, Coroutine>();
    private bool _isInitialized = false;

    public Vector3 BasePosition => _baseController != null ? _baseController.transform.position : transform.position;
    public float UnloadZoneRadius => _baseController != null ? _baseController.UnloadZoneRadius : 1.5f;
    public int BotCount => _bots.Count;

    private void OnDestroy()
    {
        foreach (var coroutinePair in _botAssignmentCoroutines)
            if (coroutinePair.Value != null)
                StopCoroutine(coroutinePair.Value);

        _botAssignmentCoroutines.Clear();
    }

    public void Initialize(BaseController baseController, ResourceAllocator resourceManager)
    {
        _baseController = baseController;
        _resourceAllocator = resourceManager;

        InitializeBotFactory();
        _isInitialized = true;

        if (_bots.Count == 0 && _initialBotsCount > 0)
            SpawnInitialBots();
    }

    public void CreateNewBot()
    {
        if (_isInitialized == false)
            return;

        _botFactory?.CreateBot(BasePosition);
    }

    public void HandleBotMissionCompleted(Bot bot, bool success)
    {
        if (Application.isPlaying == false || _isInitialized == false)
            return;

        CompleteAssignment(bot, success);

        if (_botAssignmentCoroutines.ContainsKey(bot))
        {
            StopCoroutine(_botAssignmentCoroutines[bot]);
            _botAssignmentCoroutines.Remove(bot);
        }

        const float Delay = 0.5f;
        var coroutine = StartCoroutine(AssignNewResourceAfterDelay(bot, Delay));
        _botAssignmentCoroutines[bot] = coroutine;
    }

    public void AssignResourceToBot(Bot bot)
    {
        if (Application.isPlaying == false || _isInitialized == false)
            return;

        if (bot == null || bot.IsAvailable == false)
            return;

        if (IsBotAssigned(bot))
            return;

        Item resource = _resourceAllocator?.GetNearestAvailableResource(bot.transform.position);

        if (resource != null && TryAssignResourceToBot(resource, bot))
            bot.AssignResource(resource, BasePosition, UnloadZoneRadius);
    }

    public void AssignBotToResource(Item resource)
    {
        if (Application.isPlaying == false || _isInitialized == false)
            return;

        if (resource == null || IsResourceAssigned(resource))
            return;

        var nearestBot = _bots
            .Where(b => b.IsAvailable && IsBotAssigned(b) == false)
            .OrderBy(b => (b.transform.position - resource.transform.position).sqrMagnitude)
            .FirstOrDefault();

        if (nearestBot != null && TryAssignResourceToBot(resource, nearestBot))
            nearestBot.AssignResource(resource, BasePosition, UnloadZoneRadius);
    }

    public void CompleteAssignment(Bot bot, bool success)
    {
        if (Application.isPlaying == false || gameObject.scene.name == null || _isInitialized == false)
            return;

        var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == bot);
        if (assignment.Key != null)
        {
            _resourceAssignments.Remove(assignment.Key);

            if (success)
                _resourceAllocator?.MarkAsCollected(assignment.Key);
            else
                _resourceAllocator?.ReleaseResource(assignment.Key);
        }
    }

    public bool IsBotAssigned(Bot bot) =>
        _resourceAssignments.Values.Contains(bot);

    public bool IsResourceAssigned(Item resource) =>
        _resourceAssignments.ContainsKey(resource);

    public Bot GetAvailableBotForConstruction()
    {
        var availableBot = _bots.FirstOrDefault
                (b => b != null && b.CurrentStateType == BotStateType.Idle);

        if (availableBot != null)
            return availableBot;

        availableBot = _bots.FirstOrDefault(b =>
                b != null &&
                IsBotAssigned(b) == false &&
                b.CurrentStateType != BotStateType.Collecting &&
                b.CurrentStateType != BotStateType.ReturningToBase);

        if (availableBot != null)
            return availableBot;

        return null;
    }

    public Bot ForceGetBotForConstruction()
    {
        if (_bots.Count > 0)
        {
            var bot = _bots[0];

            if (IsBotAssigned(bot))
            {
                var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == bot);
                if (assignment.Key != null)
                {
                    _resourceAssignments.Remove(assignment.Key);
                    _resourceAllocator?.ReleaseResource(assignment.Key);
                }
            }

            bot.ChangeState(new BotIdleState());
            return bot;
        }

        return null;
    }

    public void AssignResourcesToAllBots()
    {
        if (Application.isPlaying == false || gameObject.scene.name == null || _isInitialized == false)
            return;

        foreach (var bot in _bots)
            if (bot != null && bot.IsAvailable)
                AssignResourceToBot(bot);
    }

    private bool TryAssignResourceToBot(Item resource, Bot bot)
    {
        if (resource == null || bot == null || _resourceAllocator == null)
            return false;

        if (_resourceAssignments.ContainsKey(resource) || IsBotAssigned(bot))
            return false;

        if (_resourceAllocator.TryReserveResource(resource))
        {
            _resourceAssignments[resource] = bot;
            return true;
        }

        return false;
    }

    private IEnumerator AssignNewResourceAfterDelay(Bot bot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_resourceAllocator?.FreeResourcesCount > 0)
            AssignResourceToBot(bot);
        else
            bot.SetWaiting();
    }

    private void InitializeBot(Bot bot)
    {
        if (bot == null)
            return;

        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);

        if (_isInitialized && _resourceAllocator != null && _resourceAllocator.HasAvailableResources)
            AssignResourceToBot(bot);
    }

    private void InitializeBotFactory()
    {
        if (_baseController == null)
        {
            Debug.LogError("BaseController not set in BotManager!");
            return;
        }

        float spawnRadius = _baseController.SpawnZoneRadius;

        _botFactory = new BotFactory(_botPrefab, _spawnContainer, spawnRadius);
        _botFactory.BotCreated += InitializeBot;
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            CreateNewBot();
    }
}