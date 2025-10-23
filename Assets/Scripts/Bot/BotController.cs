using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    [SerializeField] private float _collectionDuration = 1f;

    private BotMovementController _movement;
    private BotInventory _inventory;
    private BotStateController _stateController;

    private BaseController _assignedBase;
    private Item _assignedResource;

    public event Action<BotController, bool> MissionCompleted;

    public BotInventory Inventory => _inventory;
    public BaseController AssignedBase => _assignedBase;
    public Item AssignedResource => _assignedResource;
    public float CollectionDuration => _collectionDuration;
    public bool IsAvailable => _stateController.CurrentStateType == BotStateType.Idle ||
                              _stateController.CurrentStateType == BotStateType.Waiting;
    public bool IsCarryingResource => _inventory != null && _inventory.HasItem;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        _stateController.UpdateState(this);
    }

    public void AssignBase(BaseController baseController)
    {
        _assignedBase = baseController;
        ChangeState(new BotIdleState());
    }

    public void AssignResource(Item resource)
    {
        if (IsAvailable == false)
            return;

        _assignedResource = resource;
        ChangeState(new BotMovingToResourceState());
    }

    public void SetWaiting() =>
        ChangeState(new BotWaitingState());

    public void ChangeState(BotState newState) =>
        _stateController.ChangeState(newState, this);

    public void CompleteMission(bool success)
    {
        if (success)
            _inventory.ClearInventory(this);
        else
            _inventory.ReleaseItem();

        _assignedResource = null;
        ChangeState(new BotIdleState());
        MissionCompleted?.Invoke(this, success);
    }

    public void MoveToPosition(Vector3 position) =>
        _movement?.MoveToPosition(position);

    public void StopMovement() =>
        _movement?.StopMovement();

    public bool HasReachedDestination() =>
         _movement?.HasReachedDestination() ?? false;

    private void InitializeComponents()
    {
        TryGetComponent(out _movement);
        TryGetComponent(out _inventory);

        _stateController = new BotStateController();
        ChangeState(new BotIdleState());
    }
}

public static class BotConstants
{
    // Item positioning
    public const float ItemYOffset = 0.5f;
    public const float GroundYPosition = 0f;
    public const float BotCarryHeight = 1.5f;
    public const float Divider = 2f;

    // Bot movement
    public const float BotSpawnRadius = 3f;
    public const float BotAvoidanceRadius = 2f;
    public const float BotTooCloseDistance = 1f;
    public const float BotTemporaryStopDuration = 0.5f;

    // Timing
    public const float DefaultCollectionDuration = 1f;
}