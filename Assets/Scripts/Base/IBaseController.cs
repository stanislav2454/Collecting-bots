using UnityEngine;

public interface IBaseController
{
    // Properties
    Vector3 BasePosition { get; }
    float UnloadZoneRadius { get; }
    float SpawnZoneRadius { get; }
    bool CanBuildNewBase { get; }
    BasePriority CurrentPriority { get; }
    int CollectedResources { get; }
    bool IsSelected { get; }
    bool HasActiveFlag { get; }

    // Methods
    void CollectResourceFromBot(Bot bot);
    bool TransferBotToNewBase(IBaseController newBase);
    BotManager GetBotManager();
    bool HasAvailableBotForTransfer();
    bool TrySetFlag(Vector3 worldPosition);
    void RemoveFlag();
    void SelectBase();
    void DeselectBase();
    void SetSelected(bool selected, bool notifyOthers = true);
}