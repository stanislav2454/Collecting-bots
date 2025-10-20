using UnityEngine;

public class BotDepositState : BotBaseState
{
    private float depositTimer = 0f;
    private const float depositDuration = 1f; // Время "сдачи"

    public BotDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        depositTimer = 0f;
        Debug.Log($"{botController.gameObject.name} depositing items");
    }

    public override void Update()
    {
        depositTimer += Time.deltaTime;

        // Имитируем процесс сдачи предметов
        if (depositTimer >= depositDuration)
        {
            // Очищаем инвентарь
            int itemsCount = botController.botInventory.GetItemCount();
            botController.botInventory.ClearInventory();

            Debug.Log($"{botController.gameObject.name} deposited {itemsCount} items");

            // Возвращаемся к поиску предметов
            ChangeState(BotState.Search);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}