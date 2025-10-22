using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private float _collectionDuration = 1f;

    private NavMeshAgent _navMeshAgent;
    private BotInventory _inventory;
    private BaseController _assignedBase;
    private Item _assignedResource;
    private BotState _currentState;
    private float _stateTimer;

    public event Action<BotController, bool> MissionCompleted;

    public bool IsAvailable => _currentState is IdleState or WaitingState;
    public bool IsCarryingResource => _inventory.HasItem;

    // Базовый класс состояния
    private abstract class BotState
    {
        public abstract void Enter(BotController bot);
        public abstract void Update(BotController bot);
        public virtual void Exit(BotController bot) { }
    }

    // Конкретные состояния
    private class IdleState : BotState
    {
        public override void Enter(BotController bot) { }
        public override void Update(BotController bot) { }
    }

    private class WaitingState : BotState
    {
        public override void Enter(BotController bot) => bot.StopMovement();
        public override void Update(BotController bot) { }
    }

    private class MovingToResourceState : BotState
    {
        public override void Enter(BotController bot)
        {
            if (bot._assignedResource != null)
            {
                bot.MoveToPosition(bot._assignedResource.transform.position);
            }
        }

        public override void Update(BotController bot)
        {
            if (bot._assignedResource == null || bot._assignedResource.CanBeCollected == false)
            {
                bot.CompleteMission(false);
                return;
            }

            if (bot.HasReachedDestination())
            {
                bot.ChangeState(new CollectingState());
            }
        }
    }

    private class CollectingState : BotState
    {
        public override void Enter(BotController bot)
        {
            bot.StopMovement();
            bot._stateTimer = 0f;
        }

        public override void Update(BotController bot)
        {
            bot._stateTimer += Time.deltaTime;

            if (bot._stateTimer >= bot._collectionDuration)
            {
                if (bot._assignedResource != null && bot._inventory.TryAddItem(bot._assignedResource))
                {
                    bot._assignedResource.Collect();
                    bot.ChangeState(new ReturningToBaseState());
                }
                else
                {
                    bot.CompleteMission(false);
                }
            }
        }
    }

    private class ReturningToBaseState : BotState
    {
        public override void Enter(BotController bot)
        {
            if (bot._assignedBase != null)
            {
                bot.MoveToPosition(bot._assignedBase.transform.position);
            }
        }

        public override void Update(BotController bot)
        {
            if (bot.HasReachedDestination())
            {
                bot._inventory.ClearInventory();
                bot.CompleteMission(true);
            }
        }
    }

    private void Awake()
    {
        if (TryGetComponent(out _navMeshAgent) && TryGetComponent(out _inventory))
        {
            ChangeState(new IdleState());
        }
    }

    public void AssignBase(BaseController baseController)
    {
        _assignedBase = baseController;
        ChangeState(new IdleState());
    }

    public void AssignResource(Item resource)
    {
        if (IsAvailable == false)
            return;

        _assignedResource = resource;
        ChangeState(new MovingToResourceState());
    }

    public void SetWaiting()
    {
        ChangeState(new WaitingState());
    }

    private void Update()
    {
        _currentState?.Update(this);
    }

    private void ChangeState(BotState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
    }

    private void CompleteMission(bool success)
    {
        _assignedResource = null;
        ChangeState(new IdleState());
        MissionCompleted?.Invoke(this, success);
    }

    private void MoveToPosition(Vector3 position)
    {
        _navMeshAgent.SetDestination(position);
    }

    private void StopMovement()
    {
        _navMeshAgent.ResetPath();
    }

    private bool HasReachedDestination()
    {
        return _navMeshAgent.pathPending == false &&
               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }
}