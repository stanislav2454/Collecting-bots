public interface IGameDependencies
{
    ResourceManager ResourceManager { get; }
    ItemSpawner ItemSpawner { get; }
    BaseGenerator BaseGenerator { get; }
    BaseSelectionManager BaseSelectionManager { get; }
}