using UnityEngine;

public class BotCollectState : BotBaseState
{
    private bool _collectionAttempted = false;
    private float _collectionTimer = 0f;
    private  float _collectionDuration = 1f;

    public BotCollectState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log($"{BotController.gameObject.name} collecting item: {BotController.TargetItem?.ItemName}");
        _collectionAttempted = false;
        _collectionTimer = 0f;

        // Останавливаем движение при достижении предмета
        BotController.StopMovement();
    }

    public override void Update()
    {
        if (BotController.TargetItem == null || !BotController.TargetItem.CanBeCollected)
        {
            Debug.Log($"{BotController.gameObject.name} target item is no longer available");
            BotController.ClearTargetItem();
            ChangeState(BotState.Search);
            return;
        }

        _collectionTimer += Time.deltaTime;

        // Ждем немного перед сбором для реалистичности
        if (!_collectionAttempted && _collectionTimer >= _collectionDuration)
        {
            AttemptCollection();
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        _collectionAttempted = false;
        _collectionTimer = 0f;
    }

    private void AttemptCollection()
    {
        _collectionAttempted = true;

        if (BotController.TargetItem != null)
        {
            bool success = BotController.TryCollectItem(BotController.TargetItem);

            if (success)
            {
                Debug.Log($"{BotController.gameObject.name} successfully collected: {BotController.TargetItem.ItemName}");

                // Очищаем цель после успешного сбора
                BotController.ClearTargetItem();

                // Проверяем заполненность инвентаря
                if (BotController.BotInventory.IsFull)
                {
                    Debug.Log($"{BotController.gameObject.name} inventory full, moving to deposit");
                    ChangeState(BotState.MoveToDeposit);
                }
                else
                {
                    Debug.Log($"{BotController.gameObject.name} continuing to search");
                    ChangeState(BotState.Search);
                }
            }
            else
            {
                Debug.LogWarning($"{BotController.gameObject.name} failed to collect item");
                BotController.ClearTargetItem();
                ChangeState(BotState.Search);
            }
        }
        else
        {
            Debug.Log($"{BotController.gameObject.name} no target item to collect");
            ChangeState(BotState.Search);
        }
    }
}
//using UnityEngine;

//public class BotCollectState : BotBaseState
//{
//    private Item _targetItem;
//    private float _collectTimer = 0f;
//    private const float _collectDuration = 0.5f; // Время "дОбычи"

//    public BotCollectState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        //// Находим ближайший предмет для сбора
//        //targetItem = ItemManager.Instance.GetNearestItem(BotController.transform.position);
//        _targetItem = StateMachine.GetTargetItem();
//        _collectTimer = 0f;

//        if (_targetItem == null || _targetItem.CanBeCollected == false)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        //collectTimer = 0f;
//        Debug.Log($"{BotController.gameObject.name} collecting item: {_targetItem.ItemName}");
//    }

//    public override void Update()
//    {
//        if (_targetItem == null || _targetItem.CanBeCollected == false || BotController.BotInventory.IsFull)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        _collectTimer += Time.deltaTime;

//        if (_collectTimer >= _collectDuration) // Имитируем процесс сбора
//        { // Пытаемся добавить предмет в инвентарь
//            if (BotController.TryCollectItem(_targetItem))
//            {
//                Debug.Log($"{BotController.gameObject.name} successfully collected: {_targetItem.ItemName}");

//                // Проверяем полон ли инвентарь
//                if (BotController.BotInventory.IsFull)
//                    ChangeState(BotState.MoveToDeposit);
//                else
//                    ChangeState(BotState.Search);
//            }
//            else
//            { // Не удалось собрать предмет (например, инвентарь полон)
//                ChangeState(BotState.Search);
//            }
//        }
//    }

//    public override void FixedUpdate() { }

//    public override void Exit() { }
//}