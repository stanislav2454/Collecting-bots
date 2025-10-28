using UnityEngine;

public class BotReturningToBaseState : BotState
{
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _checkTimer;

    public override BotStateType StateType => BotStateType.ReturningToBase;

    public BotReturningToBaseState(Vector3 basePosition, float baseRadius)
    {
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        bot.MoveToPosition(_basePosition);
        _checkTimer = 0f;
    }

    public override void Update(Bot bot)
    {
        if (CheckDestinationReached(bot, ref _checkTimer) || IsAtBase(bot, _basePosition, _baseRadius))
            bot.CompleteMission(true);
    }
}