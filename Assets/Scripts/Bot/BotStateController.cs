using System;
using System.Collections;
using UnityEngine;

public class BotStateController
{
    private BotState _currentState;
    private BotStateType _currentStateType;

    public event Action<BotStateType> StateChanged;

    public BotStateType CurrentStateType => _currentStateType;

    public void ChangeState(BotState newState, BotController bot)
    {
        _currentState?.Exit(bot);
        _currentState = newState;
        _currentStateType = newState.StateType;
        _currentState?.Enter(bot);

        StateChanged?.Invoke(_currentStateType);
    }

    public void UpdateState(BotController bot)
    {
        _currentState?.Update(bot);
    }
}

public enum BotStateType
{
    Idle,
    Waiting,
    MovingToResource,
    Collecting,
    ReturningToBase
}

public abstract class BotState
{
    public abstract BotStateType StateType { get; }
    public abstract void Enter(BotController bot);
    public abstract void Update(BotController bot);
    public virtual void Exit(BotController bot) { }
}

public class BotIdleState : BotState
{
    public override BotStateType StateType => BotStateType.Idle;

    public override void Enter(BotController bot) { }
    public override void Update(BotController bot) { }
}

public class BotWaitingState : BotState
{
    public override BotStateType StateType => BotStateType.Waiting;

    public override void Enter(BotController bot) =>
        bot.StopMovement();

    public override void Update(BotController bot) { }
}

public class BotMovingToResourceState : BotState
{
    private float _lastCheckTime;
    private const float ChekInterval = 0.2f;

    public override BotStateType StateType => BotStateType.MovingToResource;

    public override void Enter(BotController bot)
    {
        if (bot.AssignedResource != null)
        {
            bot.MoveToPosition(bot.AssignedResource.transform.position);
            _lastCheckTime = 0f;
        }
        else
        {
            bot.CompleteMission(false);
        }
    }

    public override void Update(BotController bot)
    {
        _lastCheckTime += Time.deltaTime;

        if (_lastCheckTime >= ChekInterval)
        {
            _lastCheckTime = 0f;

            if (IsResourceValid(bot) == false)
            {
                bot.CompleteMission(false);
                return;
            }

            if (bot.HasReachedDestination())
            {
                if (IsResourceValid(bot))
                    bot.ChangeState(new BotCollectingState());
                else
                    bot.CompleteMission(false);
            }
        }
    }

    private bool IsResourceValid(BotController bot)
    {
        return bot.AssignedResource != null &&
               bot.AssignedResource.CanBeCollected &&
               bot.AssignedResource.gameObject.activeInHierarchy;
    }
}

public class BotCollectingState : BotState
{
    private Coroutine _collectionCoroutine;

    public override BotStateType StateType => BotStateType.Collecting;

    public override void Enter(BotController bot)
    {
        if (IsResourceValid(bot) == false)
        {
            bot.CompleteMission(false);
            return;
        }

        bot.StopMovement();

        _collectionCoroutine = bot.StartCoroutine(CollectionRoutine(bot));
    }

    public override void Update(BotController bot) { }

    public override void Exit(BotController bot)
    {
        if (_collectionCoroutine != null)
        {
            bot.StopCoroutine(_collectionCoroutine);
            _collectionCoroutine = null;
        }
    }

    private IEnumerator CollectionRoutine(BotController bot)
    {
        if (bot.AssignedResource != null)
            bot.transform.LookAt(bot.AssignedResource.transform);

        yield return new WaitForSeconds(0.1f);
        float timer = 0f;

        while (timer < bot.CollectionDuration)
        {
            if (IsResourceValid(bot) == false)
            {
                bot.CompleteMission(false);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        CompleteCollection(bot);
    }

    private void CompleteCollection(BotController bot)
    {
        if (IsResourceValid(bot))
        {
            bot.AssignedResource.Collect();

            if (bot.Inventory.TryAddItem(bot.AssignedResource))
            {
                bot.ChangeState(new BotReturningToBaseState());
                return;
            }
        }

        bot.Inventory.ReleaseItem();
        bot.CompleteMission(false);
    }

    private bool IsResourceValid(BotController bot)
    {
        return bot.AssignedResource != null &&
               bot.AssignedResource.CanBeCollected &&
               bot.AssignedResource.gameObject.activeInHierarchy;
    }
}

public class BotReturningToBaseState : BotState
{
    private const float CheckInterval = 0.2f;

    private float _lastCheckTime;

    public override BotStateType StateType => BotStateType.ReturningToBase;

    public override void Enter(BotController bot)
    {
        if (bot.AssignedBase != null)
        {
            var unloadPosition = bot.AssignedBase.GetRandomUnloadPosition();
            bot.MoveToPosition(unloadPosition);
            _lastCheckTime = 0f;
        }
        else
        {
            bot.CompleteMission(false);
        }
    }

    public override void Update(BotController bot)
    {
        _lastCheckTime += Time.deltaTime;

        if (_lastCheckTime >= CheckInterval)
        {
            _lastCheckTime = 0f;

            if (bot.HasReachedDestination())
                bot.CompleteMission(true);
        }
    }
}