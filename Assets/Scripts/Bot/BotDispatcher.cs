using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(BotController))]
public class BotDispatcher : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ResourceAllocator _resourceAllocator;

    private List<Bot> _availableBots = new List<Bot>();
    private Queue<Item> _pendingResources = new Queue<Item>();
    private Dictionary<Item, Bot> _resourceAssignments = new Dictionary<Item, Bot>();
    private Dictionary<Bot, Coroutine> _botAssignmentCoroutines = new Dictionary<Bot, Coroutine>();

    public void Initialize(ResourceAllocator resourceAllocator)
    {
        _resourceAllocator = resourceAllocator;

        if (_resourceAllocator != null)
        {
            _resourceAllocator.ResourceBecameAvailable -= OnResourceBecameAvailable;
            _resourceAllocator.ResourceBecameAvailable += OnResourceBecameAvailable;
        }
    }

    public void RegisterBot(Bot bot)
    {
        if (bot != null && !_availableBots.Contains(bot))
        {
            _availableBots.Add(bot);
            bot.MissionCompleted += OnBotMissionCompleted;

            TryAssignResourceToBot(bot);
        }
    }

    public void UnregisterBot(Bot bot)
    {
        if (bot != null)
        {
            _availableBots.Remove(bot);
            bot.MissionCompleted -= OnBotMissionCompleted;

            var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == bot);
            if (assignment.Key != null)
            {
                _resourceAssignments.Remove(assignment.Key);
                _resourceAllocator?.ReleaseResource(assignment.Key);
            }

            if (_botAssignmentCoroutines.ContainsKey(bot))
            {
                StopCoroutine(_botAssignmentCoroutines[bot]);
                _botAssignmentCoroutines.Remove(bot);
            }
        }
    }

    public Bot GetAvailableBotForConstruction()
    {
        var availableBot = _availableBots.FirstOrDefault(b => b.IsAvailable);
        if (availableBot != null)
        {
            _availableBots.Remove(availableBot);
            return availableBot;
        }

        availableBot = _availableBots.FirstOrDefault(b =>
            b.CurrentStateType != BotStateType.Collecting &&
            b.CurrentStateType != BotStateType.ReturningToBase);

        if (availableBot != null)
        {
            _availableBots.Remove(availableBot);

            var assignment = _resourceAssignments.FirstOrDefault(x => x.Value == availableBot);
            if (assignment.Key != null)
            {
                _resourceAssignments.Remove(assignment.Key);
                _resourceAllocator?.ReleaseResource(assignment.Key);
            }
        }

        return availableBot;
    }

    public void ReturnBotToAvailable(Bot bot)
    {
        if (bot != null && !_availableBots.Contains(bot))
        {
            _availableBots.Add(bot);
            TryAssignResourceToBot(bot);
        }
    }

    private void OnResourceBecameAvailable(Item resource)
    {
        if (_availableBots.Count > 0)
            TryAssignResourceToBot(_availableBots[0]);
        else
            _pendingResources.Enqueue(resource);
    }

    private void OnBotMissionCompleted(Bot bot, bool success)
    {
        var coroutine = StartCoroutine(ReturnBotAfterDelay(bot, 0.5f));
        _botAssignmentCoroutines[bot] = coroutine;
    }

    private IEnumerator ReturnBotAfterDelay(Bot bot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bot != null)
            ReturnBotToAvailable(bot);

        _botAssignmentCoroutines.Remove(bot);
    }

    private void TryAssignResourceToBot(Bot bot)
    {
        if (bot == null || bot.IsAvailable == false || _resourceAllocator == null)
            return;

        Item resource = _resourceAllocator.GetNearestAvailableResource(bot.transform.position);
        if (resource != null && _resourceAllocator.TryReserveResource(resource))
        {
            _availableBots.Remove(bot);
            _resourceAssignments[resource] = bot;
            bot.AssignResource(resource);
        }
    }

    private void OnDestroy()
    {
        foreach (var coroutine in _botAssignmentCoroutines.Values)
            if (coroutine != null) StopCoroutine(coroutine);
    }
}