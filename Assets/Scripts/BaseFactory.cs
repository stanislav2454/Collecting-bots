using System.Collections;
using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    [Header("Base Prefab")]
    [SerializeField] private BaseController _basePrefab;
    [SerializeField] private Transform _basesContainer;

    [Header("Dependencies")]
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private BaseSelectionManager _selectionManager;
    [SerializeField] private BaseConstructionManager _constructionManager;
    [SerializeField] private Camera _mainCamera;

    private Coroutine _botAssignment;

    public BaseController CreateBase(Vector3 position)
    {
        if (_basePrefab == null)
        {
            Debug.LogError("Base prefab is not assigned in BaseFactory!");
            return null;
        }

        BaseController newBase = Instantiate(_basePrefab, position, Quaternion.identity, _basesContainer);
        newBase.name = $"Base_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        SetupNewBaseDependencies(newBase);

        return newBase;
    }

    private void SetupNewBaseDependencies(BaseController newBase)
    {
        const float Delay = 1f;

        if (_resourceManager == null)
        {
            Debug.LogError("ResourceManager not assigned in BaseFactory!");
            return;
        }

        if (_itemSpawner == null)
        {
            Debug.LogError("ItemSpawner not assigned in BaseFactory!");
            return;
        }

        if (_mainCamera == null)
        {
            Debug.LogWarning("Main camera not assigned in BaseFactory, using Camera.main");
            _mainCamera = Camera.main;
        }

        // 1. Настраиваем BaseController новой базы
        var baseController = newBase.GetComponent<BaseController>();
        if (baseController != null)
            SetupBaseController(baseController, _itemSpawner);

        // 2. Настраиваем BotManager новой базы
        var botManager = newBase.GetComponentInChildren<BotManager>();
        if (botManager != null)
        {
            botManager.SetResourceManager(_resourceManager);
            _botAssignment = StartCoroutine(StartBotAssignmentsAfterDelay(botManager, Delay));// ЗАПУСКАЕМ автоматическое назначение ресурсов ботам
        }

        // 3. Настраиваем MissionControl новой базы
        var missionControl = newBase.GetComponent<MissionControl>();
        if (missionControl != null)
            SetupMissionControl(missionControl, _resourceManager, botManager);

        // 4. Настраиваем CanvasLookAtCamera
        var canvasLookAt = newBase.GetComponentInChildren<CanvasLookAtCamera>();
        if (canvasLookAt != null && _mainCamera != null)// Через рефлексию или добавим публичный метод        
            canvasLookAt.SetTargetCamera(_mainCamera);
        //SetCameraOnCanvasLookAt(canvasLookAt, _mainCamera);

        // 5. Регистрируем базу в менеджерах
        if (_selectionManager != null)
            _selectionManager.RegisterBase(newBase);
        else
            Debug.LogWarning("BaseSelectionManager not assigned in BaseFactory!");

        Debug.Log($"[BaseFactory] New base dependencies setup completed");
    }


    private void SetupBaseController(BaseController baseController, ItemSpawner itemSpawner)
    {
        if (baseController == null || itemSpawner == null)
            return;

        var itemSpawnerField = typeof(BaseController).GetField("_itemSpawner",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (itemSpawnerField != null)
            itemSpawnerField.SetValue(baseController, itemSpawner);
    }


    private void SetupMissionControl
        (MissionControl missionControl, ResourceManager resourceManager, BotManager botManager)
    {
        if (missionControl == null)
            return;

        var setDependenciesMethod = typeof(MissionControl).GetMethod("SetDependencies");

        if (setDependenciesMethod != null)
        {
            setDependenciesMethod.Invoke(missionControl, new object[] { botManager, resourceManager });
        }
        else
        {
            var resourceManagerField = typeof(MissionControl).GetField("_resourceManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var botManagerField = typeof(MissionControl).GetField("_botManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (resourceManagerField != null)
                resourceManagerField.SetValue(missionControl, resourceManager);

            if (botManagerField != null)
                botManagerField.SetValue(missionControl, botManager);
        }
    }

    private IEnumerator StartBotAssignmentsAfterDelay(BotManager botManager, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (botManager != null)
        {
            var botsField = typeof(BotManager).GetField("_bots",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (botsField != null)
            {
                var botsList = (System.Collections.Generic.List<Bot>)botsField.GetValue(botManager);
                if (botsList != null)
                    foreach (var bot in botsList)
                        if (bot != null && bot.IsAvailable)
                            botManager.AssignResourceToBot(bot);
            }
        }
    }
}