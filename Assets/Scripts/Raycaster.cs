using UnityEngine;

public class Raycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask _groundLayerMask = 1 << 3;
    [SerializeField] private LayerMask _baseLayerMask = 1 << 9;
    [SerializeField] private float _maxRaycastDistance = 100f;

    private Camera _mainCamera;

    public LayerMask GroundLayerMask => _groundLayerMask;
    public LayerMask BaseLayerMask => _baseLayerMask;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public bool RaycastFromMouse(out RaycastHit hit, LayerMask layerMask)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, _maxRaycastDistance, layerMask);
    }

    public bool RaycastFromMouse(out RaycastHit hit)
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, _maxRaycastDistance);
    }

    public bool TryGetBaseUnderMouse(out BaseController baseController)
    {
        baseController = null;
        if (RaycastFromMouse(out RaycastHit hit, _baseLayerMask))
            return hit.collider.TryGetComponent(out baseController);

        return false;
    }

    public bool TryGetGroundUnderMouse(out Vector3 groundPoint)
    {
        groundPoint = Vector3.zero;
        if (RaycastFromMouse(out RaycastHit hit, _groundLayerMask))
        {
            groundPoint = hit.point;
            return true;
        }

        return false;
    }
}