using UnityEngine;

public class SimpleFlagInputHandler : MonoBehaviour
{
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
        if (Input.GetMouseButtonDown(0)) // ЛКМ // todo => move to field
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            // if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance))
            if (Physics.Raycast(ray, out var hit, _maxRaycastDistance) &&
                             hit.collider.TryGetComponent<BaseController>(out var baseController))
            {
                // if (hit.collider.TryGetComponent<BaseController>(out var baseController))
                //{
                SelectBase(baseController);
                // return;
                // }
            }
        }
    }

    private void HandleFlagPlacement()
    {
        if (_selectedBase == null || Input.GetMouseButtonDown(1) == false)// todo => move to field
            return;                 // ПКМ для установки флага

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _groundLayerMask))
            if (_selectedBase.CanBuildNewBase)
                _selectedBase.TrySetFlag(hit.point);
    }

    private void HandleBaseDeselection()
    {
        if (Input.GetKeyDown(KeyCode.Escape))// todo => move to field
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