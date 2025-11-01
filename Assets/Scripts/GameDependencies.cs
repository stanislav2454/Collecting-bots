using UnityEngine;

public class GameDependencies : MonoBehaviour, IGameDependencies
{
    [Header("Core Managers")]
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private BaseGenerator _baseGenerator;
    [SerializeField] private BaseSelectionManager _baseSelectionManager;

    // Реализация IGameDependencies
    public ResourceManager ResourceManager => _resourceManager;
    public ItemSpawner ItemSpawner => _itemSpawner;
    public BaseGenerator BaseGenerator => _baseGenerator;
    public BaseSelectionManager BaseSelectionManager => _baseSelectionManager;

    // Static instance для удобного доступа (не синглтон в чистом виде)
    private static GameDependencies _instance;
    public static GameDependencies Instance => _instance;

    private void Awake()
    {
        InitializeInstance();
        ValidateDependencies();
    }

    private void InitializeInstance()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple GameDependencies instances detected. Destroying: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Debug.Log("GameDependencies initialized");
    }

    private void ValidateDependencies()
    {
        if (_resourceManager == null)
            Debug.LogError("ResourceManager not assigned in GameDependencies!");

        if (_itemSpawner == null)
            Debug.LogWarning("ItemSpawner not assigned in GameDependencies!");

        if (_baseGenerator == null)
            Debug.LogWarning("BaseGenerator not assigned in GameDependencies!");

        if (_baseSelectionManager == null)
            Debug.LogWarning("BaseSelectionManager not assigned in GameDependencies!");
    }

    // Валидация в редакторе
    private void OnValidate()
    {
        if (Application.isPlaying) 
            return;

        ValidateDependencies();
    }

    // Очистка при уничтожении
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            Debug.Log("GameDependencies destroyed");
        }
    }
}