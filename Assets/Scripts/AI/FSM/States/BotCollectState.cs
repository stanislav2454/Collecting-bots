using UnityEngine;

public class BotCollectState : BotBaseState
{
    private Item targetItem;
    private float collectTimer = 0f;
    private const float collectDuration = 0.5f; // Время "дОбычи"

    public BotCollectState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Находим ближайший предмет для сбора
        targetItem = ItemManager.Instance.GetNearestItem(BotController.transform.position);

        if (targetItem == null || !targetItem.CanBeCollected)
        {
            ChangeState(BotState.Search);
            return;
        }

        collectTimer = 0f;
       // Debug.Log($"{BotController.gameObject.name} collecting item: {targetItem.ItemName}");
    }

    public override void Update()
    {
        if (targetItem == null || !targetItem.CanBeCollected)
        {
            ChangeState(BotState.Search);
            return;
        }

        collectTimer += Time.deltaTime;

        // Имитируем процесс сбора
        if (collectTimer >= collectDuration)
        { // Пытаемся добавить предмет в инвентарь
            if (BotController.TryCollectItem(targetItem))
            {
               // Debug.Log($"{BotController.gameObject.name} successfully collected: {targetItem.ItemName}");

                // Проверяем полон ли инвентарь
                if (BotController.BotInventory.IsFull)
                    ChangeState(BotState.MoveToDeposit);
                else
                    ChangeState(BotState.Search);
            }
            else
            { // Не удалось собрать предмет (например, инвентарь полон)
                ChangeState(BotState.Search);
            }
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}