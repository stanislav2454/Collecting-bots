using UnityEngine;

public interface IBaseController
{
    public Vector3 BasePosition { get; }
    public float UnloadZoneRadius { get; }
    public float SpawnZoneRadius { get; }
    public bool CanBuildNewBase { get; }
    public BasePriority CurrentPriority { get; }
    public int CollectedResources { get; }
    public bool IsSelected { get; }
    public bool HasActiveFlag { get; }

    public void CollectResourceFromBot(Bot bot);
    public bool TransferBotToNewBase(IBaseController newBase);
    public BotManager GetBotManager();
    public bool HasAvailableBotForTransfer();
    public bool TrySetFlag(Vector3 worldPosition);
    public void RemoveFlag();
    public void SelectBase();
    public void DeselectBase();
    public void SetSelected(bool selected, bool notifyOthers = true);
}