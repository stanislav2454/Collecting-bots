using System;
using System.Collections;
using UnityEngine;

public class BotStateController
{
    private BotState _currentState;
    private BotStateType _currentStateType;

    public event Action<BotStateType> StateChanged;

    public BotStateType CurrentStateType => _currentStateType;

    public void ChangeState(BotState newState, Bot bot)
    {
        _currentState?.Exit(bot);
        _currentState = newState;
        _currentStateType = newState.StateType;
        _currentState?.Enter(bot);

        StateChanged?.Invoke(_currentStateType);
    }

    public void UpdateState(Bot bot)
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
    public abstract void Enter(Bot bot);
    public abstract void Update(Bot bot);
    public virtual void Exit(Bot bot) { }
    // ОБЩИЙ МЕТОД ДЛЯ ВСЕХ СОСТОЯНИЙ
    protected bool IsResourceValid(Item resource)
    {
        return resource != null && resource.CanBeCollected && resource.gameObject.activeInHierarchy;
    }
}

public class BotIdleState : BotState
{
    public override BotStateType StateType => BotStateType.Idle;

    public override void Enter(Bot bot) { }
    public override void Update(Bot bot) { }
}

public class BotWaitingState : BotState
{
    public override BotStateType StateType => BotStateType.Waiting;

    public override void Enter(Bot bot) =>
        bot.StopMovement();

    public override void Update(Bot bot) { }
}

public class BotMovingToResourceState : BotState
{
    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius; // Добавляем поле для радиуса
    private float _lastCheckTime;
    private const float ChekInterval = 0.2f;

    public override BotStateType StateType => BotStateType.MovingToResource;

    public BotMovingToResourceState(Item resource, Vector3 basePosition, float baseRadius)
    {
        _targetResource = resource;
        _basePosition = basePosition;
        _baseRadius = baseRadius; // Сохраняем радиус
    }

    public override void Enter(Bot bot)
    {
        if (_targetResource != null && _targetResource.CanBeCollected)
        {
            Vector3 targetPosition = _targetResource.transform.position;

            bot.MoveToPosition(_targetResource.transform.position);
            _lastCheckTime = 0f;
        }
        else
        {
            Debug.Log($"❌ Бот не может двигаться к ресурсу: {_targetResource?.name ?? "NULL"}");
            bot.CompleteMission(false);
        }
    }

    public override void Update(Bot bot)
    {
        _lastCheckTime += Time.deltaTime;

        if (_lastCheckTime >= ChekInterval)
        {
            _lastCheckTime = 0f;

            if (IsResourceValid(_targetResource) == false)
            {
                bot.CompleteMission(false);
                return;
            }

            if (bot.HasReachedDestination())
                bot.ChangeState(new BotCollectingState(_targetResource, _basePosition, _baseRadius));
        }
    }
}

public class BotCollectingState : BotState
{
    private const float Delay = 0.1f;

    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius; 
    private Coroutine _collectionCoroutine;

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

        _collectionCoroutine = bot.StartCoroutine(CollectionRoutine(bot));
    }

    public override void Update(Bot bot) { }

    public override void Exit(Bot bot)
    {
        if (_collectionCoroutine != null)
        {
            bot.StopCoroutine(_collectionCoroutine);
            _collectionCoroutine = null;
        }
    }

    private IEnumerator CollectionRoutine(Bot bot)
    {
        if (_targetResource != null)
            bot.transform.LookAt(_targetResource.transform);

        yield return new WaitForSeconds(Delay);

        float timer = 0f;
        while (timer < bot.CollectionDuration)
        {
            if (IsResourceValid(_targetResource) == false)
            {
                bot.CompleteMission(false);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

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
            bot.Inventory.ReleaseItem();
            bot.CompleteMission(false);
        }
    }
}

public class BotReturningToBaseState : BotState
{
    private const float CheckInterval = 0.2f;

    private Vector3 _basePosition;
    private float _baseRadiusSqr;
    private float _lastCheckTime;

    public override BotStateType StateType => BotStateType.ReturningToBase;

    public BotReturningToBaseState(Vector3 basePosition, float baseRadius)
    {
        _basePosition = basePosition;
        _baseRadiusSqr = baseRadius * baseRadius;
    }

    public override void Enter(Bot bot)
    {
        bot.MoveToPosition(_basePosition);
        _lastCheckTime = 0f;

        Debug.Log($"Бот движется к базе: {_basePosition}");
    }

    public override void Update(Bot bot)
    {
        _lastCheckTime += Time.deltaTime;

        if (_lastCheckTime >= CheckInterval)
        {
            _lastCheckTime = 0f;

            if (bot.HasReachedDestination() || IsAtBase(bot))
            {
                Debug.Log($"Бот достиг базы. Позиция: {bot.transform.position}, База: {_basePosition}");
                bot.CompleteMission(true);
            }
        }
    }

    private bool IsAtBase(Bot bot)
    {
        float sqrDistanceToBase = (bot.transform.position - _basePosition).sqrMagnitude;
        bool atBase = sqrDistanceToBase <= _baseRadiusSqr;

        if (atBase)
        {
            float distanceToBase = Mathf.Sqrt(sqrDistanceToBase);
            Debug.Log($"Бот в радиусе базы. Расстояние: {distanceToBase:F2}");
        }

        return atBase;
    }
}