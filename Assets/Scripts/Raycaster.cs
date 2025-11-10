using UnityEngine;

public class Raycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask _groundLayerMask = 1 << 3;
    [SerializeField] private LayerMask _baseLayerMask = 1 << 9;
    [SerializeField] private LayerMask _flagLayerMask = 1 << 10;
    [SerializeField] private float _maxRaycastDistance = 100f;

    private Camera _mainCamera;

    public LayerMask GroundLayerMask => _groundLayerMask;
    public LayerMask BaseLayerMask => _baseLayerMask;
    public LayerMask FlagLayerMask => _flagLayerMask;

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

    public bool TryGetFlagUnderMouse(out Flag flag)
    {
        flag = null;
        if (RaycastFromMouse(out RaycastHit hit, _flagLayerMask))
            return hit.collider.TryGetComponent(out flag);

        return false;
    }

    public bool TryGetObjectUnderMouse<T>(out T obj, LayerMask layerMask) where T : MonoBehaviour
    {
        obj = null;
        if (RaycastFromMouse(out RaycastHit hit, layerMask))
            return hit.collider.TryGetComponent(out obj);

        return false;
    }
}
