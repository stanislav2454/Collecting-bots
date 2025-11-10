using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BotInventory), typeof(BotVisualizer))]
public class Bot : MonoBehaviour
{
    [Header("State Visualization")]
    [SerializeField]
    private List<StateVisualData> _stateVisualData = new List<StateVisualData>
    {
        new StateVisualData { StateType = BotStateType.Idle, Color = Color.gray, IconName = "sv_icon_dot0_pix16_gizmo" },
        new StateVisualData { StateType = BotStateType.MovingToResource, Color = Color.yellow, IconName = "sv_icon_dot7_pix16_gizmo" },
        new StateVisualData { StateType = BotStateType.Collecting, Color = Color.magenta, IconName = "sv_icon_dot9_pix16_gizmo" },
        new StateVisualData { StateType = BotStateType.ReturningToBase, Color = Color.cyan, IconName = "sv_icon_dot10_pix16_gizmo" }
    };

    private BotMovementController _movement;
    private BotStateMachine _stateMachine;
    private Dictionary<BotStateType, StateVisualData> _stateVisualMap;
    private BotVisualizer _visualizer;

    public event Action<Bot, bool> MissionCompleted;

    [field: Header("Bot Settings")]
    [field: SerializeField] public float CollectionDuration { get; private set; } = 1f;
    public BotInventory Inventory { get; private set; }
    public Item AssignedResource { get; private set; }
    public bool IsAvailable => _stateMachine.CurrentStateType == BotStateType.Idle;
    public bool IsCarryingResource => Inventory != null && Inventory.HasItem;
    public BotStateType CurrentStateType => _stateMachine.CurrentStateType;

    private void Awake()
    {
        InitializeComponents();
        InitializeStateVisualMap();
        UpdateVisualizationCache();

        _stateMachine.StateChanged += UpdateVisualizationCache;
    }

    private void Update()
    {
        _stateMachine.UpdateState(this);
    }

    private void OnDestroy()
    {
        if (MissionCompleted != null)
            foreach (var handler in MissionCompleted.GetInvocationList())
                MissionCompleted -= (Action<Bot, bool>)handler;

        _stateMachine.StateChanged -= UpdateVisualizationCache;
    }

    public void AssignResource(Item resource, Vector3 basePosition, float baseRadius)
    {
        if (IsAvailable == false)
            return;

        AssignedResource = resource;
        ChangeState(new BotMovingToResourceState(resource, basePosition, baseRadius));
    }

    public void SetWaiting() =>
        ChangeState(new BotIdleState());

    public void ChangeState(BotState newState) =>
        _stateMachine.ChangeState(newState, this);

    public void CompleteMission(bool success)
    {
        bool shouldRespawnResource = success == false;
        Inventory.ClearInventory(prepareForRespawn: shouldRespawnResource);
        AssignedResource = null;
        ChangeState(new BotIdleState());
        MissionCompleted?.Invoke(this, success);
    }

    public void MoveToPosition(Vector3 position) =>
        _movement?.MoveToPosition(position);

    public void StopMovement() =>
        _movement?.StopMovement();

    public bool HasReachedDestination() =>
         _movement?.HasReachedDestination() ?? false;


    public void ReassignToNewManager(BotController newManager, BotController oldManager = null)
    {
        try
        {
            if (oldManager != null)
                MissionCompleted -= oldManager.HandleBotMissionCompleted;

            MissionCompleted += newManager.HandleBotMissionCompleted;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Bot] Failed to reassign to new manager: {e.Message}");
        }
    }

    private void InitializeComponents()
    {
        TryGetComponent(out _movement);

        BotInventory inventory;
        if (TryGetComponent(out inventory))
            Inventory = inventory;

        TryGetComponent(out _visualizer);
        _visualizer.Initialize(this, _movement);

        _stateMachine = new BotStateMachine();
        ChangeState(new BotIdleState());
    }

    private void InitializeStateVisualMap()
    {
        _stateVisualMap = new Dictionary<BotStateType, StateVisualData>();

        foreach (var data in _stateVisualData)
            _stateVisualMap[data.StateType] = data;

        if (_stateVisualMap.ContainsKey(BotStateType.MovingToConstruction) == false)
            _stateVisualMap[BotStateType.MovingToConstruction] = new StateVisualData
            {
                StateType = BotStateType.MovingToConstruction,
                Color = Color.blue,
                IconName = "sv_icon_dot8_pix16_gizmo"
            };

        if (_stateVisualMap.ContainsKey(BotStateType.Building) == false)
            _stateVisualMap[BotStateType.Building] = new StateVisualData
            {
                StateType = BotStateType.Building,
                Color = Color.red,
                IconName = "sv_icon_dot6_pix16_gizmo"
            };
    }

    private void UpdateVisualizationCache()
    {
        if (_visualizer == null)
            return;

        if (_stateVisualMap.TryGetValue(CurrentStateType, out StateVisualData data))
            _visualizer.UpdateVisualization(data.Color, data.IconName);
        else
            _visualizer.UpdateVisualization(Color.white, "sv_icon_dot0_pix16_gizmo");
    }
}

[Serializable]
public class StateVisualData
{
    public BotStateType StateType;
    public Color Color;
    public string IconName;
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