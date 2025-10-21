using UnityEngine;
using System;

public interface IDepositService
{
    public event Action<DepositZone, int, int> DepositProcessed;
    public event Action<DepositZone> DepositZoneAdded;
    public event Action<DepositZone> DepositZoneRemoved;

    public DepositZone GetNearestDepositZone(Vector3 position);
    public DepositZone[] GetAllDepositZones();
    public bool IsPositionNearDepositZone(Vector3 position, float radius = 3f);
    public bool ProcessDeposit(BotInventory inventory, Vector3 depositPosition);
    public bool CanProcessDeposit(Vector3 position);
    public int GetTotalDepositZonesCount();
    public int GetTotalItemsDeposited();
    public int GetTotalPointsEarned();
    public string GetDepositZoneInfo();
}