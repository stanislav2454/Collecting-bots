using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const string MouseScrollWheelAxis = "Mouse ScrollWheel";
    private const string MouseXAxis = "Mouse X";
    private const string MouseYAxis = "Mouse Y";
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const int RightMouseButton = 1;

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
    private bool _isRightMousePressed = false;

    private void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleCameraMovement();
        HandleReset();
        ClampCameraHeight();
    }

    private void HandleCameraMovement()
    {
        HandleZoom();

        if (_isRightMousePressed)
        {
            HandleRotation();
            HandleHorizontalMovement();
            HandleVerticalMovement();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(RightMouseButton))
            _isRightMousePressed = true;

        if (Input.GetMouseButtonUp(RightMouseButton))
            _isRightMousePressed = false;
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
        const float Direction = 1f;
        float verticalMove = 0f;

        if (Input.GetKey(_cameraRaiseKey))
            verticalMove = Direction;
        else if (Input.GetKey(_cameraLowerKey))
            verticalMove = -Direction;

        if (verticalMove != 0f)
        {
            Vector3 verticalMovement = Vector3.up * verticalMove * _verticalMoveSpeed * Time.deltaTime;
            transform.Translate(verticalMovement, Space.World);
        }
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis(MouseXAxis) * _rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis(MouseYAxis) * _rotationSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, mouseX, Space.World);
        transform.Rotate(Vector3.left, mouseY, Space.Self);

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.z = 0;
        transform.eulerAngles = currentRotation;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis(MouseScrollWheelAxis);

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

    private void OnGUI()
    {
        if (Application.isPlaying == false)
            return;

        Color originalColor = GUI.color;
        GUI.color = _isRightMousePressed ? Color.green : Color.red;

        GUILayout.BeginArea(new Rect(20, 20, 300, 150));

        GUILayout.Label("=== CAMERA CONTROLS ===");
        GUILayout.Label("ПКМ + WASD: Horizontal movement");
        GUILayout.Label("ПКМ + E/Q: Vertical movement");
        GUILayout.Label("ПКМ + Mouse: Rotate camera");
        GUILayout.Label("Mouse Wheel: Zoom (always)");
        GUILayout.Label($"{_cameraViewReset}: Reset camera");
        GUILayout.Label($"Height: {transform.position.y:F1}/{_maxHeight}");
        GUILayout.Label($"Camera Active: {_isRightMousePressed}");

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}