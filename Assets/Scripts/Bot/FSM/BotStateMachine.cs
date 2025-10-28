using System;

public class BotStateMachine
{
    private BotState _currentState;

    public event Action StateChanged;

    public BotStateType CurrentStateType => _currentState?.StateType ?? BotStateType.Idle;

    public void ChangeState(BotState newState, Bot bot)
    {
        _currentState?.Exit(bot);
        _currentState = newState;
        _currentState.Enter(bot);
        StateChanged?.Invoke();
    }

    public void UpdateState(Bot bot)
    {
        _currentState?.Update(bot);
    }
}
