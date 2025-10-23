using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private float _collectionDuration = 1f;

    // Компоненты
    private BotMovementController _movement;
    //private NavMeshAgent _navMeshAgent;
    private BotInventory _inventory;
    private BotStateController _stateController;

    // Данные
    private BaseController _assignedBase;
    private Item _assignedResource;
    //  private BotState _currentState;
    private float _stateTimer;

    public event Action<BotController, bool> MissionCompleted;

    //public bool IsAvailable => _currentState is IdleState or WaitingState;
    //public bool IsCarryingResource => _inventory.HasItem;
    // Публичные свойства для доступа из состояний
    public BotMovementController Movement => _movement;
    public BotInventory Inventory => _inventory;
    public BaseController AssignedBase => _assignedBase;
    public Item AssignedResource => _assignedResource;
    public float CollectionDuration => _collectionDuration;
    public float StateTimer => _stateTimer;
    public bool IsAvailable => _stateController.CurrentStateType == BotStateType.Idle ||
                              _stateController.CurrentStateType == BotStateType.Waiting;

    #region FSM
    //// Базовый класс состояния
    //private abstract class BotState
    //{
    //    public abstract void Enter(BotController bot);
    //    public abstract void Update(BotController bot);
    //    public virtual void Exit(BotController bot) { }
    //}

    //// Конкретные состояния
    //private class IdleState : BotState
    //{
    //    public override void Enter(BotController bot) { }
    //    public override void Update(BotController bot) { }
    //}

    //private class WaitingState : BotState
    //{
    //    public override void Enter(BotController bot) => bot.StopMovement();
    //    public override void Update(BotController bot) { }
    //}

    //private class MovingToResourceState : BotState
    //{
    //    public override void Enter(BotController bot)
    //    {
    //        if (bot._assignedResource != null)
    //        {
    //            bot.MoveToPosition(bot._assignedResource.transform.position);
    //        }
    //    }

    //    public override void Update(BotController bot)
    //    {
    //        if (bot._assignedResource == null || bot._assignedResource.CanBeCollected == false)
    //        {
    //            bot.CompleteMission(false);
    //            return;
    //        }

    //        if (bot.HasReachedDestination())
    //        {
    //            bot.ChangeState(new CollectingState());
    //        }
    //    }
    //}

    //private class CollectingState : BotState
    //{
    //    public override void Enter(BotController bot)
    //    {
    //        bot.StopMovement();
    //        bot._stateTimer = 0f;
    //    }

    //    public override void Update(BotController bot)
    //    {
    //        bot._stateTimer += Time.deltaTime;

    //        if (bot._stateTimer >= bot._collectionDuration)
    //        {
    //            if (bot._assignedResource != null && bot._inventory.TryAddItem(bot._assignedResource))
    //            {
    //                bot._assignedResource.Collect();
    //                bot.ChangeState(new ReturningToBaseState());
    //            }
    //            else
    //            {
    //                bot.CompleteMission(false);
    //            }
    //        }
    //    }
    //}

    //private class ReturningToBaseState : BotState
    //{
    //    public override void Enter(BotController bot)
    //    {
    //        if (bot._assignedBase != null)
    //        {
    //            bot.MoveToPosition(bot._assignedBase.transform.position);
    //        }
    //    }

    //    public override void Update(BotController bot)
    //    {
    //        if (bot.HasReachedDestination())
    //        {
    //            bot._inventory.ClearInventory();
    //            bot.CompleteMission(true);
    //        }
    //    }
    //}
    #endregion

    private void Awake()
    {
        //if (TryGetComponent(out _navMeshAgent) && TryGetComponent(out _inventory))        
        //    ChangeState(new IdleState()); 
        InitializeComponents();
    }

    private void Update()
    {
        _stateController.UpdateState(this);
        //_currentState?.Update(this);
    }

    private void OnDestroy()
    {
        if (_stateController != null)
        {
            _stateController.StateChanged -= HandleStateChanged;
        }
    }

    private void InitializeComponents()
    {
        TryGetComponent(out _movement);
        TryGetComponent(out _inventory);

        _stateController = new BotStateController();
        _stateController.StateChanged += HandleStateChanged;

        ChangeState(new BotIdleState());
    }

    public void AssignBase(BaseController baseController)
    {
        _assignedBase = baseController;
        ChangeState(new BotIdleState());
        //ChangeState(new IdleState());
    }

    public void AssignResource(Item resource)
    {
        if (IsAvailable == false)
            return;

        _assignedResource = resource;
        ChangeState(new BotMovingToResourceState());
        //ChangeState(new MovingToResourceState());
    }

    public void SetWaiting()
    {
        ChangeState(new BotWaitingState());
        // ChangeState(new WaitingState());
    }

    public void ChangeState(BotState newState)
    {
        _stateController.ChangeState(newState, this);
        //_currentState?.Exit(this);
        //_currentState = newState;
        //_currentState?.Enter(this);
    }

    public void CompleteMission(bool success)
    {
        _assignedResource = null;
        ChangeState(new BotIdleState());
        //ChangeState(new IdleState());
        MissionCompleted?.Invoke(this, success);
    }

    public void MoveToPosition(Vector3 position)
    {
        _movement?.MoveToPosition(position);
        //_navMeshAgent.SetDestination(position);
    }

    public void StopMovement()
    {
        _movement?.StopMovement();
        // _navMeshAgent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        return _movement?.HasReachedDestination() ?? false;
        //return _navMeshAgent.pathPending == false && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }

    // Методы для управления таймером состояния
    public void ResetStateTimer() =>
        _stateTimer = 0f;
    public void UpdateStateTimer(float deltaTime) =>
        _stateTimer += deltaTime;

    /// <summary>
    /// Обрабатывает смену состояния (для отладки)
    /// </summary>
    private void HandleStateChanged(BotStateType newState)
    {
        Debug.Log($"🤖 {name} в состоянии: {newState}");
    }
}