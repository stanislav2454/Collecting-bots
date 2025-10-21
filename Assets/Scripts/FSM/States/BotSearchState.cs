using UnityEngine;

public class BotSearchState : BotBaseState
{
    private float _searchCooldown = 0f;
    private float _searchCooldownTime = 2f;
    private int _failedSearchAttempts = 0;
    private const int MaxFailedAttempts = 3;

    public BotSearchState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        _searchCooldown = 0f;
        _failedSearchAttempts = 0;
    }

    public override void Update()
    {
        _searchCooldown -= Time.deltaTime;

        if (_searchCooldown <= 0f)
        {
            SearchForItem();
            _searchCooldown = _searchCooldownTime;
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        if (BotController.TargetItem != null)
            ReleaseItemReservation(BotController.TargetItem);
    }

    private void SearchForItem()
    {
        if (BotController.BotInventory.IsFull)
        {
            ChangeState(BotState.MoveToDeposit);
            return;
        }

        if (ServiceLocator.TryGet<IItemService>(out var itemService))
        {
            Item availableItem = itemService.FindBestItemForBot(
                BotController.transform.position,
                20f,
                BotController);

            if (availableItem != null && itemService.TryReserveItem(availableItem, BotController))
            {
                BotController.SetTargetItem(availableItem);
                _failedSearchAttempts = 0;
                ChangeState(BotState.MoveToItem);
            }
            else
            {
                HandleSearchFailure();
            }
        }
        else
        {
            SearchForItemFallback();
        }
    }

    private void HandleSearchFailure()
    {
        _failedSearchAttempts++;

        if (_failedSearchAttempts >= MaxFailedAttempts)
            ChangeState(BotState.Wait);
    }

    private void SearchForItemFallback()
    {
        Item[] allItems = Object.FindObjectsOfType<Item>();
        Item closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (var item in allItems)
        {
            if (item == null || !item.CanBeCollected)
                continue;

            float distance = Vector3.Distance(BotController.transform.position, item.transform.position);// Vector3.Distance - ресурсозатратно => переделать

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            BotController.SetTargetItem(closestItem);
            ChangeState(BotState.MoveToItem);
        }
        else
        {
            _failedSearchAttempts++;

            if (_failedSearchAttempts >= MaxFailedAttempts)
            {
                ChangeState(BotState.Wait);
            }
        }
    }

    private void ReleaseItemReservation(Item item)
    {
        if (ServiceLocator.TryGet<IItemService>(out var itemService))
            itemService.ReleaseItem(item);
    }
}