using UnityEngine;

public class FlagInputHandler : MonoBehaviour
{
    [Header("InputKeys")]
    [SerializeField] private KeyCode _selectBase = KeyCode.Mouse0;
    [SerializeField] private KeyCode _flagPlacement = KeyCode.Mouse1;
    [SerializeField] private KeyCode _baseDeselection = KeyCode.Escape;

    [Space(5)]
    [Header("Input Settings")]
    [SerializeField] private float _maxRaycastDistance = 100f;
    [SerializeField] private LayerMask _groundLayerMask;

    private Camera _mainCamera;
    private BaseController _selectedBase;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleBaseSelection();
        HandleFlagPlacement();
        HandleBaseDeselection();
    }

    private void OnDestroy()
    {
        DeselectBase();
    }

    private void HandleBaseSelection()
    {
        if (Input.GetKeyDown(_selectBase))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, _maxRaycastDistance) &&
                                hit.collider.TryGetComponent<BaseController>(out var baseController))
                SelectBase(baseController);
        }
    }

    private void HandleFlagPlacement()
    {
        if (_selectedBase == null || Input.GetKeyDown(_flagPlacement) == false)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _groundLayerMask))
            if (_selectedBase.CanBuildNewBase)
                _selectedBase.TrySetFlag(hit.point);
    }

    private void HandleBaseDeselection()
    {
        if (Input.GetKeyDown(_baseDeselection))
            DeselectBase();
    }

    private void SelectBase(BaseController baseController)
    {
        if (_selectedBase == baseController)
            return;

        DeselectBase();
        _selectedBase = baseController;
        _selectedBase.SelectBase();
    }

    private void DeselectBase()
    {
        _selectedBase?.DeselectBase();
        _selectedBase = null;
    }
}