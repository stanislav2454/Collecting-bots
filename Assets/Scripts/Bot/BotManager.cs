using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BotManager : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private float _spawnZoneRadius = 3f;

    [Header("Dependencies")]
    [SerializeField] private BaseController _baseController;

    private List<Bot> _bots = new List<Bot>();
    private ResourceAssignmentManager _assignmentManager = new ResourceAssignmentManager();

    public event System.Action<Bot> BotCreated;

    public Vector3 BasePosition => _baseController != null ? _baseController.transform.position : transform.position;
    public float UnloadZoneRadius => _baseController != null ? _baseController.UnloadZoneRadius : 1.5f;
    public int BotCount => _bots.Count;
    public int AvailableBotsCount => _bots.Count(b => b.IsAvailable);

    private void Start()
    {
        if (_baseController == null)
        {
            Debug.LogError("BaseController not assigned in BotManager!");
            return;
        }

        SpawnInitialBots();
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
            SpawnBot();
    }

    private void SpawnBot()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        Bot bot = Instantiate(_botPrefab, spawnPosition, Quaternion.identity);
        InitializeBot(bot);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 basePos = BasePosition;
        return basePos + new Vector3(
            Random.Range(-_spawnZoneRadius, _spawnZoneRadius),
            0,
            Random.Range(-_spawnZoneRadius, _spawnZoneRadius));
    }

    private void InitializeBot(Bot bot)
    {
        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);
        BotCreated?.Invoke(bot);

        if (ResourceManager.Instance != null && ResourceManager.Instance.HasAvailableResources)
            AssignResourceToBot(bot);
    }

    public void AssignResourceToBot(Bot bot)
    {
        if (bot.IsAvailable && _assignmentManager.IsBotAssigned(bot) == false)
        {
            Item resource = ResourceManager.Instance.GetNearestAvailableResource(bot.transform.position);
            if (resource != null && _assignmentManager.TryAssignResourceToBot(resource, bot))
            {
                Debug.Log($"Боту назначен ресурс: {resource.name} at {resource.transform.position}");
                bot.AssignResource(resource, BasePosition, UnloadZoneRadius);
            }
        }
    }

    public void AssignBotToResource(Item resource)
    {
        if (resource == null || _assignmentManager.IsResourceAssigned(resource))
        {
            Debug.Log($"❌ Ресурс {resource?.name ?? "NULL"} не может быть назначен");
            return;
        }

        var nearestBot = _bots
            .Where(b => b.IsAvailable && !_assignmentManager.IsBotAssigned(b))
            .OrderBy(b => (b.transform.position - resource.transform.position).sqrMagnitude)
            .FirstOrDefault();

        if (nearestBot != null && _assignmentManager.TryAssignResourceToBot(resource, nearestBot))
            nearestBot.AssignResource(resource, BasePosition, UnloadZoneRadius);
    }

    private void HandleBotMissionCompleted(Bot bot, bool success)
    {
        _assignmentManager.CompleteAssignment(bot, success);

        if (success && _baseController != null)
            _baseController.CollectResourceFromBot(bot);

        StartCoroutine(AssignNewResourceAfterDelay(bot, 0.5f));
    }

    private IEnumerator AssignNewResourceAfterDelay(Bot bot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ResourceManager.Instance?.FreeResourcesCount > 0)
            AssignResourceToBot(bot);
        else
            bot.SetWaiting();
    }

    public void DebugLogBotState()
    {
        int availableBots = _bots.Count(b => b.IsAvailable);
        int assignedBots = _bots.Count(b => _assignmentManager.IsBotAssigned(b));

        Debug.Log($"=== BOT MANAGER STATE ===");
        Debug.Log($"Total Bots: {_bots.Count}");
        Debug.Log($"Available Bots: {availableBots}");
        Debug.Log($"Assigned Bots: {assignedBots}");
        Debug.Log($"=========================");
    }
}