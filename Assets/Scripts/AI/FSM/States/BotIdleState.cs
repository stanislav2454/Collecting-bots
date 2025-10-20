using UnityEngine;

public class BotIdleState : BotBaseState
{
    private float idleTime;
    private float currentIdleTime;

    public BotIdleState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        idleTime = Random.Range(1f, 3f); // Случайное время ожидания
        currentIdleTime = 0f;

        BotController.StopMovement();
       // Debug.Log($"{BotController.gameObject.name} is idling for {idleTime} seconds");
    }

    public override void Update()
    {
        currentIdleTime += Time.deltaTime;

        // После ожидания переходим к поиску предметов
        if (currentIdleTime >= idleTime)
        {
            ChangeState(BotState.Search);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }
}