using UnityEngine;

public class BotBuildingBaseState : BotState
{
    private const float BuildDuration = 2f;

    private BaseController _newBase;
    private Vector3 _buildPosition;
    private float _buildTimer;

    public override BotStateType StateType => BotStateType.BuildingBase;

    public BotBuildingBaseState(BaseController newBase, Vector3 buildPosition)
    {
        _newBase = newBase;
        _buildPosition = buildPosition;
    }

    public override void Enter(Bot bot)
    {
        bot.StopMovement();
        _buildTimer = 0f;
    }

    public override void Update(Bot bot)
    {
        _buildTimer += Time.deltaTime;

        if (_buildTimer >= BuildDuration)
            CompleteBuilding(bot);
    }

    private void CompleteBuilding(Bot bot)
    {
        if (BaseGenerator.Instance != null)
            BaseGenerator.Instance.CreateNewBase(_buildPosition, bot, _newBase);
        else
            Debug.LogError("BaseGenerator instance not found!");

        _newBase?.OnBaseConstructionCompleted();

        bot.CompleteMission(true);

        Debug.Log("Base construction completed!");
    }
}