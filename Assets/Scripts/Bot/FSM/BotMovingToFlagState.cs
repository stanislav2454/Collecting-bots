using UnityEngine;

public class BotMovingToFlagState : BotState
{
    private Vector3 _flagPosition;
    private BaseController _newBase;
    private float _checkTimer;

    public override BotStateType StateType => BotStateType.MovingToFlag;

    public BotMovingToFlagState(Vector3 flagPosition, BaseController newBase)
    {
        _flagPosition = flagPosition;
        _newBase = newBase;
    }

    public override void Enter(Bot bot)
    {
        bot.MoveToPosition(_flagPosition);
        _checkTimer = 0f;
        Debug.Log($"Bot moving to flag at: {_flagPosition}");
    }

    public override void Update(Bot bot)
    {
        if (CheckDestinationReached(bot, ref _checkTimer))
        {
            bot.ChangeState(new BotBuildingBaseState(_newBase, _flagPosition));
        }
    }
}