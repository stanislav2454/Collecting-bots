//using UnityEngine;

//public class BotDepositState : BotBaseState
//{
//    private DepositZone _depositZone;
//    private float _depositTimer = 0f;
//    private const float DEPOSIT_DURATION = 1.5f;

//    public BotDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        FindDepositZone();
//        _depositTimer = 0f;

//        if (_depositZone == null)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        if (BotController.BotInventory.CurrentCount == 0)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        BotController.StopMovement();
//    }

//    public override void Update()
//    {
//        if (_depositZone == null || BotController.BotInventory.CurrentCount == 0)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        _depositTimer += Time.deltaTime;

//        if (_depositTimer >= DEPOSIT_DURATION)
//            CompleteDeposit();
//    }

//    public override void FixedUpdate()
//    {
//        // Не используется в этом состоянии
//    }

//    public override void Exit()
//    {
//        _depositTimer = 0f;
//    }

//    private void CompleteDeposit()
//    {
//        bool success = false;

//        if (ServiceLocator.TryGet<IDepositService>(out var depositService))
//        {
//            success = depositService.ProcessDeposit(
//                BotController.BotInventory,
//                BotController.transform.position);
//        }
//        else
//        {
//            success = _depositZone.ProcessDeposit(BotController.BotInventory);
//        }

//        ChangeState(BotState.Search);
//    }

//    private void FindDepositZone()
//    {
//        if (ServiceLocator.TryGet<IDepositService>(out var depositService))
//            _depositZone = depositService.GetNearestDepositZone(BotController.transform.position);
//        else
//            FindDepositZoneFallback();
//    }

//    private void FindDepositZoneFallback()
//    {
//        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

//        if (depositZones.Length == 0)
//        {
//            _depositZone = null;
//            return;
//        }

//        DepositZone closestZone = null;
//        float closestDistance = Mathf.Infinity;

//        foreach (var zone in depositZones)
//        {
//            float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);// Vector3.Distance - ресурсозатратно => переделать

//            if (distance <= 3f && distance < closestDistance)
//            {
//                closestDistance = distance;
//                closestZone = zone;
//            }
//        }

//        _depositZone = closestZone;
//    }
//}