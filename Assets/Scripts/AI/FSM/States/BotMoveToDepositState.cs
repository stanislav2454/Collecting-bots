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
        Debug.Log($"{BotController.gameObject.name} moving to deposit zone");

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
            Debug.LogWarning($"{BotController.gameObject.name} no deposit zone found!");
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
            Debug.Log($"{BotController.gameObject.name} reached deposit zone");
            ChangeState(BotState.Deposit);
            return;
        }

        float distanceToDeposit = Vector3.Distance(BotController.transform.position, _depositPosition);
        if (distanceToDeposit <= 2f)
        {
            Debug.Log($"{BotController.gameObject.name} close enough to deposit");
            ChangeState(BotState.Deposit);
        }
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
        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();

        if (depositZones.Length == 0)
        {
            _depositZone = null;
            Debug.LogWarning("No deposit zones found in scene!");
            return;
        }

        DepositZone closestZone = null;
        float closestDistance = Mathf.Infinity;

        foreach (var zone in depositZones)
        {
            // ПРОВЕРЯЕМ ДОСТУПНОСТЬ ЧЕРЕЗ NAVMESH
            if (IsPositionReachable(zone.transform.position))
            {
                float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestZone = zone;
                }
            }
        }

        if (closestZone == null)
        {
            Debug.LogWarning($"{BotController.gameObject.name} no reachable deposit zone found!");
        }

        _depositZone = closestZone;
    }

    private bool IsPositionReachable(Vector3 targetPosition)
    {
        // Проверяем доступность позиции через NavMesh
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
        float distanceMoved = Vector3.Distance(BotController.transform.position, _lastPosition);

        if (distanceMoved < 0.1f)
        {
            _stuckTimer += Time.deltaTime;

            if (_stuckTimer >= 5f)
            {
                Debug.LogWarning($"{BotController.gameObject.name} stuck going to deposit, recalculating");

                // Пытаемся найти другую зону сдачи
                FindDepositZone();

                if (_depositZone != null && IsPositionReachable(_depositZone.transform.position))
                {
                    // Пробуем еще раз с новой позицией
                    _depositPosition = _depositZone.transform.position;
                    BotController.MoveToPosition(_depositPosition);
                    _stuckTimer = 0f;
                    _lastPosition = BotController.transform.position;
                    Debug.Log($"{BotController.gameObject.name} retrying with new path");
                }
                else
                {
                    // Если не нашли доступную зону - возвращаемся к поиску
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
//using UnityEngine;

//public class BotMoveToDepositState : BotBaseState
//{
//    private DepositZone _depositZone;
//    private Vector3 _depositPosition;

//    public BotMoveToDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        Debug.Log($"{BotController.gameObject.name} moving to deposit zone");

//        FindDepositZone();

//        if (_depositZone != null)
//        {
//            _depositPosition = _depositZone.transform.position;
//            BotController.MoveToPosition(_depositPosition);
//        }
//        else
//        {
//            Debug.LogWarning($"{BotController.gameObject.name} no deposit zone found!");
//            StateMachine.ChangeState(BotState.Search);
//        }

//        ////  Debug.Log($"{BotController.gameObject.name} moving to deposit zone");
//        //BotController.MoveToPosition(_depositZone.transform.position);
//    }

//    public override void Update()
//    {
//        if (_depositZone == null)
//        {
//            StateMachine.ChangeState(BotState.Search);
//            return;
//        }

//        if (BotController.HasReachedDestination())
//        {
//            Debug.Log($"{BotController.gameObject.name} reached deposit zone");
//            StateMachine.ChangeState(BotState.Deposit);
//            return;
//        }

//        // Дополнительная проверка proximity к зоне
//        float distanceToDeposit = Vector3.Distance(BotController.transform.position, _depositPosition);
//        if (distanceToDeposit <= 2f) // Радиус сдачи
//        {
//            Debug.Log($"{BotController.gameObject.name} close enough to deposit");
//            StateMachine.ChangeState(BotState.Deposit);
//        }
//    }

//    public override void FixedUpdate() { }

//    public override void Exit()
//    {// Останавливаем движение при выходе из состояния
//        BotController.StopMovement();
//    }

//    private void FindDepositZone()
//    {// Ищем ближайшую зону сдачи
//        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>(); // todo =>

//        if (depositZones.Length == 0)
//        {
//            _depositZone = null;
//            return;
//        }

//        // Выбираем ближайшую зону
//        DepositZone closestZone = null;
//        float closestDistance = Mathf.Infinity;

//        foreach (var zone in depositZones)
//        {            // todo => Vector3.Distance()
//            float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);
//            if (distance < closestDistance)
//            {
//                closestDistance = distance;
//                closestZone = zone;
//            }
//        }

//        _depositZone = closestZone;
//    }
//}