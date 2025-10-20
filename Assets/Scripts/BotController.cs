using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(BotInventory))]
public class BotController : MonoBehaviour
{
    [Header("Bot Components")]
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private BotInventory _botInventory;

    [Header("AI Settings")]
    [SerializeField] private bool _enableAI = true;

    [Header("Debug")]
    [SerializeField] private bool _showMovementGizmos = true;
    [SerializeField] private bool _showStateDebug = true;

    private Vector3 _targetPosition;
    private bool _hasTarget = false;
    private BotStateMachine _stateMachine;

    public BotState CurrentState => _stateMachine?.CurrentStateType ?? BotState.Idle;
    public bool EnableAI => _enableAI;
    public BotInventory BotInventory => _botInventory;
    public NavMeshAgent NavMeshAgent => _navMeshAgent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeBot();
    }

    private void Update()
    {
        if (_enableAI)
            _stateMachine?.Update();

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (_enableAI)
            _stateMachine?.FixedUpdate();
    }

    public void MoveToPosition(Vector3 position)
    {
        _targetPosition = position;
        _hasTarget = true;

        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
            _navMeshAgent.SetDestination(position);
    }

    public void StopMovement()
    {
        _hasTarget = false;

        if (_navMeshAgent != null)
            _navMeshAgent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (_hasTarget == false || _navMeshAgent == null)
            return false;

        return _navMeshAgent.pathPending == false &&
               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }

    public bool TryCollectItem(Item item)
    {
        if (item == null || item.CanBeCollected == false)
            return false;

        return _botInventory.TryAddItem(item);
    }

    public void SetAIEnabled(bool enabled)
    {
        _enableAI = enabled;

        if (enabled == false)
            StopMovement();

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        BotVisualIndicator indicator = GetComponent<BotVisualIndicator>();
        if (indicator != null)
            indicator.UpdateAIStatus(_enableAI, CurrentState);
    }

    public string GetBotInfo() =>
         $"Bot: {gameObject.name} | AI: {(_enableAI ? "ON" : "OFF")} | State: {CurrentState}" +
        $" | Inventory: {_botInventory.CurrentCount}/{_botInventory.maxCapacity}";

    private void InitializeComponents()
    {
        if (_navMeshAgent == null)
            _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
            _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

        if (_botInventory == null)
            _botInventory = GetComponent<BotInventory>();

        if (_botInventory == null)
            _botInventory = gameObject.AddComponent<BotInventory>();

        _stateMachine = new BotStateMachine(this);
    }

    private void InitializeBot()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameSettings != null)
        {
            var settings = GameManager.Instance.gameSettings;
            _navMeshAgent.speed = settings.botMoveSpeed;
            _navMeshAgent.angularSpeed = settings.botRotationSpeed;
            _navMeshAgent.stoppingDistance = settings.botStoppingDistance;
        }

        Debug.Log($"Bot {gameObject.name} initialized. Inventory: {_botInventory.maxCapacity} slots");
    }

    private void OnDrawGizmos() // Для отладки в редакторе
    {
        const float Radius = 0.5f;

        if (_showMovementGizmos == false)
            return;
        // УБРАТЬ: GUI.color не работает в OnDrawGizmos!
        //Color originalColor = GUI.color;
        //GUI.color = Color.red;// Устанавливаем красный цвет для всего последующего GUI

        if (_hasTarget)// Показываем целеую позицию        
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPosition, Radius);
            Gizmos.DrawLine(transform.position, _targetPosition);
        }

        // Показываем инвентарь
        Gizmos.color = _botInventory.IsFull ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

        // Показываем состояние
        if (_showStateDebug)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"State: {CurrentState}");
#endif
        }

        // Показываем текущий путь
        if (_navMeshAgent != null && _navMeshAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _navMeshAgent.destination);

            // Рисуем точки пути
            for (int i = 0; i < _navMeshAgent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(_navMeshAgent.path.corners[i], 0.1f);
                Gizmos.DrawLine(_navMeshAgent.path.corners[i], _navMeshAgent.path.corners[i + 1]);
            }
        }
    }
}