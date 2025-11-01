using UnityEngine;

public class BotBuildingBaseState : BotState
{
    private BaseController _newBase;
    private Vector3 _buildPosition;
    private float _buildTimer;
    private const float BUILD_DURATION = 2f; // Длительность строительства

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
        Debug.Log($"Bot started building base at: {_buildPosition}");
    }

    public override void Update(Bot bot)
    {
        _buildTimer += Time.deltaTime;

        if (_buildTimer >= BUILD_DURATION)
        {
            CompleteBuilding(bot);
        }
    }

    private void CompleteBuilding(Bot bot)
    {
        //// Создаем новую базу
        //CreateNewBase();

        // Создаем новую базу через генератор
        if (BaseGenerator.Instance != null)
            BaseGenerator.Instance.CreateNewBase(_buildPosition, bot, _newBase);
        else
            Debug.LogError("BaseGenerator instance not found!");

        // Уведомляем старую базу о завершении строительства
        _newBase?.OnBaseConstructionCompleted();

        // Бот завершает миссию (теперь он принадлежит новой базе)
        bot.CompleteMission(true);

        Debug.Log("Base construction completed!");
    }

    private void CreateNewBase()
    {
        // TODO: Реализовать создание новой базы через BaseGenerator
        // Пока просто логируем
        Debug.Log($"New base created at: {_buildPosition}");
    }
}