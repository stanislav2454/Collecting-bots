using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    private const string MouseAxisHandleZoom = "Mouse ScrollWheel";
    private const string MouseAxisX = "Mouse X";
    private const string MouseAxisY = "Mouse Y";
    private const string HorizontalAxis = "Horizontal";
    private const string VerticalAxis = "Vertical";
    private const int MouseButtonScrollWheel = 2;

    [Header("InputKeys")]
    [SerializeField] private KeyCode _cameraViewReset = KeyCode.R;

    [Header("Camera Settings")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
        HandleReset();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis(HorizontalAxis);
        float vertical = Input.GetAxis(VerticalAxis);

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(MouseButtonScrollWheel)) 
        {
            float mouseX = Input.GetAxis(MouseAxisX) * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis(MouseAxisY) * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY, Space.Self);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis(MouseAxisHandleZoom);
        Vector3 zoom = transform.forward * scroll * zoomSpeed;
        transform.position += zoom;
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(_cameraViewReset))
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }
}