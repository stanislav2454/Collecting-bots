using UnityEngine;

public class BotMovingToResourceState : BotState
{
    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _checkTimer;

    public override BotStateType StateType => BotStateType.MovingToResource;

    public BotMovingToResourceState(Item resource, Vector3 basePosition, float baseRadius)
    {
        _targetResource = resource;
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        if (IsResourceValid(_targetResource))
        {
            bot.MoveToPosition(_targetResource.transform.position);
            _checkTimer = 0f;
        }
        else
        {
            bot.CompleteMission(false);
        }
    }

    public override void Update(Bot bot)
    {
        if (CheckDestinationReached(bot, ref _checkTimer))
        {
            if (IsResourceValid(_targetResource))
                bot.ChangeState(new BotCollectingState(_targetResource, _basePosition, _baseRadius));
            else
                bot.CompleteMission(false);
        }
    }
}
