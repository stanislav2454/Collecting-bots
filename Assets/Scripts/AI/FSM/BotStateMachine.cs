using System.Collections.Generic;
using UnityEngine;

public class BotStateMachine
{
    private Dictionary<BotState, BotBaseState> _states = new Dictionary<BotState, BotBaseState>();
    private BotBaseState _currentState;
    private Item _targetItem; // Храним целевой предмет

    public BotState CurrentStateType { get; private set; }
    public BotController BotController { get; private set; }

    public BotStateMachine(BotController botController)
    {
        BotController = botController;
        InitializeStates();
    }

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

    public void ChangeState(BotState newState)
    {
        _currentState?.Exit();
        _currentState = _states[newState];
        CurrentStateType = newState;
        _currentState.Enter();

        Debug.Log($"{BotController.gameObject.name} changed state to: {newState}");
    }

    public void Update() =>
        _currentState?.Update();

    public void FixedUpdate() =>
        _currentState?.FixedUpdate();

    // Методы для работы с целевым предметом
    public void SetTargetItem(Item item) =>
        _targetItem = item;

    public Item GetTargetItem() =>
        _targetItem;

    public void ClearTargetItem() =>
        _targetItem = null;
}