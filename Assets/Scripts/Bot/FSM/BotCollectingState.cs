using UnityEngine;

public class BotCollectingState : BotState
{
    private Item _targetResource;
    private Vector3 _basePosition;
    private float _baseRadius;
    private float _collectionTimer;

    public override BotStateType StateType => BotStateType.Collecting;

    public BotCollectingState(Item resource, Vector3 basePosition, float baseRadius)
    {
        _targetResource = resource;
        _basePosition = basePosition;
        _baseRadius = baseRadius;
    }

    public override void Enter(Bot bot)
    {
        if (IsResourceValid(_targetResource) == false)
        {
            bot.CompleteMission(false);
            return;
        }

        bot.StopMovement();

        if (_targetResource != null)
            bot.transform.LookAt(_targetResource.transform);

        _collectionTimer = 0f;
    }

    public override void Update(Bot bot)
    {
        if (IsResourceValid(_targetResource) == false)
        {
            bot.CompleteMission(false);
            return;
        }

        _collectionTimer += Time.deltaTime;

        if (_collectionTimer >= bot.CollectionDuration)
            CompleteCollection(bot);
    }

    private void CompleteCollection(Bot bot)
    {
        if (IsResourceValid(_targetResource) && bot.Inventory.TryAddItem(_targetResource))
        {
            _targetResource.Collect();
            bot.ChangeState(new BotReturningToBaseState(_basePosition, _baseRadius));
        }
        else
        {
            bot.CompleteMission(false);
        }
    }
}
