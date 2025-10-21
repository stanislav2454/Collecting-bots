using UnityEngine;

public class BotIdleState : BotBaseState
{
    private float idleTime;
    private float currentIdleTime;

    public BotIdleState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        idleTime = Random.Range(1f, 3f);
        currentIdleTime = 0f;

        BotController.StopMovement();
    }

    public override void Update()
    {
        currentIdleTime += Time.deltaTime;

        if (currentIdleTime >= idleTime)
            ChangeState(BotState.Search);
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}