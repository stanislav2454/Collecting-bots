using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    [Header("Bot Components")]
    public NavMeshAgent navMeshAgent;

    [Header("Debug")]
    public bool showMovementGizmos = true;

    private Vector3 targetPosition;
    private bool hasTarget = false;

    private void Awake()
    {// Получаем или добавляем необходимые компоненты
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
    }

    private void Start()
    {
        InitializeBot();
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

        Debug.Log($"Bot {gameObject.name} initialized");
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

        return !navMeshAgent.pathPending &&
               navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    // Для отладки в редакторе
    private void OnDrawGizmos()
    {
        if (showMovementGizmos == false) 
            return;

        // Показываем целеую позицию
        if (hasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);
            Gizmos.DrawLine(transform.position, targetPosition);
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