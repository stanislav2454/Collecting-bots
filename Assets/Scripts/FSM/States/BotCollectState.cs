//using UnityEngine;

//public class BotCollectState : BotBaseState
//{
//    private bool _collectionAttempted = false;
//    private float _collectionTimer = 0f;
//    private float _collectionDuration = 1f;

//    public BotCollectState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        _collectionAttempted = false;
//        _collectionTimer = 0f;

//        BotController.StopMovement();
//    }

//    public override void Update()
//    {
//        if (BotController.TargetItem == null || !BotController.TargetItem.CanBeCollected)
//        {
//            BotController.ClearTargetItem();
//            ChangeState(BotState.Search);
//            return;
//        }

//        _collectionTimer += Time.deltaTime;

//        if (_collectionAttempted == false && _collectionTimer >= _collectionDuration)
//            AttemptCollection();
//    }

//    public override void FixedUpdate()
//    {
//        // Не используется в этом состоянии
//    }

//    public override void Exit()
//    {
//        _collectionAttempted = false;
//        _collectionTimer = 0f;
//    }

//    private void AttemptCollection()
//    {
//        _collectionAttempted = true;

//        if (BotController.TargetItem != null)
//        {
//            bool success = BotController.TryCollectItem(BotController.TargetItem);

//            if (success)
//            {
//                BotController.ClearTargetItem();

//                if (BotController.BotInventory.IsFull)
//                    ChangeState(BotState.MoveToDeposit);
//                else
//                    ChangeState(BotState.Search);
//            }
//            else
//            {
//                BotController.ClearTargetItem();
//                ChangeState(BotState.Search);
//            }
//        }
//        else
//        {
//            ChangeState(BotState.Search);
//        }
//    }
//}