using UnityEngine;

public class BotMoveToItemState : BotBaseState
{
    private float _stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 5f;
    private Vector3 _lastPosition;

    public BotMoveToItemState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (BotController.TargetItem != null)
        {
            Debug.Log($"{BotController.gameObject.name} moving to item: {BotController.TargetItem.ItemName}");
            BotController.MoveToPosition(BotController.TargetItem.transform.position);
            _stuckTimer = 0f;
            _lastPosition = BotController.transform.position;
        }
        else
        {
            Debug.LogWarning($"{BotController.gameObject.name} no target item to move to!");
            ChangeState(BotState.Search);
        }
    }

    public override void Update()
    {
        if (BotController.TargetItem == null || !BotController.TargetItem.CanBeCollected)
        {
            Debug.Log($"{BotController.gameObject.name} target item no longer available");
            BotController.ClearTargetItem();
            ChangeState(BotState.Search);
            return;
        }

        CheckIfStuck();

        if (BotController.HasReachedDestination())
        {
            Debug.Log($"{BotController.gameObject.name} reached item");
            ChangeState(BotState.Collect);
            return;
        }

        float distanceToItem = Vector3.Distance(BotController.transform.position, BotController.TargetItem.transform.position);
        if (distanceToItem <= 1.5f)
        {
            Debug.Log($"{BotController.gameObject.name} close enough to item");
            ChangeState(BotState.Collect);
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        _stuckTimer = 0f;
    }

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(BotController.transform.position, _lastPosition);

        if (distanceMoved < 0.1f)
        {
            _stuckTimer += Time.deltaTime;

            if (_stuckTimer >= STUCK_THRESHOLD)
            {
                Debug.LogWarning($"{BotController.gameObject.name} appears stuck, recalculating path");
                BotController.ClearTargetItem();
                ChangeState(BotState.Search);
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

//public class BotMoveToItemState : BotBaseState
//{
//    private Item targetItem;
//    private float searchTimer = 0f;
//    private const float searchInterval = 1f;

//    public BotMoveToItemState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        FindNearestItem();

//        if (targetItem == null)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//     //   Debug.Log($"{BotController.gameObject.name} moving to item: {targetItem.ItemName}");
//        BotController.MoveToPosition(targetItem.transform.position);
//    }

//    public override void Update()
//    {
//        if (targetItem == null || !targetItem.CanBeCollected)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        // Периодически проверяем актуальность цели
//        searchTimer += Time.deltaTime;
//        if (searchTimer >= searchInterval)
//        {
//            searchTimer = 0f;
//            FindNearestItem(); // Может найти более близкий предмет
//        }

//        // Проверяем достигли ли мы предмета
//        if (BotController.HasReachedDestination())
//        {
//            ChangeState(BotState.Collect);
//        }
//    }

//    public override void FixedUpdate() { }

//    public override void Exit() { }

//    private void FindNearestItem()
//    {
//        Item newTarget = ItemManager.Instance.GetNearestItem(BotController.transform.position);

//        if (newTarget != null && newTarget != targetItem)
//        {
//            targetItem = newTarget;
//            BotController.MoveToPosition(targetItem.transform.position);
//        }
//    }
//}