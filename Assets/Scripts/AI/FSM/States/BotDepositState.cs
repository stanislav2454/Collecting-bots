using UnityEngine;

public class BotDepositState : BotBaseState
{
    private DepositZone _depositZone;
    private float _depositTimer = 0f;
    private const float DEPOSIT_DURATION = 1.5f;

    public BotDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log($"{BotController.gameObject.name} depositing items");

        FindDepositZone();
        _depositTimer = 0f;

        if (_depositZone == null)
        {
            Debug.LogWarning($"{BotController.gameObject.name} no deposit zone found for deposit!");
            ChangeState(BotState.Search);
            return;
        }

        if (BotController.BotInventory.CurrentCount == 0)
        {
            Debug.Log($"{BotController.gameObject.name} no items to deposit");
            ChangeState(BotState.Search);
            return;
        }

        BotController.StopMovement();
    }

    public override void Update()
    {
        if (_depositZone == null || BotController.BotInventory.CurrentCount == 0)
        {
            ChangeState(BotState.Search);
            return;
        }

        _depositTimer += Time.deltaTime;

        if (_depositTimer >= DEPOSIT_DURATION)
        {
            CompleteDeposit();
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        _depositTimer = 0f;
    }

    private void CompleteDeposit()
    {
        //bool success = _depositZone.ProcessDeposit(BotController.BotInventory);
        bool success = false;

        // Используем DepositService если доступен
        if (ServiceLocator.TryGet<IDepositService>(out var depositService))
        {
            success = depositService.ProcessDeposit(
                BotController.BotInventory,
                BotController.transform.position);
        }
        else
        {
            // Fallback на прямую работу с DepositZone
            success = _depositZone.ProcessDeposit(BotController.BotInventory);
        }

        if (success)
        {
            Debug.Log($"{BotController.gameObject.name} completed deposit");
        }
        else
        {
            Debug.LogWarning($"{BotController.gameObject.name} failed to deposit items");
        }

        ChangeState(BotState.Search);
    }

    private void FindDepositZone()
    {
        // Используем DepositService если доступен
        if (ServiceLocator.TryGet<IDepositService>(out var depositService))
        {
            _depositZone = depositService.GetNearestDepositZone(BotController.transform.position);
        }
        else
        {
            // Fallback на старую систему
            FindDepositZoneFallback();
        }
        //DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();

        //if (depositZones.Length == 0)
        //{
        //    _depositZone = null;
        //    return;
        //}

        //DepositZone closestZone = null;
        //float closestDistance = Mathf.Infinity;

        //foreach (var zone in depositZones)
        //{
        //    float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);
        //    if (distance <= 3f && distance < closestDistance)
        //    {
        //        closestDistance = distance;
        //        closestZone = zone;
        //    }
        //}

        //_depositZone = closestZone;
    }

    private void FindDepositZoneFallback()
    {
        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();

        if (depositZones.Length == 0)
        {
            _depositZone = null;
            return;
        }

        DepositZone closestZone = null;
        float closestDistance = Mathf.Infinity;

        foreach (var zone in depositZones)
        {
            float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);
            if (distance <= 3f && distance < closestDistance)
            {
                closestDistance = distance;
                closestZone = zone;
            }
        }

        _depositZone = closestZone;
    }
}//using UnityEngine;

//public class BotDepositState : BotBaseState
//{
//    [SerializeField] private float _depositDuration = 1.5f; // Время "разгрузки"
//    private DepositZone _depositZone;
//    private float _depositTimer = 0f;

//    public BotDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        Debug.Log($"{BotController.gameObject.name} depositing items");

//        FindDepositZone();
//        _depositTimer = 0f;

//        if (_depositZone == null)
//        {
//            Debug.LogWarning($"{BotController.gameObject.name} no deposit zone found for deposit!");
//            StateMachine.ChangeState(BotState.Search);
//            return;
//        }

//        if (BotController.BotInventory.CurrentCount == 0)
//        {
//            Debug.Log($"{BotController.gameObject.name} no items to deposit");
//            StateMachine.ChangeState(BotState.Search);
//            return;
//        }

//        // Останавливаем бота
//        BotController.StopMovement();
//    }

//    public override void Update()
//    {
//        if (_depositZone == null || BotController.BotInventory.IsEmpty)
//        {
//            ChangeState(BotState.Search);
//            return;
//        }

//        _depositTimer += Time.deltaTime;

//        if (_depositTimer >= _depositDuration)
//            CompleteDeposit();
//    }

//    public override void FixedUpdate() { }

//    public override void Exit()
//    {
//        _depositTimer = 0f;
//    }

//    private void CompleteDeposit()
//    {
//        // Процесс сдачи предметов
//        bool success = _depositZone.ProcessDeposit(BotController.BotInventory);

//        if (success)
//            Debug.Log($"{BotController.gameObject.name} completed deposit");
//        else
//            Debug.LogWarning($"{BotController.gameObject.name} failed to deposit items");

//        // Возвращаемся к поиску предметов
//        ChangeState(BotState.Search);
//    }

//    private void FindDepositZone()
//    {// Ищем зону сдачи в радиусе
//        DepositZone[] depositZones = Object.FindObjectsOfType<DepositZone>();

//        if (depositZones.Length == 0)
//        {
//            _depositZone = null;
//            return;
//        }

//        // Выбираем ближайшую зону в радиусе
//        DepositZone closestZone = null;
//        float closestDistance = Mathf.Infinity;

//        foreach (var zone in depositZones)
//        {            // todo => Vector3.Distance()
//            float distance = Vector3.Distance(BotController.transform.position, zone.transform.position);
//            if (distance <= 3f && distance < closestDistance) // Радиус взаимодействия
//            {
//                closestDistance = distance;
//                closestZone = zone;
//            }
//        }

//        _depositZone = closestZone;
//    }
//}