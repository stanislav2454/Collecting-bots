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
        Debug.Log($"[BotMovingToConstruction] Bot {bot.name} moving to construction site at {_constructionSitePosition}");
        bot.MoveToPosition(_constructionSitePosition);
    }

    public override void Update(Bot bot)
    {
        //if (bot.HasReachedDestination())
        //{
        //    Debug.Log($"[BotMovingToConstruction] Bot arrived at construction site");
        //    // Состояние изменится в ConstructionManager когда бот достигнет места
        //}
    }
}