using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class BotController : MonoBehaviour
{
    public event Action<BotController, bool> MissionCompleted;

    [Header("Bot Settings")]
    [SerializeField] private float _collectionDuration = 1f;

    private NavMeshAgent _navMeshAgent;
    private BotInventory _inventory;
    private BaseController _assignedBase;
    private Item _assignedResource;
    private BotState _currentState;
    private float _stateTimer;

    public bool IsAvailable => _currentState is IdleState or WaitingState;
    public bool IsCarryingResource => _inventory.HasItem;

    // Базовый класс состояния
    private abstract class BotState
    {
        public abstract void Enter(BotController bot);
        public abstract void Update(BotController bot);
        public virtual void Exit(BotController bot) { }
    }

    // Конкретные состояния
    private class IdleState : BotState
    {
        public override void Enter(BotController bot) { }
        public override void Update(BotController bot) { }
    }

    private class WaitingState : BotState
    {
        public override void Enter(BotController bot) => bot.StopMovement();
        public override void Update(BotController bot) { }
    }

    private class MovingToResourceState : BotState
    {
        public override void Enter(BotController bot)
        {
            if (bot._assignedResource != null)
            {
                bot.MoveToPosition(bot._assignedResource.transform.position);
            }
        }

        public override void Update(BotController bot)
        {
            if (bot._assignedResource == null || !bot._assignedResource.CanBeCollected)
            {
                bot.CompleteMission(false);
                return;
            }

            if (bot.HasReachedDestination())
            {
                bot.ChangeState(new CollectingState());
            }
        }
    }

    private class CollectingState : BotState
    {
        public override void Enter(BotController bot)
        {
            bot.StopMovement();
            bot._stateTimer = 0f;
        }

        public override void Update(BotController bot)
        {
            bot._stateTimer += Time.deltaTime;

            if (bot._stateTimer >= bot._collectionDuration)
            {
                if (bot._assignedResource != null && bot._inventory.TryAddItem(bot._assignedResource))
                {
                    bot._assignedResource.Collect();
                    bot.ChangeState(new ReturningToBaseState());
                }
                else
                {
                    bot.CompleteMission(false);
                }
            }
        }
    }

    private class ReturningToBaseState : BotState
    {
        public override void Enter(BotController bot)
        {
            if (bot._assignedBase != null)
            {
                bot.MoveToPosition(bot._assignedBase.transform.position);
            }
        }

        public override void Update(BotController bot)
        {
            if (bot.HasReachedDestination())
            {
                bot._inventory.ClearInventory();
                bot.CompleteMission(true);
            }
        }
    }

    private void Awake()
    {
        if (TryGetComponent(out _navMeshAgent) && TryGetComponent(out _inventory))
        {
            ChangeState(new IdleState());
        }
    }

    public void AssignBase(BaseController baseController)
    {
        _assignedBase = baseController;
        ChangeState(new IdleState());
    }

    public void AssignResource(Item resource)
    {
        if (IsAvailable == false)
            return;

        _assignedResource = resource;
        ChangeState(new MovingToResourceState());
    }

    public void SetWaiting()
    {
        ChangeState(new WaitingState());
    }

    private void Update()
    {
        _currentState?.Update(this);
    }

    private void ChangeState(BotState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
    }

    private void CompleteMission(bool success)
    {
        _assignedResource = null;
        ChangeState(new IdleState());
        MissionCompleted?.Invoke(this, success);
    }

    private void MoveToPosition(Vector3 position)
    {
        _navMeshAgent.SetDestination(position);
    }

    private void StopMovement()
    {
        _navMeshAgent.ResetPath();
    }

    private bool HasReachedDestination()
    {
        return _navMeshAgent.pathPending == false &&
               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }
}
//using UnityEngine;
//using UnityEngine.AI;

//[DisallowMultipleComponent]
//[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
//public class BotController : MonoBehaviour
//{
//    [Header("Bot Coordination")]
//    [SerializeField] private Item _targetItem;

//    [Header("Bot Components")]
//    [SerializeField] private NavMeshAgent _navMeshAgent;
//    [SerializeField] private BotInventory _botInventory;

//    [Header("AI Settings")]
//    [SerializeField] private bool _enableAI = true;

//    [Header("Debug")]
//    [SerializeField] private bool _showMovementGizmos = true;
//    [SerializeField] private bool _showStateDebug = true;

//    private Vector3 _targetPosition;
//    private bool _hasTarget = false;
//    private BotStateMachine _stateMachine;

//    public BotState CurrentState => _stateMachine?.CurrentStateType ?? BotState.Idle;
//    public bool EnableAI => _enableAI;
//    public BotInventory BotInventory => _botInventory;
//    public NavMeshAgent NavMeshAgent => _navMeshAgent;
//    public Item TargetItem => _targetItem;

//    private void Awake()
//    {
//        InitializeComponents();
//    }

//    private void Start()
//    {
//        InitializeBot();
//    }

//    private void Update()
//    {
//        if (_enableAI)
//            _stateMachine?.Update();

//        UpdateVisuals();
//    }

//    private void FixedUpdate()
//    {
//        if (_enableAI)
//            _stateMachine?.FixedUpdate();
//    }

//    public void MoveToPosition(Vector3 position)
//    {
//        _targetPosition = position;
//        _hasTarget = true;

//        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
//            _navMeshAgent.SetDestination(position);
//    }

//    public void StopMovement()
//    {
//        _hasTarget = false;

//        if (_navMeshAgent != null)
//            _navMeshAgent.ResetPath();
//    }

//    public bool HasReachedDestination()
//    {
//        if (_hasTarget == false || _navMeshAgent == null)
//            return false;

//        return _navMeshAgent.pathPending == false &&
//               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
//    }

//    public bool TryCollectItem(Item item)
//    {
//        if (item == null || item.CanBeCollected == false)
//            return false;

//        return _botInventory.TryAddItem(item);
//    }

//    public void SetAIEnabled(bool enabled)
//    {
//        _enableAI = enabled;

//        if (enabled == false)
//            StopMovement();

//        UpdateVisuals();
//    }

//    public void SetTargetItem(Item item) =>
//        _targetItem = item;

//    public void ClearTargetItem()
//    {
//        if (_targetItem != null)// зачем нужен ? если не используется - удалить !
//        {
//            // Освобождаем предмет в менеджере
//            // BotManager botManager = FindObjectOfType<BotManager>();
//            //if (botManager != null)
//            //    botManager.ReleaseItem(_targetItem);

//            _targetItem = null;
//        }
//    }

//    public void UpdateVisuals()
//    {
//        BotVisualIndicator indicator = GetComponent<BotVisualIndicator>();
//        if (indicator != null)
//            indicator.UpdateAIStatus(_enableAI, CurrentState);
//    }

//    public string GetBotInfo() =>
//         $"Bot: {gameObject.name} | AI: {(_enableAI ? "ON" : "OFF")} | State: {CurrentState}" +
//        $" | Inventory: {_botInventory.CurrentCount}/{_botInventory.maxCapacity}";

//    private void InitializeComponents()
//    {
//        if (_navMeshAgent == null)
//            _navMeshAgent = GetComponent<NavMeshAgent>();

//        if (_navMeshAgent == null)
//            _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

//        if (_botInventory == null)
//            _botInventory = GetComponent<BotInventory>();

//        if (_botInventory == null)
//            _botInventory = gameObject.AddComponent<BotInventory>();

//        _stateMachine = new BotStateMachine(this);
//    }

//    private void InitializeBot()
//    {
//        if (GameManager.Instance != null && GameManager.Instance.gameSettings != null)
//        {
//            var settings = GameManager.Instance.gameSettings;
//            _navMeshAgent.speed = settings.botMoveSpeed;
//            _navMeshAgent.angularSpeed = settings.botRotationSpeed;
//            _navMeshAgent.stoppingDistance = settings.botStoppingDistance;
//        }
//    }

//    private void OnDrawGizmos()
//    {
//        const float Radius = 0.5f;

//        if (_showMovementGizmos == false)
//            return;

//        if (_hasTarget)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(_targetPosition, Radius);
//            Gizmos.DrawLine(transform.position, _targetPosition);
//        }

//        Gizmos.color = _botInventory.IsFull ? Color.red : Color.green;
//        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

//        if (_showStateDebug)
//        {
//#if UNITY_EDITOR
//            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"State: {CurrentState}");
//#endif
//        }

//        if (_navMeshAgent != null && _navMeshAgent.hasPath)
//        {
//            Gizmos.color = Color.blue;
//            Gizmos.DrawLine(transform.position, _navMeshAgent.destination);

//            for (int i = 0; i < _navMeshAgent.path.corners.Length - 1; i++)
//            {
//                Gizmos.DrawSphere(_navMeshAgent.path.corners[i], 0.1f);
//                Gizmos.DrawLine(_navMeshAgent.path.corners[i], _navMeshAgent.path.corners[i + 1]);
//            }
//        }
//    }
//}