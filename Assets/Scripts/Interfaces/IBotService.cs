using UnityEngine;
using System;

public interface IBotService
{
    public event Action<BotController> BotSelected;
    public event Action<BotController> BotDeselected;
    public event Action<BotController> BotSpawned;
    public event Action<BotController> BotDespawned;
    public event Action AllBotsReset;

    public BotController[] GetAllBots();
    public BotController GetSelectedBot();
    public void SelectBot(BotController bot);
    public void DeselectAllBots();
    public GameObject SpawnBot(Vector3 position);
    public void DespawnBot(GameObject bot);
    public void ResetAllBots();
    public int GetActiveBotsCount();
    public int GetTotalBotsCount();
    public string GetBotPoolInfo();
}