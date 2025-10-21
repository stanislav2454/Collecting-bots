using UnityEngine;
using System;

public interface IDepositService
{
    // Основные операции с зонами сдачи
    DepositZone GetNearestDepositZone(Vector3 position);
    DepositZone[] GetAllDepositZones();
    bool IsPositionNearDepositZone(Vector3 position, float radius = 3f);

    // Обработка сдачи предметов
    bool ProcessDeposit(BotInventory inventory, Vector3 depositPosition);
    bool CanProcessDeposit(Vector3 position);

    // Статистика
    int GetTotalDepositZonesCount();
    int GetTotalItemsDeposited();
    int GetTotalPointsEarned();
    string GetDepositZoneInfo();

    // События
    event Action<DepositZone, int, int> OnDepositProcessed; // zone, itemCount, points
    event Action<DepositZone> OnDepositZoneAdded;
    event Action<DepositZone> OnDepositZoneRemoved;
}