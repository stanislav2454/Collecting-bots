using UnityEngine;

public class BotDepositState : BotBaseState
{
    private float depositTimer = 0f;
    private const float depositDuration = 1f; // Время "разгрузки"

    public BotDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        depositTimer = 0f;
        //Debug.Log($"{BotController.gameObject.name} depositing items");
    }

    public override void Update()
    {
        depositTimer += Time.deltaTime;

        // Имитируем процесс сдачи предметов
        if (depositTimer >= depositDuration)
        {
            int itemsCount = BotController.BotInventory.GetItemCount();
            BotController.BotInventory.ClearInventory();

            //Debug.Log($"{BotController.gameObject.name} deposited {itemsCount} items");

            ChangeState(BotState.Search);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}