using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BotMovementController : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;

    public bool IsMoving => _navMeshAgent.hasPath && !_navMeshAgent.isStopped;
    public float RemainingDistance => _navMeshAgent.remainingDistance;

    private void Awake()
    {
        TryGetComponent(out _navMeshAgent);
    }

    /// <summary>
    /// Начинает движение к указанной позиции
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            _navMeshAgent.SetDestination(position);
        }
    }

    /// <summary>
    /// Останавливает движение бота
    /// </summary>
    public void StopMovement()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.ResetPath();
        }
    }

    /// <summary>
    /// Проверяет, достигнута ли целевая позиция
    /// </summary>
    public bool HasReachedDestination()
    {
        return _navMeshAgent != null &&
               !_navMeshAgent.pathPending &&
               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
    }

    /// <summary>
    /// Настраивает параметры движения
    /// </summary>
    public void ConfigureMovement(float speed, float angularSpeed, float stoppingDistance)
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.speed = speed;
            _navMeshAgent.angularSpeed = angularSpeed;
            _navMeshAgent.stoppingDistance = stoppingDistance;
        }
    }
}