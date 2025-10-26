using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class Bot : MonoBehaviour
{
    [field: Header("Bot Settings")]
    // [SerializeField] private float CollectionDuration = 1f;
    [field: SerializeField] public float CollectionDuration { get; private set; } = 1f;

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
    private BotStateController _stateController;
    private Dictionary<BotStateType, StateVisualData> _stateVisualMap;

    private Color _currentStateColor;
    private string _currentStateIcon;

    public event Action<Bot, bool> MissionCompleted;

    public BotInventory Inventory { get; private set; }
    public Item AssignedResource { get; private set; }
    //  public float CollectionDuration => CollectionDuration;
    public bool IsAvailable => _stateController.CurrentStateType == BotStateType.Idle;
    public bool IsCarryingResource => Inventory != null && Inventory.HasItem;
    public BotStateType CurrentStateType => _stateController.CurrentStateType;

    private void Awake()
    {
        InitializeComponents();
        InitializeStateVisualMap();
        UpdateVisualizationCache();

        _stateController.StateChanged += UpdateVisualizationCache;
    }

    private void Update()
    {
        _stateController.UpdateState(this);
    }

    private void OnDestroy()
    {
        if (MissionCompleted != null)
            foreach (var handler in MissionCompleted.GetInvocationList())
                MissionCompleted -= (Action<Bot, bool>)handler;

        _stateController.StateChanged -= UpdateVisualizationCache;
    }

    public void AssignResource(Item resource, Vector3 basePosition, float baseRadius)
    {
        if (IsAvailable == false)
            return;

        AssignedResource = resource;
        ChangeState(new BotMovingToResourceState(resource, basePosition, baseRadius));
    }

    public void SetWaiting()
    {
        ChangeState(new BotIdleState());
    }

    public void ChangeState(BotState newState)
    {
        _stateController.ChangeState(newState, this);
    }

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

    private void InitializeComponents()
    {
        TryGetComponent(out _movement);

        BotInventory inventory;
        if (TryGetComponent(out inventory))
            Inventory = inventory;

        _stateController = new BotStateController();
        ChangeState(new BotIdleState());
    }

    private void InitializeStateVisualMap()
    {
        _stateVisualMap = new Dictionary<BotStateType, StateVisualData>();

        foreach (var data in _stateVisualData)
            _stateVisualMap[data.StateType] = data;
    }

    private void UpdateVisualizationCache()
    {
        if (_stateVisualMap.TryGetValue(CurrentStateType, out StateVisualData data))
        {
            _currentStateColor = data.Color;
            _currentStateIcon = data.IconName;
        }
        else
        {
            _currentStateColor = Color.white;
            _currentStateIcon = "sv_icon_dot0_pix16_gizmo";
        }
    }

    private Color GetStateColor()
    {
        if (_stateVisualMap.TryGetValue(CurrentStateType, out StateVisualData data))
            return data.Color;
        return Color.white;
    }

    private string GetStateIcon()
    {
        if (_stateVisualMap.TryGetValue(CurrentStateType, out StateVisualData data))
            return data.IconName;
        return "sv_icon_dot0_pix16_gizmo";
    }

    // ВИЗУАЛИЗАЦИЯ В SCENE VIEW
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        Color stateColor = GetStateColor();//
        Gizmos.color = stateColor;

        Vector3 iconPosition = transform.position + Vector3.up * 3f;
        Gizmos.DrawIcon(iconPosition, _currentStateIcon, true);

        if (_movement != null && _movement.IsMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, _movement.transform.position + Vector3.up);
        }

        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 0.5f);

        if (IsCarryingResource)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.2f, 0.3f);
        }
    }

    // ВИЗУАЛИЗАЦИЯ ДЛЯ GAME VIEW 
    private void OnGUI()
    {
        if (Application.isPlaying == false)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

        if (screenPos.z <= 0)
            return;

        string stateText = $"{CurrentStateType}";

        if (IsCarryingResource)
            stateText += " 📦";

        if (AssignedResource != null)
            stateText += $"\nTarget: {AssignedResource.name}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = GetStateColor();//
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 30, 180, 70), stateText, style);
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