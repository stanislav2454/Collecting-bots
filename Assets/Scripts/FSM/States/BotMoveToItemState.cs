//using UnityEngine;

//public class BotMoveToItemState : BotBaseState
//{
//    private float _stuckTimer = 0f;
//    private const float STUCK_THRESHOLD = 5f;
//    private Vector3 _lastPosition;

//    public BotMoveToItemState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        if (BotController.TargetItem != null)
//        {
//            BotController.MoveToPosition(BotController.TargetItem.transform.position);
//            _stuckTimer = 0f;
//            _lastPosition = BotController.transform.position;
//        }
//        else
//        {
//            ChangeState(BotState.Search);
//        }
//    }

//    public override void Update()
//    {
//        if (BotController.TargetItem == null || !BotController.TargetItem.CanBeCollected)
//        {
//            BotController.ClearTargetItem();
//            ChangeState(BotState.Search);
//            return;
//        }

//        CheckIfStuck();

//        if (BotController.HasReachedDestination())
//        {
//            ChangeState(BotState.Collect);
//            return;
//        }

//        float distanceToItem = Vector3.Distance(BotController.transform.position, BotController.TargetItem.transform.position);// Vector3.Distance - ресурсозатратно => переделать

//        if (distanceToItem <= 1.5f)
//        {
//            ChangeState(BotState.Collect);
//        }
//    }

//    public override void FixedUpdate()
//    {
//        // Не используется в этом состоянии
//    }

//    public override void Exit()
//    {
//        _stuckTimer = 0f;
//    }

//    private void CheckIfStuck()
//    {
//        float distanceMoved = Vector3.Distance(BotController.transform.position, _lastPosition);// Vector3.Distance - ресурсозатратно => переделать

//        if (distanceMoved < 0.1f)
//        {
//            _stuckTimer += Time.deltaTime;

//            if (_stuckTimer >= STUCK_THRESHOLD)
//            {
//                BotController.ClearTargetItem();
//                ChangeState(BotState.Search);
//            }
//        }
//        else
//        {
//            _stuckTimer = 0f;
//            _lastPosition = BotController.transform.position;
//        }
//    }
//}