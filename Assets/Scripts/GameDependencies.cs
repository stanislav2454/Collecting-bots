using UnityEngine;

public class GameDependencies : MonoBehaviour, IGameDependencies
{
    [Header("Core Managers")]
    [SerializeField] private ResourceManager _resourceManager;// оптимизировать
    public ResourceManager ResourceManager => _resourceManager;// оптимизировать

    [SerializeField] private ItemSpawner _itemSpawner;// оптимизировать
    public ItemSpawner ItemSpawner => _itemSpawner;// оптимизировать

    [SerializeField] private BaseGenerator _baseGenerator;// оптимизировать
    public BaseGenerator BaseGenerator => _baseGenerator;// оптимизировать

    [SerializeField] private BaseSelectionManager _baseSelectionManager;// оптимизировать
    public BaseSelectionManager BaseSelectionManager => _baseSelectionManager;// оптимизировать

    [Header("Base Systems")]
    [SerializeField] private BaseResourceController _baseResourceController;// оптимизировать
    public BaseResourceController BaseResourceController => _baseResourceController;// оптимизировать


    private static GameDependencies _instance;// оптимизировать
    public static GameDependencies Instance => _instance;// оптимизировать

    private void Awake()
    {
        InitializeInstance();
        ValidateDependencies();
    }

    // Метод для ручной установки зависимостей (полезно для тестов)
    public void SetBaseResourceController(BaseResourceController controller)
    {
        _baseResourceController = controller;
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

        if (_baseResourceController == null)
            Debug.LogWarning("BaseResourceController not assigned in GameDependencies!");
    }

    private void OnValidate()
    {
        if (Application.isPlaying) 
            return;

        ValidateDependencies();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            Debug.Log("GameDependencies destroyed");
        }
    }
}