using UnityEngine;

public interface IBotManager
{
    // Properties
    Vector3 BasePosition { get; }
    float UnloadZoneRadius { get; }
    int BotCount { get; }
    int AvailableBotsCount { get; }

    // Methods
    void SetBaseController(IBaseController baseController);
    void SetResourceManager(ResourceManager resourceManager);
    bool TransferBotToNewBase(Bot bot, IBaseController newBase);
    Bot GetAvailableBotForTransfer();
    void AddExistingBot(Bot bot);
    void AssignBotToResource(Item resource);
    void CreateNewBot();
    void CompleteAssignment(Bot bot, bool success);
    bool IsBotAssigned(Bot bot);
    bool IsResourceAssigned(Item resource);
}