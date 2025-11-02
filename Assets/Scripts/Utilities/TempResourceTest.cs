using UnityEngine;

public class TempResourceTest : MonoBehaviour
{// Добавить скрипт на любой объект на сцене
    private void Update()
    {
        // Тест доступа через GameDependencies - нажмите R для проверки
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestResourceAccess();
        }

        // Тест добавления ресурсов - нажмите T для проверки  
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestAddResources();
        }
    }

    private void TestResourceAccess()
    {
        if (GameDependencies.Instance == null)
        {
            Debug.LogError("GameDependencies instance is null!");
            return;
        }

        var resourceController = GameDependencies.Instance.BaseResourceController;
        if (resourceController == null)
        {
            Debug.LogError("BaseResourceController is null in GameDependencies!");
            return;
        }

        Debug.Log($"✅ SUCCESS: Accessed BaseResourceController via GameDependencies!");
        Debug.Log($"Current resources: {resourceController.CollectedResources}");
        Debug.Log($"Can afford bot (3): {resourceController.CanAfford(3)}");
        Debug.Log($"Can afford base (5): {resourceController.CanAfford(5)}");
    }

    private void TestAddResources()
    {
        var resourceController = GameDependencies.Instance?.BaseResourceController;
        if (resourceController != null)
        {
            resourceController.AddResource(2);
            Debug.Log($"✅ Added 2 resources via GameDependencies. Total: {resourceController.CollectedResources}");
        }
    }
}