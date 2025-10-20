using System.Collections.Generic;
using UnityEngine;

public class BotStateMachine
{
    private Dictionary<BotState, BotBaseState> states = new Dictionary<BotState, BotBaseState>();
    private BotBaseState currentState;

    public BotState CurrentStateType { get; private set; }
    public BotController BotController { get; private set; }

    public BotStateMachine(BotController botController)
    {
        BotController = botController;
        InitializeStates();
    }

    private void InitializeStates()
    {
        states[BotState.Idle] = new BotIdleState(this);
        states[BotState.Search] = new BotSearchState(this);
        states[BotState.MoveToItem] = new BotMoveToItemState(this);
        states[BotState.Collect] = new BotCollectState(this);
        states[BotState.MoveToDeposit] = new BotMoveToDepositState(this);
        states[BotState.Deposit] = new BotDepositState(this);
        states[BotState.Wait] = new BotWaitState(this);

        ChangeState(BotState.Idle);
    }

    public void ChangeState(BotState newState)
    {
        currentState?.Exit();

        currentState = states[newState];
        CurrentStateType = newState;
        currentState.Enter();

       // Debug.Log($"{BotController.gameObject.name} changed state to: {newState}");
    }

    public void Update()=>
        currentState?.Update();

    public void FixedUpdate()=>
        currentState?.FixedUpdate();
}