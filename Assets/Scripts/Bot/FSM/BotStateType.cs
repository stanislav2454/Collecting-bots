public enum BotStateType
{
    Idle,
    MovingToResource,
    Collecting,
    ReturningToBase,
    MovingToFlag,       // Новое состояние - движение к флагу
    BuildingBase        // Новое состояние - строительство базы
}
