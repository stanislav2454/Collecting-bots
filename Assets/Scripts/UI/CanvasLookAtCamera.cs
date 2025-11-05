using UnityEngine;

public class CanvasLookAtCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private bool _reverseForward = true;

    private Transform _cameraTransform;

    private void Start()
    {
        InitializeCamera();
    }

    private void Update()
    {
        RotateTowardsCamera();
    }

    private void InitializeCamera()
    {
        //if (_targetCamera == null)
        //{
        //    _targetCamera = Camera.main;
        //    if (_targetCamera == null)
        //        _targetCamera = FindAnyObjectByType<Camera>();
        //}


        if (_targetCamera != null)
            _cameraTransform = _targetCamera.transform;

        if (_cameraTransform == null)
            Debug.LogWarning("CanvasLookAtCamera: Камера не найдена!");
    }

    // ДОБАВЛЯЕМ публичный метод для установки камеры
    public void SetTargetCamera(Camera camera)
    {
        _targetCamera = camera;
        if (_targetCamera != null)
        {
            _cameraTransform = _targetCamera.transform;
        }
    }

    private void RotateTowardsCamera()
    {
        if (_cameraTransform == null)
            return;

        Vector3 directionToCamera = _cameraTransform.position - transform.position;

        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_reverseForward ?
                                        -directionToCamera : directionToCamera);
            transform.rotation = targetRotation;
        }
    }
}