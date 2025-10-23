using System;
using UnityEngine;

public class BotStateController
{
    public event Action<BotStateType> StateChanged;

    private BotState _currentState;
    private BotStateType _currentStateType;

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

    public override void Enter(BotController bot) => bot.StopMovement();
    public override void Update(BotController bot) { }
}
public class BotMovingToResourceState : BotState
{
    public override BotStateType StateType => BotStateType.MovingToResource;

    public override void Enter(BotController bot)
    {
        if (bot.AssignedResource != null)
        {
            bot.MoveToPosition(bot.AssignedResource.transform.position);
        }
    }

    public override void Update(BotController bot)
    {
        if (bot.AssignedResource == null || !bot.AssignedResource.CanBeCollected)
        {
            bot.CompleteMission(false);
            return;
        }

        if (bot.HasReachedDestination())
        {
            bot.ChangeState(new BotCollectingState());
        }
    }
}
public class BotCollectingState : BotState
{
    public override BotStateType StateType => BotStateType.Collecting;

    public override void Enter(BotController bot)
    {
        bot.StopMovement();
        bot.ResetStateTimer();
    }

    public override void Update(BotController bot)
    {
        bot.UpdateStateTimer(Time.deltaTime);

        if (bot.StateTimer >= bot.CollectionDuration)
        {
            if (bot.AssignedResource != null && bot.Inventory.TryAddItem(bot.AssignedResource))
            {
                bot.AssignedResource.Collect();
                bot.ChangeState(new BotReturningToBaseState());
            }
            else
            { // Не удалось собрать - освобождаем ресурс и завершаем миссию неудачей
                bot.Inventory.ReleaseItem();
                bot.CompleteMission(false);
            }
        }
    }
}
public class BotReturningToBaseState : BotState
{
    public override BotStateType StateType => BotStateType.ReturningToBase;

    public override void Enter(BotController bot)
    {
        if (bot.AssignedBase != null)        
            bot.MoveToPosition(bot.AssignedBase.transform.position);        
    }

    public override void Update(BotController bot)
    {
        if (bot.HasReachedDestination())
        {
            bot.Inventory.ClearInventory();
            bot.CompleteMission(true);
        }
    }
}