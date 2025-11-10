public class BotBuildingState : BotState
{
    private float _constructionTime;

    public override BotStateType StateType => BotStateType.Building;

    public BotBuildingState(float constructionTime)
    {
        _constructionTime = constructionTime;
    }

    public override void Enter(Bot bot)
    {
        bot.StopMovement();
    }

    public override void Update(Bot bot)
    {
    }
}