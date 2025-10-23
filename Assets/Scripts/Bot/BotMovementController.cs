using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BotMovementController : MonoBehaviour
{
    [SerializeField] private float _pathRecalculationInterval = 0.5f;
    [SerializeField] private float _targetChangeThreshold = 0.1f;

    private NavMeshAgent _navMeshAgent;
    private float _lastRecalculationTime;
    private Vector3 _lastTargetPosition;
    private float _sqrTargetChangeThreshold;

    public bool IsMoving => _navMeshAgent.hasPath && _navMeshAgent.isStopped == false;
    public float RemainingDistance => _navMeshAgent.remainingDistance;

    private void Awake()
    {
        TryGetComponent(out _navMeshAgent);
        _sqrTargetChangeThreshold = _targetChangeThreshold * _targetChangeThreshold;
    }

    public void MoveToPosition(Vector3 position)
    {
        if (_navMeshAgent != null && _navMeshAgent.isActiveAndEnabled)
        {
            bool needsRecalculation = (_lastTargetPosition - position).sqrMagnitude > _sqrTargetChangeThreshold ||
                                     Time.time - _lastRecalculationTime >= _pathRecalculationInterval;

            if (needsRecalculation)
            {
                _navMeshAgent.SetDestination(position);
                _lastTargetPosition = position;
                _lastRecalculationTime = Time.time;
            }
        }
    }

    public void StopMovement()
    {
        if (_navMeshAgent != null)
            _navMeshAgent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (_navMeshAgent == null || _navMeshAgent.isActiveAndEnabled == false)
            return false;

        return _navMeshAgent.pathPending == false &&
               _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
               (_navMeshAgent.hasPath == false || _navMeshAgent.velocity.sqrMagnitude == 0f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            _sqrTargetChangeThreshold = _targetChangeThreshold * _targetChangeThreshold;
    }
#endif
}