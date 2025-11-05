using UnityEngine;

public class BotMovingToConstructionState : BotState
{
    private Vector3 _constructionSitePosition;

    public override BotStateType StateType => BotStateType.MovingToConstruction;

    public BotMovingToConstructionState(Vector3 constructionSitePosition)
    {
        _constructionSitePosition = constructionSitePosition;
    }

    public override void Enter(Bot bot)
    {
        bot.MoveToPosition(_constructionSitePosition);
    }

    public override void Update(Bot bot)
    {

    }
}