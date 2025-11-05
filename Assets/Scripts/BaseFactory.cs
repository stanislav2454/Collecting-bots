using System.Collections;
using UnityEngine;

public class BaseFactory : MonoBehaviour
{
    [SerializeField] private BaseController _basePrefab;
    [SerializeField] private Transform _basesContainer;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private ItemSpawner _itemSpawner;
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
        if (_resourceManager == null)
            _resourceManager = FindAnyObjectByType<ResourceManager>();//TODO:

        if (_itemSpawner == null)
            _itemSpawner = FindAnyObjectByType<ItemSpawner>();//TODO:

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        // 1. Настраиваем BaseController новой базы
        var baseController = newBase.GetComponent<BaseController>();
        if (baseController != null)        
            SetupBaseController(baseController, _itemSpawner);        

        // 2. Настраиваем BotManager новой базы
        var botManager = newBase.GetComponentInChildren<BotManager>();
        if (botManager != null)
        {
            botManager.SetResourceManager(_resourceManager);
            _botAssignment = StartCoroutine(StartBotAssignmentsAfterDelay(botManager, 1f));// ЗАПУСКАЕМ автоматическое назначение ресурсов ботам
        }

        // 3. Настраиваем MissionControl новой базы
        var missionControl = newBase.GetComponent<MissionControl>();
        if (missionControl != null)        
            SetupMissionControl(missionControl, _resourceManager, botManager);        

        // 4. Настраиваем CanvasLookAtCamera
        var canvasLookAt = newBase.GetComponentInChildren<CanvasLookAtCamera>();
        if (canvasLookAt != null && _mainCamera != null)// Через рефлексию или добавим публичный метод        
            SetCameraOnCanvasLookAt(canvasLookAt, _mainCamera);        

        // Регистрируем базу в менеджерах
        var selectionManager = FindAnyObjectByType<BaseSelectionManager>();
        selectionManager?.RegisterBase(newBase);

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

    private void SetCameraOnCanvasLookAt(CanvasLookAtCamera canvasLookAt, Camera camera)
    {
        // Если нет публичного метода, используем рефлексию или добавь метод в CanvasLookAtCamera
        var cameraField = typeof(CanvasLookAtCamera).GetField("_targetCamera",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (cameraField != null)
            cameraField.SetValue(canvasLookAt, camera);
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

    public void SetBasePrefab(BaseController prefab) =>
        _basePrefab = prefab;
}