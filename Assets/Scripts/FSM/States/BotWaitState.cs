//using UnityEngine;

//public class BotWaitState : BotBaseState
//{
//    private float _waitTimer = 0f;
//    private float _waitDuration = 0f;

//    public BotWaitState(BotStateMachine stateMachine) : base(stateMachine) { }

//    public override void Enter()
//    {
//        _waitDuration = Random.Range(3f, 8f);
//        _waitTimer = 0f;

//        BotController.StopMovement();
//    }

//    public override void Update()
//    {
//        _waitTimer += Time.deltaTime;

//        if (_waitTimer >= _waitDuration)
//            ChangeState(BotState.Search);
//    }

//    public override void FixedUpdate()
//    {
//        // Не используется в этом состоянии
//    }

//    public override void Exit()
//    {
//        _waitTimer = 0f;
//        _waitDuration = 0f;
//    }
//}