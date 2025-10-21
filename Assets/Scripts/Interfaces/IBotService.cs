using UnityEngine;
using System;

public interface IBotService
{
    // Основные операции с ботами
    BotController[] GetAllBots();
    BotController GetSelectedBot();
    void SelectBot(BotController bot);
    void DeselectAllBots();
    GameObject SpawnBot(Vector3 position);
    void DespawnBot(GameObject bot);
    void ResetAllBots();

    // Статистика
    int GetActiveBotsCount();
    int GetTotalBotsCount();
    string GetBotPoolInfo();

    // События для слабой связности
    event Action<BotController> OnBotSelected;
    event Action<BotController> OnBotDeselected;
    event Action<BotController> OnBotSpawned;
    event Action<BotController> OnBotDespawned;
    event Action OnAllBotsReset;
}