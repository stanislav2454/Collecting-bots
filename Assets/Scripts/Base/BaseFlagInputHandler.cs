using UnityEngine;

public class BaseFlagInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private float _maxRaycastDistance = 100f;

    private Camera _mainCamera;
    private BaseController _selectedBase;
    private BaseFlag _placementPreview;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleBaseSelection();
        HandleFlagPreview();
        HandleFlagPlacement();
        HandleBaseDeselection();
    }

    private void HandleBaseSelection()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance))
            {
                if (hit.collider.TryGetComponent<BaseController>(out var baseController))
                {
                    SelectBase(baseController);
                    return;
                }
            }
        }
    }

    private void HandleFlagPreview()
    {
        if (_selectedBase == null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance))
        {
            bool isGround = hit.collider.TryGetComponent<Ground>(out _);
            bool isValid = isGround && _selectedBase.CanBuildNewBase;

            UpdateOrCreatePreview(hit.point, isValid);
        }
        else
        {
            ClearPreview();
        }
    }

    private void HandleFlagPlacement()
    {
        if (_selectedBase == null)
            return;

        if (Input.GetMouseButtonDown(0)) // ЛКМ 
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance))
            {
                if (hit.collider.TryGetComponent<Ground>(out _))
                {
                    if (_selectedBase.CanBuildNewBase)
                    {
                        _selectedBase.TrySetFlag(hit.point);
                        ClearPreview();
                    }
                    else
                    {
                        Debug.LogWarning("🚫 Need at least 2 bots to place flag");
                    }
                }
            }
        }
    }

    private void HandleBaseDeselection()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            DeselectBase();
    }

    private void UpdateOrCreatePreview(Vector3 position, bool isValid)
    {
        if (_placementPreview == null)
            CreatePreview(position, isValid);
        else
            _placementPreview.UpdatePosition(position, isValid);
    }

    private void CreatePreview(Vector3 position, bool isValid)
    {
        if (_selectedBase.FlagPrefab != null)
        {
            _placementPreview = Instantiate(_selectedBase.FlagPrefab);
            _placementPreview.Initialize(_selectedBase, position, true);
            _placementPreview.gameObject.name = "FlagPlacementPreview";
            _placementPreview.UpdatePosition(position, isValid);
        }
    }

    private void SelectBase(BaseController baseController)
    {
        if (_selectedBase != baseController)
        {
            DeselectBase();
            _selectedBase = baseController;
            Debug.Log($"✅ Base selected: {_selectedBase.name}");
        }
    }

    private void DeselectBase()
    {
        if (_selectedBase != null)
        {
            Debug.Log("❌ Base deselected");
            _selectedBase = null;
        }
        ClearPreview();
    }

    private void ClearPreview()
    {
        if (_placementPreview != null)
        {
            Destroy(_placementPreview.gameObject);
            _placementPreview = null;
        }
    }

    private void OnDestroy()// 👈
    {
        ClearPreview();
    }
}