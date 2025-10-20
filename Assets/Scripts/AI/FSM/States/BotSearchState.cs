using UnityEngine;

public class BotSearchState : BotBaseState
{
    public BotSearchState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
       // Debug.Log($"{BotController.gameObject.name} started searching for items");
        SearchForItem();
    }

    public override void Update()
    {
        // Если инвентарь полный, идем сдавать предметы
        if (BotController.BotInventory.IsFull)
        {
            ChangeState(BotState.MoveToDeposit);
            return;
        }

        SearchForItem();
    }

    public override void FixedUpdate() { }

    public override void Exit() { }

    private void SearchForItem()
    {
        Item nearestItem = ItemManager.Instance.GetNearestItem(BotController.transform.position);

        if (nearestItem != null) // Нашли предмет - переходим к движению к нему
            StateMachine.ChangeState(BotState.MoveToItem);
        else // Не нашли предмет - ждем немного и снова ищем
            ChangeState(BotState.Wait);
    }
}