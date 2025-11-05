using UnityEngine;
using System;
using System.Collections;

public class BaseConstructionManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseFactory _baseFactory;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private BaseSelectionManager _selectionManager;

    [Header("Construction Settings")]
    [SerializeField] private float _constructionTime = 3f;
    [SerializeField] private GameObject _constructionSitePrefab;// TODO: на конкретный тип 1/2

    private Coroutine _currentConstructionCoroutine;
    private Coroutine _activateBotCoroutine;

    public event Action<BaseController> BaseConstructionStarted;
    public event Action<BaseController> BaseConstructionCompleted;

    private void OnDestroy()
    {
        if (_currentConstructionCoroutine != null)
            StopCoroutine(_currentConstructionCoroutine);

        if (_activateBotCoroutine != null)
            StopCoroutine(_activateBotCoroutine);
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
        GameObject constructionSite = null;// TODO: на конкретный тип
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

    private GameObject CreateConstructionSite(Vector3 position)// TODO: на конкретный тип 2/2
    {
        if (_constructionSitePrefab != null)
            return Instantiate(_constructionSitePrefab, position, Quaternion.identity);

        var site = GameObject.CreatePrimitive(PrimitiveType.Cube);
        site.transform.position = position;
        site.transform.localScale = new Vector3(2f, 0.1f, 2f);
        site.name = "ConstructionSite";
        return site;
    }

    private void TransferBotToNewBase(Bot bot, BaseController fromBase, BaseController toBase)
    {
        const float Delay = 0.5f;

        if (bot == null || toBase == null)
            return;

        try
        {
            var toBotManager = toBase.GetComponentInChildren<BotManager>();
            var fromBotManager = fromBase.GetComponentInChildren<BotManager>();

            if (toBotManager == null)
                return;

            bot.ReassignToNewManager(toBotManager, fromBotManager);

            if (fromBotManager != null)
            {
                var botsField = typeof(BotManager).GetField("_bots",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (botsField != null)
                {
                    var botsList = (System.Collections.Generic.List<Bot>)botsField.GetValue(fromBotManager);
                    botsList?.Remove(bot);
                }
            }

            var toBotsField = typeof(BotManager).GetField("_bots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (toBotsField != null)
            {
                var toBotsList = (System.Collections.Generic.List<Bot>)toBotsField.GetValue(toBotManager);
                toBotsList?.Add(bot);
            }

            if (toBotManager.transform != null)
                bot.transform.SetParent(toBotManager.transform);

            bot.ChangeState(new BotIdleState());

            _activateBotCoroutine = StartCoroutine(ActivateBotAfterDelay(bot, toBotManager, Delay));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Construction] Bot transfer failed: {e.Message}");
            bot.ChangeState(new BotIdleState());
        }
    }

    private IEnumerator ActivateBotAfterDelay(Bot bot, BotManager targetBotManager, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bot != null && targetBotManager != null)
            targetBotManager.AssignResourceToBot(bot);
    }
}