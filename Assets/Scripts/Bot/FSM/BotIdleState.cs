public class BotIdleState : BotState
{
    public override BotStateType StateType => BotStateType.Idle;

    public override void Enter(Bot bot)
    {
        bot.StopMovement();
    }

    public override void Update(Bot bot) { }
}
