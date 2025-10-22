using UnityEngine;
using System.Collections.Generic;
using System;

public class BaseController : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private int _initialBotsCount = 3;
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private float _scanRadius = 20f;

    [Header("Dependencies")]
    [SerializeField] private ItemSpawner _itemSpawner;

    private List<BotController> _bots = new List<BotController>();
    private List<Item> _availableResources = new List<Item>();
    private int _collectedResources;
    private Coroutine _scanResources;

    public event Action<Item> ResourceScanned;
    public event Action<int> ResourceCollected;
    public event Action<BotController> BotAssigned;

    public int CollectedResources => _collectedResources;
    public IReadOnlyList<BotController> Bots => _bots;

    private void Start()
    {
        SpawnInitialBots();

        if (_itemSpawner != null)
            _itemSpawner.ItemSpawned += HandleItemSpawned;

        _scanResources = StartCoroutine(ScanForResourcesCoroutine());
    }

    private void OnDestroy()
    {
        if (_itemSpawner != null)
        {
            _itemSpawner.ItemSpawned -= HandleItemSpawned;
        }
    }

    private void HandleItemSpawned(Item item)
    {
        if (_availableResources.Contains(item) == false)
        {
            _availableResources.Add(item);
            ResourceScanned?.Invoke(item);
            AssignBotToResource(item);
        }
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < _initialBotsCount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                UnityEngine.Random.Range(-3f, 3f),
                0,
                UnityEngine.Random.Range(-3f, 3f)
            );

            GameObject botObject = Instantiate(_botPrefab, spawnPosition, Quaternion.identity);
            if (botObject.TryGetComponent(out BotController bot))
            {
                InitializeBot(bot);
            }
        }
    }

    private void InitializeBot(BotController bot)
    {
        bot.AssignBase(this);
        bot.MissionCompleted += HandleBotMissionCompleted;
        _bots.Add(bot);
        BotAssigned?.Invoke(bot);
    }

    private System.Collections.IEnumerator ScanForResourcesCoroutine()
    {
        while (true)
        {
            ScanForResources();
            yield return new WaitForSeconds(2f);
        }
    }

    private void ScanForResources()
    {// Теперь используем событийную модель вместо Physics.OverlapSphere
        // Сканирование происходит через события от ItemSpawner

        //// Временная реализация - заменим на событийную в следующем шаге
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius);
        //foreach (var collider in hitColliders)
        //{
        //    if (collider.TryGetComponent(out Item item) && item.CanBeCollected)
        //    {
        //        if (_availableResources.Contains(item)==false)
        //        {
        //            _availableResources.Add(item);
        //            ResourceScanned?.Invoke(item);
        //            AssignBotToResource(item);
        //        }
        //    }
        //}
    }

    private void AssignBotToResource(Item resource)
    {
        foreach (var bot in _bots)
        {
            if (bot.IsAvailable)
            {
                bot.AssignResource(resource);
                return;
            }
        }
    }

    private void HandleBotMissionCompleted(BotController bot, bool success)
    {
        if (success)
        {
            _collectedResources++;
            ResourceCollected?.Invoke(_collectedResources);
        }

        // Ищем новое задание для бота
        AssignNextResourceToBot(bot);
    }

    private void AssignNextResourceToBot(BotController bot)
    {
        foreach (var resource in _availableResources)
        {
            if (resource != null && resource.CanBeCollected)
            {
                bot.AssignResource(resource);
                return;
            }
        }

        // Нет доступных ресурсов - бот ждет
        bot.SetWaiting();
    }
}