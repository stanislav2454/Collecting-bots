public abstract class BotBaseState
{
    protected BotStateMachine StateMachine;
    protected BotController BotController;

    public BotBaseState(BotStateMachine stateMachine)
    {
        this.StateMachine = stateMachine;
        this.BotController = stateMachine.BotController;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();

    protected void ChangeState(BotState newState)
    {
        StateMachine.ChangeState(newState);
    }
}
public enum BotState
{
    Idle,
    Search,
    MoveToItem,
    Collect,
    MoveToDeposit,
    Deposit,
    Wait
}