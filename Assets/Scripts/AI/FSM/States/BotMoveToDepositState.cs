using UnityEngine;

public class BotMoveToDepositState : BotBaseState
{
    private DepositZone depositZone;

    public BotMoveToDepositState(BotStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        depositZone = FindDepositZone();

        if (depositZone == null)
        {
            Debug.LogError("No deposit zone found!");
            ChangeState(BotState.Idle);
            return;
        }

      //  Debug.Log($"{BotController.gameObject.name} moving to deposit zone");
        BotController.MoveToPosition(depositZone.transform.position);
    }

    public override void Update()
    {
        if (depositZone == null)
        {
            ChangeState(BotState.Search);
            return;
        }

        if (BotController.HasReachedDestination())
        {
            ChangeState(BotState.Deposit);
        }
    }

    public override void FixedUpdate() { }

    public override void Exit() { }

    private DepositZone FindDepositZone()
    {
        DepositZone zone = GameObject.FindObjectOfType<DepositZone>();
        return zone;
    }
}