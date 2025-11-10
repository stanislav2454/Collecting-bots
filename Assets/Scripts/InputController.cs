using UnityEngine;

public class InputController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode _selectButton = KeyCode.Mouse0;
    [SerializeField] private KeyCode _interactButton = KeyCode.Mouse1;

    [Header("Dependencies")]
    [SerializeField] private Raycaster _raycaster;
    [SerializeField] private BaseSelector _baseSelector;

    public event System.Action<Vector3> GroundInteracted;
    public event System.Action<BaseController> BaseSelected;
    public event System.Action<Flag> FlagInteracted;

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetKeyDown(_selectButton))
            HandleSelectionClick();

        if (Input.GetKeyDown(_interactButton))
            HandleInteractionClick();
    }

    private void HandleSelectionClick()
    {
        if (_raycaster.TryGetFlagUnderMouse(out Flag flag))
        {
            Debug.Log($"InputController: Flag clicked - {flag.name}");
            FlagInteracted?.Invoke(flag);
        }
        else if (_raycaster.TryGetBaseUnderMouse(out BaseController baseController))
        {
            Debug.Log($"InputController: Base selected - {baseController.name}");
            BaseSelected?.Invoke(baseController);
            _baseSelector.SelectBase(baseController);
        }
        else
        {
            Debug.Log("InputController: Deselecting all");
            _baseSelector.DeselectCurrentBase();
        }
    }

    private void HandleInteractionClick()
    {
        if (_raycaster.TryGetGroundUnderMouse(out Vector3 groundPoint))
        {
            Debug.Log($"InputController: Ground interaction at {groundPoint}");
            GroundInteracted?.Invoke(groundPoint);
        }
    }

    public void SetDependencies(Raycaster raycaster, BaseSelector baseSelector)
    {
        _raycaster = raycaster;
        _baseSelector = baseSelector;
    }
}