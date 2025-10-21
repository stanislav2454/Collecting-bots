using UnityEngine;

public class BotWaitState : BotBaseState
{
    private float _waitTimer = 0f;
    private float _waitDuration = 0f;

    public BotWaitState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        _waitDuration = Random.Range(3f, 8f);
        _waitTimer = 0f;

        Debug.Log($"{BotController.gameObject.name} waiting for {_waitDuration:F1} seconds");
        BotController.StopMovement();
    }

    public override void Update()
    {
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitDuration)
        {
            Debug.Log($"{BotController.gameObject.name} finished waiting, resuming search");
            ChangeState(BotState.Search);
        }
    }

    public override void FixedUpdate()
    {
        // Не используется в этом состоянии
    }

    public override void Exit()
    {
        _waitTimer = 0f;
        _waitDuration = 0f;
    }
}
//using UnityEngine;

//public class BotWaitState : BotBaseState
//{
//    private float waitTime;
//    private float currentWaitTime;

//    public BotWaitState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        waitTime = Random.Range(0.5f, 1.5f);
//        currentWaitTime = 0f;

//        BotController.StopMovement();
//        Debug.Log($"{BotController.gameObject.name} waiting for {waitTime} seconds");
//    }

//    public override void Update()
//    {
//        currentWaitTime += Time.deltaTime;

//        if (currentWaitTime >= waitTime)
//        {
//            ChangeState(BotState.Search);
//        }
//    }

//    public override void FixedUpdate() { }

//    public override void Exit() { }
//}