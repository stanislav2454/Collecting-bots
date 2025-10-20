public abstract class BotBaseState
{
    protected BotStateMachine stateMachine;
    protected BotController botController;

    public BotBaseState(BotStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.botController = stateMachine.BotController;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();

    protected void ChangeState(BotState newState)
    {
        stateMachine.ChangeState(newState);
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