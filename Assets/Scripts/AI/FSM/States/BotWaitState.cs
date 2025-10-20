using UnityEngine;

public class BotWaitState : BotBaseState
{
    private float waitTime;
    private float currentWaitTime;

    public BotWaitState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        waitTime = Random.Range(0.5f, 1.5f);
        currentWaitTime = 0f;

        BotController.StopMovement();
        Debug.Log($"{BotController.gameObject.name} waiting for {waitTime} seconds");
    }

    public override void Update()
    {
        currentWaitTime += Time.deltaTime;

        if (currentWaitTime >= waitTime)
        {
            ChangeState(BotState.Search);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}