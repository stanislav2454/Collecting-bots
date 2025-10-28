using UnityEngine;

public abstract class BotState
{
    public abstract BotStateType StateType { get; }
    public abstract void Enter(Bot bot);
    public abstract void Update(Bot bot);
    public virtual void Exit(Bot bot) { }

    protected bool IsResourceValid(Item resource) =>
         resource != null && resource.gameObject.activeInHierarchy;

    protected bool CheckDestinationReached(Bot bot, ref float checkTimer, float checkInterval = 0.2f)
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            return bot.HasReachedDestination();
        }

        return false;
    }

    protected bool IsAtBase(Bot bot, Vector3 basePosition, float baseRadius)
    {
        float sqrDistanceToBase = (bot.transform.position - basePosition).sqrMagnitude;
        float baseRadiusSqr = baseRadius * baseRadius;
        return sqrDistanceToBase <= baseRadiusSqr;
    }
}
