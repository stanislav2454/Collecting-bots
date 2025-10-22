using UnityEngine;

public class GameIntegrationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseController _baseController;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private SimpleCameraController _cameraController;

    [Header("UI Debug")]
    [SerializeField] private bool _showDebugUI = true;

    private void Start()
    {
        SetupEventConnections();
        Debug.Log("🎮 Система ботов инициализирована");
    }

    private void SetupEventConnections()
    {
        if (_baseController != null)
        {
            _baseController.ResourceCollected += HandleResourceCollected;
            _baseController.BotAssigned += HandleBotAssigned;
        }

        if (_itemSpawner != null)
        {
            _itemSpawner.ItemSpawned += HandleItemSpawned;
        }
    }

    private void HandleResourceCollected(int totalResources)
    {
        Debug.Log($"📦 Ресурс доставлен! Всего: {totalResources}");
    }

    private void HandleBotAssigned(BotController bot)
    {
        Debug.Log($"🤖 Бот {bot.gameObject.name} активирован");
    }

    private void HandleItemSpawned(Item item)
    {
        Debug.Log($"🔄 Ресурс создан: {item.gameObject.name}");
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (_baseController != null)
        {
            _baseController.ResourceCollected -= HandleResourceCollected;
            _baseController.BotAssigned -= HandleBotAssigned;
        }
    }
}