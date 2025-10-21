using UnityEngine;

public class BotMoveToDepositState : BotBaseState
{
    private DepositZone _depositZone;
    private Vector3 _depositPosition;
    private float _stuckTimer = 0f;
    private Vector3 _lastPosition;

    public BotMoveToDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        FindDepositZone();

        if (_depositZone != null)
        {
            _depositPosition = _depositZone.transform.position;
            BotController.MoveToPosition(_depositPosition);
            _stuckTimer = 0f;
            _lastPosition = BotController.transform.position;
        }
        else
        {
            ChangeState(BotState.Search);
        }
    }

    public override void Update()
    {
        if (_depositZone == null)
        {
            ChangeState(BotState.Search);
            return;
        }

        CheckIfStuck();

        if (BotController.HasReachedDestination())
        {
            ChangeState(BotState.Deposit);
            return;
        }

        float distanceToDeposit = Vector3.Distance(BotController.transform.position, _depositPosition);// Vector3.Distance - ресурсозатратно => переделать

        if (distanceToDeposit <= 2f)
            ChangeState(BotState.Deposit);
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        BotController.StopMovement();
    }

    private void FindDepositZone()
    {
        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        if (depositZones.Length == 0)
        {
            _depositZone = null;
            return;
        }

        if (ServiceLocator.TryGet<IDepositService>(out var depositService))
            _depositZone = depositService.GetNearestDepositZone(BotController.transform.position);
        else
            FindDepositZoneFallback();
    }

    private void FindDepositZoneFallback()
    {
        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        if (depositZones.Length == 0)
        {
            _depositZone = null;
            return;
        }

        DepositZone closestZone = null;
        float closestDistance = Mathf.Infinity;

        foreach (var zone in depositZones)
        {
            float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);// Vector3.Distance - ресурсозатратно => переделать

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestZone = zone;
            }
        }

        _depositZone = closestZone;
    }

    private bool IsPositionReachable(Vector3 targetPosition)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        bool pathFound = UnityEngine.AI.NavMesh.CalculatePath(
            BotController.transform.position,
            targetPosition,
            UnityEngine.AI.NavMesh.AllAreas,
            path);

        return pathFound && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
    }

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(BotController.transform.position, _lastPosition);// Vector3.Distance - ресурсозатратно => переделать

        if (distanceMoved < 0.1f)
        {
            _stuckTimer += Time.deltaTime;

            if (_stuckTimer >= 5f)
            {
                FindDepositZone();

                if (_depositZone != null && IsPositionReachable(_depositZone.transform.position))
                {
                    _depositPosition = _depositZone.transform.position;
                    BotController.MoveToPosition(_depositPosition);
                    _stuckTimer = 0f;
                    _lastPosition = BotController.transform.position;
                }
                else
                {
                    BotController.ClearTargetItem();
                    ChangeState(BotState.Search);
                }
            }
        }
        else
        {
            _stuckTimer = 0f;
            _lastPosition = BotController.transform.position;
        }
    }
}