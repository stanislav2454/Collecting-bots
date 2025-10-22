using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    private const string MouseAxisHandleZoom = "Mouse ScrollWheel";
    private const string MouseAxisX = "Mouse X";
    private const string MouseAxisY = "Mouse Y";
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const int MouseButtonScrollWheel = 2;

    [Header("InputKeys Settings")]
    [SerializeField] private KeyCode _cameraRaiseKey = KeyCode.E;
    [SerializeField] private KeyCode _cameraLowerKey = KeyCode.Q;
    [SerializeField] private KeyCode _cameraViewReset = KeyCode.R;

    [Header("Camera Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _verticalMoveSpeed = 5f;

    [Header("Height Limits")]
    [SerializeField] private float _minHeight = 2f;
    [SerializeField] private float _maxHeight = 20f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        HandleHorizontalMovement();
        HandleVerticalMovement();
        HandleRotation();
        HandleZoom();
        HandleReset();
        ClampCameraHeight();
    }

    private void HandleHorizontalMovement()
    {
        float horizontal = Input.GetAxis(HorizontalAxis);
        float vertical = Input.GetAxis(VerticalAxis);

        Vector3 movement = new Vector3(horizontal, 0, vertical) * _moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    private void HandleVerticalMovement()
    {
        float verticalMove = 0f;

        if (Input.GetKey(_cameraRaiseKey))
            verticalMove = 1f;
        else if (Input.GetKey(_cameraLowerKey))
            verticalMove = -1f;

        if (verticalMove != 0f)
        {
            Vector3 verticalMovement = Vector3.up * verticalMove * _verticalMoveSpeed * Time.deltaTime;
            transform.Translate(verticalMovement, Space.World);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(MouseButtonScrollWheel))
        {
            float mouseX = Input.GetAxis(MouseAxisX) * _rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis(MouseAxisY) * _rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY, Space.Self);

            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.z = 0;
            transform.eulerAngles = currentRotation;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis(MouseAxisHandleZoom);

        if (scroll != 0f)
        {
            Vector3 zoom = transform.forward * scroll * _zoomSpeed;
            transform.position += zoom;
        }
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(_cameraViewReset))
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }

    private void ClampCameraHeight()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, _minHeight, _maxHeight);
        transform.position = position;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        string heightInfo = $"Height: {transform.position.y:F1}\nMin: {_minHeight} Max: {_maxHeight}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, heightInfo);

        Gizmos.color = Color.cyan;
        Vector3 groundPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Gizmos.DrawLine(transform.position, groundPosition);
        Gizmos.DrawWireCube(groundPosition + Vector3.up * 0.1f, new Vector3(0.5f, 0.1f, 0.5f));
#endif
    }

    private void OnGUI()
    {
        if (Application.isPlaying == false)
            return;

        Color originalColor = GUI.color;
        GUI.color = Color.red;

        GUILayout.BeginArea(new Rect(10, 560, 300, 150));

        GUILayout.Label("=== CAMERA CONTROLS ===");
        GUILayout.Label("WASD: Horizontal movement");
        GUILayout.Label($"{_cameraRaiseKey}/Q: Vertical movement");
        GUILayout.Label("RMB + Mouse: Rotate");
        GUILayout.Label("Mouse Wheel: Zoom");
        GUILayout.Label($"{_cameraViewReset}: Reset camera");
        GUILayout.Label($"Height: {transform.position.y:F1}/{_maxHeight}");

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}