using UnityEngine;

public class BotSearchState : BotBaseState
{
    private float _searchCooldown = 0f;
    private const float _searchCooldownTime = 2f;
    private int _failedSearchAttempts = 0;
    private const int MaxFailedAttempts = 3;

    public BotSearchState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log($"{BotController.gameObject.name} started searching for items");
        _searchCooldown = 0f;
        _failedSearchAttempts = 0;
    }

    public override void Update()
    {
        _searchCooldown -= Time.deltaTime;

        if (_searchCooldown <= 0f)
        {
            SearchForItem();
            _searchCooldown = _searchCooldownTime;
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        // Освобождаем зарезервированный предмет при выходе из поиска
        if (BotController.TargetItem != null)
        {
            BotManager botManager = Object.FindObjectOfType<BotManager>();
            if (botManager != null)
            {
                botManager.ReleaseItem(BotController.TargetItem);
            }
        }
    }

    private void SearchForItem()
    {
        // Проверяем не заполнен ли инвентарь
        if (BotController.BotInventory.IsFull)
        {
            Debug.Log($"{BotController.gameObject.name} inventory full, moving to deposit");
            ChangeState(BotState.MoveToDeposit);
            return;
        }

        BotManager botManager = Object.FindObjectOfType<BotManager>();
        if (botManager == null)
        {
            // Fallback к старой системе если менеджера нет
            SearchForItemFallback();
            return;
        }

        // Используем менеджер для поиска доступного предмета
        Item availableItem = botManager.FindAvailableItemForBot(
            BotController,
            BotController.transform.position,
            20f
        );

        if (availableItem != null)
        {
            BotController.SetTargetItem(availableItem);
            _failedSearchAttempts = 0;
            Debug.Log($"{BotController.gameObject.name} found available item: {availableItem.ItemName}");
            ChangeState(BotState.MoveToItem);
        }
        else
        {
            _failedSearchAttempts++;
            Debug.Log($"{BotController.gameObject.name} no available items found (attempt {_failedSearchAttempts})");

            if (_failedSearchAttempts >= MaxFailedAttempts)
            {
                Debug.Log($"{BotController.gameObject.name} too many failed searches, waiting...");
                ChangeState(BotState.Wait);
            }
        }
    }

    private void SearchForItemFallback()
    {
        Item[] allItems = Object.FindObjectsOfType<Item>();
        Item closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (var item in allItems)
        {
            if (item == null || !item.CanBeCollected)
                continue;

            float distance = Vector3.Distance(BotController.transform.position, item.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            BotController.SetTargetItem(closestItem);
            ChangeState(BotState.MoveToItem);
        }
        else
        {
            _failedSearchAttempts++;
            if (_failedSearchAttempts >= MaxFailedAttempts)
            {
                ChangeState(BotState.Wait);
            }
        }
    }
}
//using UnityEngine;

//public class BotSearchState : BotBaseState
//{
//    private const float SEARCH_COOLDOWN_TIME = 1f;//todo

//    private float _searchCooldown = 0.5f;
//    private float _lastSearchTime;

//    public BotSearchState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        Debug.Log($"{BotController.gameObject.name} started searching for items");
//        _searchCooldown = 0f;
//        //_lastSearchTime = -_searchCooldown; // Чтобы сразу начать поиск
//        //Debug.Log($"{BotController.gameObject.name} started searching for items");
//        //// SearchForItem();
//    }

//    public override void Update()
//    {
//        _searchCooldown -= Time.deltaTime;

//        if (_searchCooldown <= 0f)
//        {
//            SearchForItem();
//            _searchCooldown = SEARCH_COOLDOWN_TIME;
//        }
//        //// Если инвентарь полный, идем сдавать предметы
//        //if (BotController.BotInventory.IsFull)
//        //{// ПРИОРИТЕТ 1: Если инвентарь полный - идем сдавать
//        //    ChangeState(BotState.MoveToDeposit);
//        //    return;
//        //}

//        //if (Time.time - _lastSearchTime >= _searchCooldown)
//        //{// Поиск с интервалом чтобы не нагружать CPU каждый кадр
//        //    _lastSearchTime = Time.time;
//        //    SearchForItem();
//        //}
//    }

//    public override void FixedUpdate() { }

//    public override void Exit()
//    {
//        // Освобождаем зарезервированный предмет при выходе из поиска
//        if (BotController.TargetItem != null)
//        {
//            BotManager botManager = Object.FindObjectOfType<BotManager>();
//            if (botManager != null)
//                botManager.ReleaseItem(BotController.TargetItem);
//        }
//    }

//    private void SearchForItem()
//    {
//        BotManager botManager = Object.FindObjectOfType<BotManager>();
//        if (botManager == null)
//        {
//            // Fallback к старой системе если менеджера нет
//            SearchForItemFallback();
//            return;
//        }

//        // Используем менеджер для поиска доступного предмета
//        Item availableItem = botManager.FindAvailableItemForBot(
//            BotController, BotController.transform.position, 20f);// радиус поиска

//        if (availableItem != null)
//        {
//            BotController.SetTargetItem(availableItem);
//            Debug.Log($"{BotController.gameObject.name} found available item: {availableItem.ItemName}");
//            StateMachine.ChangeState(BotState.MoveToItem);
//        }
//        else
//        {
//            // Если предметов нет - ждем
//            Debug.Log($"{BotController.gameObject.name} no available items found, waiting...");
//            StateMachine.ChangeState(BotState.Wait);
//        }
//        //Item nearestItem = ItemManager.Instance.GetNearestItem(BotController.transform.position);

//        //if (nearestItem != null && nearestItem.CanBeCollected)
//        //{ // Сохраняем целевой предмет в StateMachine для использования в других состояниях
//        //    StateMachine.SetTargetItem(nearestItem);
//        //    ChangeState(BotState.MoveToItem);
//        //}
//        //else
//        //{
//        //    // Не нашли предмет - ждем немного и снова ищем
//        //    ChangeState(BotState.Wait);
//        //}
//    }

//    private void SearchForItemFallback()
//    {
//        // Старая система поиска для обратной совместимости
//        Item[] allItems = Object.FindObjectsOfType<Item>();
//        Item closestItem = null;
//        float closestDistance = float.MaxValue;

//        foreach (var item in allItems)
//        {
//            if (item == null || item.CanBeCollected == false)
//                continue;

//            float distance = Vector3.Distance(BotController.transform.position, item.transform.position);
//            if (distance < closestDistance)
//            {
//                closestDistance = distance;
//                closestItem = item;
//            }
//        }

//        if (closestItem != null)
//        {
//            BotController.SetTargetItem(closestItem);
//            StateMachine.ChangeState(BotState.MoveToItem);
//        }
//        else
//        {
//            StateMachine.ChangeState(BotState.Wait);
//        }
//    }
//}