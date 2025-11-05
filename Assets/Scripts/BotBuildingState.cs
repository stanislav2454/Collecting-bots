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
        //_constructionTimer += Time.deltaTime;

        //// Визуализация прогресса строительства
        //if (Mathf.FloorToInt(_constructionTimer) != Mathf.FloorToInt(_constructionTimer - Time.deltaTime))
        //{
        //    Debug.Log($"[BotBuilding] Construction progress: {_constructionTimer:F1}/{_constructionTime}");
        //}

        //if (_constructionTimer >= _constructionTime)
        //{
        //    Debug.Log($"[BotBuilding] Construction completed!");
        //    // Бот завершил строительство, состояние изменится в ConstructionManager
        //}
    }
}