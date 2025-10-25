using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class Bot : MonoBehaviour
{// (бывший BotController)
    [Header("Bot Settings")]
    [SerializeField] private float _collectionDuration = 1f;

    private BotMovementController _movement;
    private BotInventory _inventory;
    private BotStateController _stateController;

    private Item _currentAssignedResource;

    public event Action<Bot, bool> MissionCompleted;

    public BotInventory Inventory => _inventory;
    public Item AssignedResource => _currentAssignedResource;
    public float CollectionDuration => _collectionDuration;
    public bool IsAvailable => _stateController.CurrentStateType == BotStateType.Idle ||
                              _stateController.CurrentStateType == BotStateType.Waiting;
    public bool IsCarryingResource => _inventory != null && _inventory.HasItem;
    public BotStateType CurrentStateType => _stateController.CurrentStateType;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        _stateController.UpdateState(this);
    }


    public void AssignResource(Item resource, Vector3 basePosition, float baseRadius)
    {
        if (IsAvailable == false)
        {
            Debug.Log($"❌ Бот {name} недоступен для назначения ресурса");
            return;
        }

        Debug.Log($"🎯 БОТУ {name} НАЗНАЧЕН РЕСУРС {resource.name} at {resource.transform.position}");

        _currentAssignedResource = resource;
        ChangeState(new BotMovingToResourceState(resource, basePosition, baseRadius));
    }

    public void SetWaiting()
    {
        ChangeState(new BotWaitingState());
    }

    public void ChangeState(BotState newState)
    {
        _stateController.ChangeState(newState, this);
    }

    public void CompleteMission(bool success)
    {
        if (success)
            _inventory.ClearInventory(this);
        else
            _inventory.ReleaseItem();

        _currentAssignedResource = null;
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


    // ВИЗУАЛИЗАЦИЯ В SCENE VIEW
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        Color stateColor = GetStateColor();
        Gizmos.color = stateColor;

        Vector3 iconPosition = transform.position + Vector3.up * 3f;
        Gizmos.DrawIcon(iconPosition, GetStateIcon(), true);

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

    // ВИЗУАЛИЗАЦИЯ ДЛЯ GAME VIEW (в мире)
    private void OnGUI()
    {
        if (Application.isPlaying==false)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

        if (screenPos.z <= 0) 
            return;

        string stateText = $"{CurrentStateType}";
        if (IsCarryingResource) stateText += " 📦";
        if (_currentAssignedResource != null) stateText += $"\nTarget: {_currentAssignedResource.name}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = GetStateColor();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 30, 100, 40), stateText, style);
    }

    private Color GetStateColor()
    {
        return CurrentStateType switch
        {
            BotStateType.Idle => Color.gray,
            BotStateType.Waiting => Color.blue,
            BotStateType.MovingToResource => Color.yellow,
            BotStateType.Collecting => Color.magenta,
            BotStateType.ReturningToBase => Color.cyan,
            _ => Color.white
        };
    }

    private string GetStateIcon()
    {
        return CurrentStateType switch
        {
            BotStateType.Idle => "sv_icon_dot0_pix16_gizmo",
            BotStateType.Waiting => "sv_icon_dot3_pix16_gizmo",
            BotStateType.MovingToResource => "sv_icon_dot7_pix16_gizmo",
            BotStateType.Collecting => "sv_icon_dot9_pix16_gizmo",
            BotStateType.ReturningToBase => "sv_icon_dot10_pix16_gizmo",
            _ => "sv_icon_dot0_pix16_gizmo"
        };
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