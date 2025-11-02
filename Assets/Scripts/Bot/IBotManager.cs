using UnityEngine;

public interface IBotManager
{
    public Vector3 BasePosition { get; }
    public float UnloadZoneRadius { get; }
    public int BotCount { get; }
    public int AvailableBotsCount { get; }

    public void SetBaseController(IBaseController baseController);
    public void SetResourceManager(ResourceManager resourceManager);
    public bool TransferBotToNewBase(Bot bot, IBaseController newBase);
    public Bot GetAvailableBotForTransfer();
    public void AddExistingBot(Bot bot);
    public void AssignBotToResource(Item resource);
    public void CreateNewBot();
    public void CompleteAssignment(Bot bot, bool success);
    public bool IsBotAssigned(Bot bot);
    public bool IsResourceAssigned(Item resource);
}