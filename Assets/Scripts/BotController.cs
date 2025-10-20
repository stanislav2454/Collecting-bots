using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    [Header("Bot Components")]
    public NavMeshAgent navMeshAgent;
    public BotInventory botInventory;

    [Header("AI Settings")]
    public bool enableAI = true;

    [Header("Debug")]
    public bool showMovementGizmos = true;
    public bool showStateDebug = true;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private BotStateMachine stateMachine;

    public BotState CurrentState => stateMachine?.CurrentStateType ?? BotState.Idle;

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
        if (enableAI)
        {
            stateMachine?.Update();
        }
    }

    private void FixedUpdate()
    {
        if (enableAI)
        {
            stateMachine?.FixedUpdate();
        }
    }

    private void InitializeComponents()
    {
        // NavMeshAgent
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

        // Inventory
        if (botInventory == null)
            botInventory = GetComponent<BotInventory>();

        if (botInventory == null)
            botInventory = gameObject.AddComponent<BotInventory>();

        // State Machine
        stateMachine = new BotStateMachine(this);
    }

    private void InitializeBot()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameSettings != null)
        {
            var settings = GameManager.Instance.gameSettings;
            navMeshAgent.speed = settings.botMoveSpeed;
            navMeshAgent.angularSpeed = settings.botRotationSpeed;
            navMeshAgent.stoppingDistance = settings.botStoppingDistance;
        }

        Debug.Log($"Bot {gameObject.name} initialized. Inventory: {botInventory.maxCapacity} slots");
    }

    public void MoveToPosition(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            navMeshAgent.SetDestination(position);
    }

    public void StopMovement()
    {
        hasTarget = false;
        if (navMeshAgent != null)
            navMeshAgent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (hasTarget == false || navMeshAgent == null)
            return false;

        return navMeshAgent.pathPending == false &&
               navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    public bool TryCollectItem(Item item)
    {    // Новый метод для сбора предмета
        if (item == null || item.CanBeCollected == false)
            return false;

        return botInventory.TryAddItem(item);
    }

    // Для отладки в редакторе
    private void OnDrawGizmos()
    {
        const float Radius = 0.5f;

        if (showMovementGizmos == false)
            return;

        // Показываем целеую позицию
        if (hasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, Radius);
            Gizmos.DrawLine(transform.position, targetPosition);
        }

        // Показываем инвентарь
        Gizmos.color = botInventory.IsFull ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

        // Показываем состояние
        if (showStateDebug)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"State: {CurrentState}");
#endif
        }

        // Показываем текущий путь
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, navMeshAgent.destination);

            // Рисуем точки пути
            for (int i = 0; i < navMeshAgent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(navMeshAgent.path.corners[i], 0.1f);
                Gizmos.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1]);
            }
        }
    }
}