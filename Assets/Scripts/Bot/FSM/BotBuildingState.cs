using UnityEngine;

public class BotBuildingState : BotState
{
    private float _constructionTime;
    private float _constructionTimer;

    public override BotStateType StateType => BotStateType.Building;

    public BotBuildingState( float constructionTime)
    {
        _constructionTime = constructionTime;
    }

    public override void Enter(Bot bot)
    {
        bot.StopMovement(); 
        _constructionTimer = 0f;
    }

    public override void Update(Bot bot)
    {
    }
}