using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private ResourceAllocator _resourceAllocator;
    [SerializeField] private BaseConstructor _baseConstructor;
    [SerializeField] private BaseSelector _baseSelector;
    [SerializeField] private InputController _inputController;

    [Header("Initial Setup")]
    [SerializeField] private BaseController _initialBase;

    private void Start()
    {
        InitializeGame();
        SetupInputHandlers();
    }

    private void InitializeGame()
    {
        if (_itemSpawner == null)
        {
            Debug.LogError("ItemSpawner not assigned in GameController!");
            return;
        }

        if (_resourceAllocator == null)
        {
            Debug.LogError("ResourceAllocator not assigned in GameController!");
            return;
        }

        if (_baseConstructor == null)
        {
            Debug.LogError("BaseConstructor not assigned in GameController!");
            return;
        }

        if (_baseSelector == null)
        {
            Debug.LogError("BaseSelector not assigned in GameController!");
            return;
        }

        if (_inputController == null)
        {
            Debug.LogError("InputController not assigned in GameController!");
            return;
        }

        if (_initialBase == null)
        {
            Debug.LogError("InitialBase not assigned in GameController!");
            return;
        }

        _initialBase.Initialize(_itemSpawner, _resourceAllocator);
        _baseSelector.RegisterBase(_initialBase);

        Debug.Log("GameController: Game initialized successfully!");
    }

    private void SetupInputHandlers()
    {
        _inputController.GroundInteracted += OnGroundInteracted;
        _inputController.FlagInteracted += OnFlagInteracted;

        Debug.Log("GameController: Input handlers setup completed");
    }

    private void OnGroundInteracted(Vector3 groundPoint)
    {
        var selectedBase = _baseSelector.CurrentlySelectedBase;
        if (selectedBase != null && selectedBase.CanBuildNewBase)
        {
            selectedBase.TrySetFlag(groundPoint);
        }
    }

    private void OnFlagInteracted(Flag flag)
    {
        flag.HandleClickInteraction();
    }

    private void OnDestroy()
    {
        if (_inputController != null)
        {
            _inputController.GroundInteracted -= OnGroundInteracted;
            _inputController.FlagInteracted -= OnFlagInteracted;
        }
    }
}