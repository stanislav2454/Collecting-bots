//using UnityEngine;
//using System;
//using System.Linq;

//public class DepositService : MonoBehaviour, IDepositService
//{//после рефакторинга УДАЛИТЬ!
//    private DepositZone[] _depositZones = new DepositZone[0];
//    private float _lastCacheTime;
//    private const float CACHE_UPDATE_INTERVAL = 2f;//todo to Pascal case

//    private int _totalItemsDeposited = 0;
//    private int _totalPointsEarned = 0;

//    public event Action<DepositZone, int, int> DepositProcessed;
//    public event Action<DepositZone> DepositZoneAdded;
//    public event Action<DepositZone> DepositZoneRemoved;

//    private void Start()
//    {
//        UpdateDepositZonesCache();
//        ServiceLocator.Register<IDepositService>(this);
//    }

//    private void Update()
//    {
//        if (Time.time - _lastCacheTime >= CACHE_UPDATE_INTERVAL)
//            UpdateDepositZonesCache();
//    }

//    public DepositZone GetNearestDepositZone(Vector3 position)
//    {
//        if (_depositZones.Length == 0)
//            return null;

//        DepositZone nearestZone = null;
//        float nearestDistance = float.MaxValue;

//        foreach (var zone in _depositZones)
//        {
//            if (zone == null)
//                continue;

//            float distance = Vector3.Distance(position, zone.transform.position);// Vector3.Distance - ресурсозатратно => переделать

//            if (distance < nearestDistance)
//            {
//                nearestDistance = distance;
//                nearestZone = zone;
//            }
//        }

//        return nearestZone;
//    }

//    public DepositZone[] GetAllDepositZones() =>
//        _depositZones;

//    public bool IsPositionNearDepositZone(Vector3 position, float radius = 3f)
//    {
//        return _depositZones.Any(zone =>
//            zone != null && Vector3.Distance(position, zone.transform.position) <= radius);// Vector3.Distance - ресурсозатратно => переделать
//    }

//    public bool ProcessDeposit(BotInventory inventory, Vector3 depositPosition)
//    {
//        if (inventory == null || inventory.CurrentCount == 0)
//            return false;

//        var nearestZone = GetNearestDepositZone(depositPosition);

//        if (nearestZone == null)
//            return false;

//        int itemsToDeposit = inventory.CurrentCount;
//        int pointsEarned = itemsToDeposit * 10;

//        _totalItemsDeposited += itemsToDeposit;
//        _totalPointsEarned += pointsEarned;

//        inventory.ClearInventory();

//        DepositProcessed?.Invoke(nearestZone, itemsToDeposit, pointsEarned);

//        return true;
//    }

//    public bool CanProcessDeposit(Vector3 position) =>
//         IsPositionNearDepositZone(position, 3f);

//    public int GetTotalDepositZonesCount() =>
//        _depositZones.Length;

//    public int GetTotalItemsDeposited() =>
//        _totalItemsDeposited;

//    public int GetTotalPointsEarned() =>
//        _totalPointsEarned;

//    public string GetDepositZoneInfo() =>
//         $"Deposit Zones: {_depositZones.Length}, Items Deposited: {_totalItemsDeposited}" +
//        $", Points: {_totalPointsEarned}";

//    private void UpdateDepositZonesCache()
//    {
//        _depositZones = FindObjectsOfType<DepositZone>();
//        _lastCacheTime = Time.time;
//    }

//    private void OnDestroy()
//    {
//        DepositProcessed = null;
//        DepositZoneAdded = null;
//        DepositZoneRemoved = null;
//    }
//}