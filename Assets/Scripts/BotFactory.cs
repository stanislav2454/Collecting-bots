using System;
using UnityEngine;

public class BotFactory
{
    private readonly Bot _botPrefab;
    private readonly Transform _spawnContainer;
    private readonly float _spawnRadius;

    public event Action<Bot> BotCreated;

    public BotFactory(Bot botPrefab, Transform spawnContainer, float spawnRadius)
    {
        _botPrefab = botPrefab;
        _spawnContainer = spawnContainer;
        _spawnRadius = spawnRadius;
    }

    public Bot CreateBot(Vector3 basePosition)
    {
        if (_botPrefab == null)
        {
            Debug.LogError("Bot prefab is not assigned in BotFactory!");
            return null;
        }

        Vector3 spawnPosition = GetSpawnPosition(basePosition);
        Bot newBot = UnityEngine.Object.Instantiate(_botPrefab, spawnPosition, Quaternion.identity, _spawnContainer);

        BotCreated?.Invoke(newBot);
        Debug.Log($"Bot created at position: {spawnPosition}");

        return newBot;
    }

    private Vector3 GetSpawnPosition(Vector3 basePosition)
    {
        return basePosition + new Vector3(
            UnityEngine.Random.Range(-_spawnRadius, _spawnRadius),
            0,
            UnityEngine.Random.Range(-_spawnRadius, _spawnRadius));
    }
}