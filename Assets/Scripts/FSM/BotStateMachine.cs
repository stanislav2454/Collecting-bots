using System.Collections.Generic;
using UnityEngine;

public class BotStateMachine
{
    private Dictionary<BotState, BotBaseState> _states = new Dictionary<BotState, BotBaseState>();
    private BotBaseState _currentState;
    private Item _targetItem;

    public BotState CurrentStateType { get; private set; }
    public BotController BotController { get; private set; }

    public BotStateMachine(BotController botController)
    {
        BotController = botController;
        InitializeStates();
    }

    public void ChangeState(BotState newState)
    {
        _currentState?.Exit();
        _currentState = _states[newState];
        CurrentStateType = newState;
        _currentState.Enter();
    }

    public void Update() =>
        _currentState?.Update();

    public void FixedUpdate() =>
        _currentState?.FixedUpdate();

    public void SetTargetItem(Item item) =>// зачем нужен ? если не используется - удалить !
        _targetItem = item;

    public Item GetTargetItem() =>// зачем нужен ? если не используется - удалить !
        _targetItem;

    public void ClearTargetItem() =>// зачем нужен ? если не используется - удалить !
        _targetItem = null;

    private void InitializeStates()
    {
        _states[BotState.Idle] = new BotIdleState(this);
        _states[BotState.Search] = new BotSearchState(this);
        _states[BotState.MoveToItem] = new BotMoveToItemState(this);
        _states[BotState.Collect] = new BotCollectState(this);
        _states[BotState.MoveToDeposit] = new BotMoveToDepositState(this);
        _states[BotState.Deposit] = new BotDepositState(this);
        _states[BotState.Wait] = new BotWaitState(this);

        ChangeState(BotState.Idle);
    }
}