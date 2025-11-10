using UnityEngine;
using System;
using System.Collections;

public class BaseConstructor : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseFactory _baseFactory;
    [SerializeField] private ResourceAllocator _resourceAllocator;
    [SerializeField] private BaseSelector _baseSelector;

    [Header("Construction Settings")]
    [SerializeField] private float _constructionTime = 3f;
    [SerializeField] private ConstructionSite _constructionSitePrefab;

    private Coroutine _currentConstructionCoroutine;

    public event Action<BaseController> BaseConstructionStarted;
    public event Action<BaseController> BaseConstructionCompleted;

    private void OnDestroy()
    {
        if (_currentConstructionCoroutine != null)
            StopCoroutine(_currentConstructionCoroutine);

        BaseConstructionStarted = null;
        BaseConstructionCompleted = null;
    }

    public void StartBaseConstruction(BaseController parentBase, Vector3 flagPosition, Bot builderBot)
    {
        if (parentBase == null || builderBot == null)
        {
            Debug.LogError("BaseConstructionManager: Invalid construction parameters!");
            return;
        }

        if (_currentConstructionCoroutine != null)
            StopCoroutine(_currentConstructionCoroutine);

        _currentConstructionCoroutine = StartCoroutine(ConstructionProcess(parentBase, flagPosition, builderBot));
    }

    private IEnumerator ConstructionProcess(BaseController parentBase, Vector3 flagPosition, Bot builderBot)
    {
        GameObject constructionSite = null;
        BaseController newBase = null;

        constructionSite = CreateConstructionSite(flagPosition);
        builderBot.ChangeState(new BotMovingToConstructionState(flagPosition));

        yield return new WaitUntil(() => builderBot.HasReachedDestination());

        builderBot.ChangeState(new BotBuildingState(_constructionTime));

        yield return new WaitForSeconds(_constructionTime);

        newBase = _baseFactory.CreateBase(flagPosition);

        TransferBotToNewBase(builderBot, parentBase, newBase);

        if (constructionSite != null)
            Destroy(constructionSite);

        var priorityController = parentBase.GetComponent<BasePriorityController>();
        priorityController?.ResetConstructionFlag();

        BaseConstructionCompleted?.Invoke(newBase);
        _currentConstructionCoroutine = null;
    }

    private GameObject CreateConstructionSite(Vector3 position)
    {
        if (_constructionSitePrefab != null)
            return Instantiate(_constructionSitePrefab.gameObject, position, Quaternion.identity);

        var site = GameObject.CreatePrimitive(PrimitiveType.Cube);
        site.transform.position = position;
        site.transform.localScale = new Vector3(2f, 0.1f, 2f);
        site.name = "ConstructionSite";
        return site;
    }

    private void TransferBotToNewBase(Bot bot, BaseController fromBase, BaseController toBase)
    {
        if (bot == null || toBase == null)
            return;

        try
        {
            var toBotController = toBase.GetComponentInChildren<BotController>();
            var fromBotController = fromBase.GetComponentInChildren<BotController>();

            if (toBotController != null)
            {
                fromBotController.TransferBotToNewController(bot, toBotController);
                bot.TransferToNewParent(toBotController.transform);
                bot.ChangeState(new BotIdleState());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Construction] Bot transfer failed: {e.Message}");
            bot.ChangeState(new BotIdleState());
        }
    }
}