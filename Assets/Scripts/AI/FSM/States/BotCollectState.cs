using UnityEngine;

public class BotCollectState : BotBaseState
{
    private Item targetItem;
    private float collectTimer = 0f;
    private const float collectDuration = 0.5f; // Время "сбора"

    public BotCollectState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Находим ближайший предмет для сбора
        targetItem = ItemManager.Instance.GetNearestItem(botController.transform.position);

        if (targetItem == null || !targetItem.CanBeCollected)
        {
            ChangeState(BotState.Search);
            return;
        }

        collectTimer = 0f;
        Debug.Log($"{botController.gameObject.name} collecting item: {targetItem.ItemName}");
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
        {
            // Пытаемся добавить предмет в инвентарь
            if (botController.TryCollectItem(targetItem))
            {
                Debug.Log($"{botController.gameObject.name} successfully collected: {targetItem.ItemName}");

                // Проверяем полон ли инвентарь
                if (botController.botInventory.IsFull)
                {
                    ChangeState(BotState.MoveToDeposit);
                }
                else
                {
                    ChangeState(BotState.Search);
                }
            }
            else
            {
                // Не удалось собрать предмет (например, инвентарь полон)
                ChangeState(BotState.Search);
            }
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}