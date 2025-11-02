using UnityEngine;
using System.Collections.Generic;

public class BaseGenerator : MonoBehaviour
{
    [Header("Base Generation Settings")]
    [SerializeField] private BaseController _basePrefab;
    [SerializeField] private Transform _basesContainer;
    [SerializeField] private BaseController _initialBase;

    private List<BaseController> _allBases = new List<BaseController>();// оптимизировать
    private static BaseGenerator _instance;// оптимизировать

    public static BaseGenerator Instance => _instance;// оптимизировать
    public IReadOnlyList<BaseController> AllBases => _allBases;// оптимизировать

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_initialBase != null)
            _allBases.Add(_initialBase);
    }

    public BaseController CreateNewBase(Vector3 position, Bot builderBot, BaseController parentBase)
    {
        if (_basePrefab == null)
        {
            Debug.LogError("Base prefab not assigned in BaseGenerator!");
            return null;
        }

        BaseController newBase = Instantiate(_basePrefab, position, Quaternion.identity, _basesContainer);
        newBase.name = $"Base_{_allBases.Count + 1}";// доделать

        _allBases.Add(newBase);

        InitializeNewBase(newBase, builderBot, parentBase);

        Debug.Log($"New base created at: {position}");
        return newBase;
    }

    private void InitializeNewBase(BaseController newBase, Bot builderBot, BaseController parentBase)
    {
        if (builderBot != null && parentBase != null)
        {
            var newBaseBotManager = newBase.GetComponentInChildren<BotManager>();
            var parentBotManager = parentBase.GetBotManager();

            if (newBaseBotManager != null && parentBotManager != null)
            {
                parentBotManager.TransferBotToNewBase(builderBot, newBase);
                newBaseBotManager.AddExistingBot(builderBot);
            }
        }

        var itemCounter = newBase.GetComponentInChildren<ItemCounter>();
        if (itemCounter != null)
        {
            itemCounter.Reset();
        }

        Debug.Log($"New base initialized with bot: {builderBot?.name ?? "none"}");
    }

    public void UnregisterBase(BaseController baseController)
    {
        _allBases.Remove(baseController);
    }
}