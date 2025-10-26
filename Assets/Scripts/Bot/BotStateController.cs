using System;
using UnityEngine;

public class BotStateController
{
    public event Action StateChanged;

    private BotState _currentState;
    private bool _stateChanged;

    public BotStateType CurrentStateType => _currentState?.StateType ?? BotStateType.Idle;
    public bool StateChange => _stateChanged;
    //private BotState _currentState;
    //private BotStateType _currentStateType;

    //public event Action<BotStateType> StateChanged;

    //public BotStateType CurrentStateType => _currentStateType;

    public void ChangeState(BotState newState, Bot bot)
    {
        //_currentState?.Exit(bot);
        //_currentState = newState;
        //_currentStateType = newState.StateType;
        //_currentState?.Enter(bot);
        _currentState?.Exit(bot);
        _currentState = newState;
        _currentState.Enter(bot);
        _stateChanged = true;
        StateChanged?.Invoke();
        //StateChanged?.Invoke(_currentStateType);
    }

    public void UpdateState(Bot bot)
    {
        _stateChanged = false;
        _currentState?.Update(bot);
    }
}

public enum BotStateType
{
    Idle,
    MovingToResource,
    Collecting,
    ReturningToBase
}

public abstract class BotState
{
    public abstract BotStateType StateType { get; }
    public abstract void Enter(Bot bot);
    public abstract void Update(Bot bot);
    public virtual void Exit(Bot bot) { }

    protected bool IsResourceValid(Item resource) =>
         resource != null && resource.gameObject.activeInHierarchy;

    protected bool CheckDestinationReached(Bot bot, ref float checkTimer, float checkInterval = 0.2f)
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            return bot.HasReachedDestination();
        }

        return false;
    }

    protected bool IsAtBase(Bot bot, Vector3 basePosition, float baseRadius)
    {
        float sqrDistanceToBase = (bot.transform.position - basePosition).sqrMagnitude;
        float baseRadiusSqr = baseRadius * baseRadius;
        return sqrDistanceToBase <= baseRadiusSqr;
    }
}

public class BotIdleState : BotState
{
    public override BotStateType StateType => BotStateType.Idle;

    public override void Enter(Bot bot)
    {
        bot.StopMovement();
    }

    public override void Update(Bot bot) { }
}

public class BotMovingToResourceState : BotState
{
    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _checkTimer;

    public override BotStateType StateType => BotStateType.MovingToResource;

    public BotMovingToResourceState(Item resource, Vector3 basePosition, float baseRadius)
    {
        _targetResource = resource;
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        if (IsResourceValid(_targetResource))
        {
            bot.MoveToPosition(_targetResource.transform.position);
            _checkTimer = 0f;
        }
        else
        {
            bot.CompleteMission(false);
        }
    }

    public override void Update(Bot bot)
    {
        if (CheckDestinationReached(bot, ref _checkTimer))
        {
            if (IsResourceValid(_targetResource))
                bot.ChangeState(new BotCollectingState(_targetResource, _basePosition, _baseRadius));
            else
                bot.CompleteMission(false);
        }
    }
}

public class BotCollectingState : BotState
{
    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _collectionTimer;

    public override BotStateType StateType => BotStateType.Collecting;

    public BotCollectingState(Item resource, Vector3 basePosition, float baseRadius)
    {
        _targetResource = resource;
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        if (IsResourceValid(_targetResource) == false)
        {
            bot.CompleteMission(false);
            return;
        }

        bot.StopMovement();

        if (_targetResource != null)
            bot.transform.LookAt(_targetResource.transform);

        _collectionTimer = 0f;
    }

    public override void Update(Bot bot)
    {
        if (IsResourceValid(_targetResource) == false)
        {
            bot.CompleteMission(false);
            return;
        }

        _collectionTimer += Time.deltaTime;

        if (_collectionTimer >= bot.CollectionDuration)
            CompleteCollection(bot);
    }

    private void CompleteCollection(Bot bot)
    {
        if (IsResourceValid(_targetResource) && bot.Inventory.TryAddItem(_targetResource))
        {
            _targetResource.Collect();
            bot.ChangeState(new BotReturningToBaseState(_basePosition, _baseRadius));
        }
        else
        {
            bot.CompleteMission(false);
        }
    }
}

public class BotReturningToBaseState : BotState
{
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _checkTimer;

    public override BotStateType StateType => BotStateType.ReturningToBase;

    public BotReturningToBaseState(Vector3 basePosition, float baseRadius)
    {
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        bot.MoveToPosition(_basePosition);
        _checkTimer = 0f;
    }

    public override void Update(Bot bot)
    {
        if (CheckDestinationReached(bot, ref _checkTimer) || IsAtBase(bot, _basePosition, _baseRadius))
            bot.CompleteMission(true);
    }
}