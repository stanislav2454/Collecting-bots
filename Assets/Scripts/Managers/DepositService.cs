using UnityEngine;
using System;
using System.Linq;

public class DepositService : MonoBehaviour, IDepositService
{
    private DepositZone[] _depositZones = new DepositZone[0];
    private float _lastCacheTime;
    private const float CACHE_UPDATE_INTERVAL = 2f;

    private int _totalItemsDeposited = 0;
    private int _totalPointsEarned = 0;

    // События
    public event Action<DepositZone, int, int> OnDepositProcessed;
    public event Action<DepositZone> OnDepositZoneAdded;
    public event Action<DepositZone> OnDepositZoneRemoved;

    private void Start()
    {
        UpdateDepositZonesCache();
        ServiceLocator.Register<IDepositService>(this);

        Debug.Log("DepositService initialized and registered");
    }

    private void Update()
    {
        // Обновляем кэш зон сдачи с интервалом
        if (Time.time - _lastCacheTime >= CACHE_UPDATE_INTERVAL)
        {
            UpdateDepositZonesCache();
        }
    }

    private void UpdateDepositZonesCache()
    {
        _depositZones = FindObjectsOfType<DepositZone>();
        _lastCacheTime = Time.time;

        Debug.Log($"Deposit zones cache updated: {_depositZones.Length} zones");
    }

    #region IDepositService Implementation

    public DepositZone GetNearestDepositZone(Vector3 position)
    {
        if (_depositZones.Length == 0) return null;

        DepositZone nearestZone = null;
        float nearestDistance = float.MaxValue;

        foreach (var zone in _depositZones)
        {
            if (zone == null) continue;

            float distance = Vector3.Distance(position, zone.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestZone = zone;
            }
        }

        return nearestZone;
    }

    public DepositZone[] GetAllDepositZones() =>
        _depositZones;

    public bool IsPositionNearDepositZone(Vector3 position, float radius = 3f)
    {
        return _depositZones.Any(zone =>
            zone != null &&
            Vector3.Distance(position, zone.transform.position) <= radius);
    }

    public bool ProcessDeposit(BotInventory inventory, Vector3 depositPosition)
    {
        if (inventory == null || inventory.CurrentCount == 0)
            return false;

        // Находим ближайшую зону для обработки депозита
        var nearestZone = GetNearestDepositZone(depositPosition);
        if (nearestZone == null)
        {
            Debug.LogWarning("No deposit zone found for processing deposit");
            return false;
        }

        // Обрабатываем депозит через зону
        int itemsToDeposit = inventory.CurrentCount;
        int pointsEarned = itemsToDeposit * 10; // Базовая стоимость

        _totalItemsDeposited += itemsToDeposit;
        _totalPointsEarned += pointsEarned;

        // Очищаем инвентарь
        inventory.ClearInventory();

        // Вызываем событие
        OnDepositProcessed?.Invoke(nearestZone, itemsToDeposit, pointsEarned);

        Debug.Log($"Deposit processed: {itemsToDeposit} items, {pointsEarned} points");

        return true;
    }

    public bool CanProcessDeposit(Vector3 position)
    {
        return IsPositionNearDepositZone(position, 3f);
    }

    public int GetTotalDepositZonesCount() =>
        _depositZones.Length;

    public int GetTotalItemsDeposited() =>
        _totalItemsDeposited;

    public int GetTotalPointsEarned() =>
        _totalPointsEarned;

    public string GetDepositZoneInfo()
    {
        return $"Deposit Zones: {_depositZones.Length}, Items Deposited: {_totalItemsDeposited}, Points: {_totalPointsEarned}";
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        OnDepositProcessed = null;
        OnDepositZoneAdded = null;
        OnDepositZoneRemoved = null;
    }

    #endregion
}