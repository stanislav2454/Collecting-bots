using UnityEngine;

public class BotMoveToItemState : BotBaseState
{
    private Item targetItem;
    private float searchTimer = 0f;
    private const float searchInterval = 1f;

    public BotMoveToItemState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        FindNearestItem();

        if (targetItem == null)
        {
            ChangeState(BotState.Search);
            return;
        }

     //   Debug.Log($"{BotController.gameObject.name} moving to item: {targetItem.ItemName}");
        BotController.MoveToPosition(targetItem.transform.position);
    }

    public override void Update()
    {
        if (targetItem == null || !targetItem.CanBeCollected)
        {
            ChangeState(BotState.Search);
            return;
        }

        // Периодически проверяем актуальность цели
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            FindNearestItem(); // Может найти более близкий предмет
        }

        // Проверяем достигли ли мы предмета
        if (BotController.HasReachedDestination())
        {
            ChangeState(BotState.Collect);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }

    private void FindNearestItem()
    {
        Item newTarget = ItemManager.Instance.GetNearestItem(BotController.transform.position);

        if (newTarget != null && newTarget != targetItem)
        {
            targetItem = newTarget;
            BotController.MoveToPosition(targetItem.transform.position);
        }
    }
}