using UnityEngine;
using System;
using System.Collections;

public class BaseConstructionManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BaseFactory _baseFactory;
    [SerializeField] private ResourceManager _resourceManager;

    [Header("Construction Settings")]
    [SerializeField] private float _constructionTime = 3f;
    [SerializeField] private GameObject _constructionSitePrefab;// TODO: на конкретный тип 1/2

    private Coroutine _currentConstructionCoroutine;
    private Coroutine _activateBotCoroutine;

    public event Action<BaseController> BaseConstructionStarted;
    public event Action<BaseController> BaseConstructionCompleted;

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


        // 1. Создаем строительную площадку
        constructionSite = CreateConstructionSite(flagPosition);

        // 2. Отправляем бота НА строительную площадку
        builderBot.ChangeState(new BotMovingToConstructionState(flagPosition));

        // 3. Ждем пока бот дойдет до места
        yield return new WaitUntil(() => builderBot.HasReachedDestination());

        // 4. Начинаем строительство
        builderBot.ChangeState(new BotBuildingState(_constructionTime));
        Debug.Log($"[Construction] Bot started building at {flagPosition}");

        // 5. Ждем завершения строительства
        yield return new WaitForSeconds(_constructionTime);

        // 6. Создаем новую базу
        newBase = _baseFactory.CreateBase(flagPosition);

        // 7. Передаем бота новой базе
        TransferBotToNewBase(builderBot, parentBase, newBase);

        // 8. Убираем строительную площадку
        if (constructionSite != null)
            Destroy(constructionSite);

        // УВЕДОМЛЯЕМ PriorityController о завершении строительства
        var priorityController = parentBase.GetComponent<BasePriorityController>();
        priorityController?.ResetConstructionFlag();

        Debug.Log($"[Construction] Base construction completed! New base: {newBase.name}");
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
        Debug.Log($"[Construction] Transferring bot {bot.name} from {fromBase.name} to {toBase.name}");

        if (bot == null || toBase == null)
        {
            Debug.LogError("[Construction] Cannot transfer bot - bot or target base is null");
            return;
        }

        try
        {
            // 1. Получаем BotManager новой базы
            var toBotManager = toBase.GetComponentInChildren<BotManager>();
            var fromBotManager = fromBase.GetComponentInChildren<BotManager>();

            if (toBotManager == null)
            {
                Debug.LogError("[Construction] Target BotManager not found!");
                return;
            }

            // 2. Используем новый метод перепривязки
            bot.ReassignToNewManager(toBotManager, fromBotManager);

            // 3. Удаляем бота из старой базы
            if (fromBotManager != null)
            {
                // Используем рефлексию для доступа к приватному списку _bots
                var botsField = typeof(BotManager).GetField("_bots",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (botsField != null)
                {
                    var botsList = (System.Collections.Generic.List<Bot>)botsField.GetValue(fromBotManager);
                    botsList?.Remove(bot);
                    Debug.Log($"[Construction] Bot removed from old base, remaining: {botsList?.Count ?? 0}");
                }
            }

            // 4. Добавляем бота в новую базу
            var toBotsField = typeof(BotManager).GetField("_bots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (toBotsField != null)
            {
                var toBotsList = (System.Collections.Generic.List<Bot>)toBotsField.GetValue(toBotManager);
                toBotsList?.Add(bot);
                Debug.Log($"[Construction] Bot added to new base, total: {toBotsList?.Count ?? 0}");
            }

            // 5. ВАЖНО: Меняем физический parent бота на BotManager новой базы
            if (toBotManager.transform != null)
            {
                bot.transform.SetParent(toBotManager.transform);
                Debug.Log($"[Construction] Bot parent changed to: {toBotManager.transform.name}");
            }

            // 6. Переводим бота в idle состояние
            bot.ChangeState(new BotIdleState());

            // 7. Активируем бота для сбора ресурсов в новой базе
            _activateBotCoroutine = StartCoroutine(ActivateBotAfterDelay(bot, toBotManager, 0.5f));// TODO:   magic num

            Debug.Log($"[Construction] Bot transfer completed successfully");
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