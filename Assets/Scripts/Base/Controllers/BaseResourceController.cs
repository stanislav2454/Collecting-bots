using System;
using UnityEngine;

public class BaseResourceController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ItemCounter _itemCounter;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private BasePriorityController _priorityController;

    public int CollectedResources => _itemCounter.CurrentValue;// зачем дубляж ?
    public int AvailableResources => _itemCounter.CurrentValue;// зачем дубляж ?

    //// События для уведомления других систем
    //public event Action<int> ResourcesChanged; // количество ресурсов
    //public event Action<int> ResourcesAdded;   // когда добавили
    //public event Action<int> ResourcesSpent;   // когда потратили

    private void Start()
    {
        // Подписываемся на события ItemCounter для проброса наружу
        if (_itemCounter != null)
        {
            _itemCounter.Changed += OnCounterChanged;
        }
    }

    private void OnValidate()
    {
        if (_itemCounter == null)
            Debug.LogError("ItemCounter not assigned in BaseResourceController!");

        if (_priorityController == null)
            Debug.LogWarning("BasePriorityController not assigned in BaseResourceController!");
    }

    private void OnDestroy()
    {
        if (_itemCounter != null)
        {
            _itemCounter.Changed -= OnCounterChanged;
        }

        //ResourcesChanged = null;
        //ResourcesAdded = null;
        //ResourcesSpent = null;
    }

    public bool CanAfford(int cost) =>
        _itemCounter.CanAfford(cost);

    public void CollectResourceFromBot(Bot bot)
    {
        if (bot.IsCarryingResource == false)
            return;

        var item = bot.Inventory.CarriedItem;

        if (item != null)
        {
            _itemCounter.Add(item.Value);
            _priorityController?.OnResourcesChanged();
           // ResourcesAdded?.Invoke(item.Value);
        }

        bot.Inventory.ClearInventory();
        _itemSpawner?.ReturnItemToPool(item);
    }

    public void AddResource(int amount)
    {
        if (amount > 0)
        {
            _itemCounter.Add(amount);
            _priorityController?.OnResourcesChanged();
          //  ResourcesAdded?.Invoke(amount);
        }
    }

    public bool TrySpendResources(int amount)
    {
        if (_itemCounter.TrySubtract(amount))
        {
            _priorityController?.OnResourcesChanged();
            //ResourcesAdded?.Invoke(amount);
            return true;
        }

        return false;
    }

    public void ResetResources()
    {
        _itemCounter.Reset();
      //  ResourcesChanged?.Invoke(0);
    }

    private void OnCounterChanged()
    {
       // ResourcesChanged?.Invoke(_itemCounter.CurrentValue);
    }
}