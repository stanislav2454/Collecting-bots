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
    [SerializeField] private KeyCode _cameraLowerKey = KeyCode.Q; // Изменил на Q, т.к. W обычно вперед
    [SerializeField] private KeyCode _cameraViewReset = KeyCode.R;

    [Header("Camera Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _verticalMoveSpeed = 5f; // Скорость вертикального перемещения

    [Header("Height Limits")]
    [SerializeField] private float _minHeight = 2f;  // Минимальная высота
    [SerializeField] private float _maxHeight = 20f; // Максимальная высота

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

    /// <summary>
    /// Обработка горизонтального движения (WASD/Стрелки)
    /// </summary>
    private void HandleHorizontalMovement()
    {
        float horizontal = Input.GetAxis(HorizontalAxis);
        float vertical = Input.GetAxis(VerticalAxis);
        //float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * _moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    /// <summary>
    /// Обработка вертикального движения (E/Q)
    /// </summary>
    private void HandleVerticalMovement()
    {
        float verticalMove = 0f;

        if (Input.GetKey(_cameraRaiseKey)) // Поднимаем камеру

            verticalMove = 1f;
        else if (Input.GetKey(_cameraLowerKey)) // Опускаем камеру

            verticalMove = -1f;

        if (verticalMove != 0f) // Применяем вертикальное движение
        {
            Vector3 verticalMovement = Vector3.up * verticalMove * _verticalMoveSpeed * Time.deltaTime;
            transform.Translate(verticalMovement, Space.World);
        }
    }

    //private void HandleMovement()
    //{
    //    float horizontal = Input.GetAxis(HorizontalAxis);
    //    float vertical = Input.GetAxis(VerticalAxis);

    //    Vector3 movement = new Vector3(horizontal, 0, vertical) * _moveSpeed * Time.deltaTime;
    //    transform.Translate(movement, Space.Self);
    //}

    /// <summary>
    /// Обработка вращения камеры (зажатое Колесико мыши + движение мыши)
    /// </summary>
    private void HandleRotation()
    {
        if (Input.GetMouseButton(MouseButtonScrollWheel))
        {
            float mouseX = Input.GetAxis(MouseAxisX) * _rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis(MouseAxisY) * _rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, mouseX, Space.World);// Вращение по горизонтали (вокруг мировой оси Y)
            transform.Rotate(Vector3.left, mouseY, Space.Self);// Вращение по вертикали (вокруг локальной оси X)

            // Фиксируем Z-rotation чтобы камера не переворачивалась
            Vector3 currentRotation = transform.eulerAngles;
            currentRotation.z = 0;
            transform.eulerAngles = currentRotation;
        }
    }

    /// <summary>
    /// Обработка зума (прокрутка Колесика мыши)
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis(MouseAxisHandleZoom);

        if (scroll != 0f)
        {
            Vector3 zoom = transform.forward * scroll * _zoomSpeed;
            transform.position += zoom;
        }
    }

    /// <summary>
    /// Сброс камеры в начальное положение
    /// </summary>
    private void HandleReset()
    {
        if (Input.GetKeyDown(_cameraViewReset))
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }

    /// <summary>
    /// Ограничение высоты камеры
    /// </summary>
    private void ClampCameraHeight()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, _minHeight, _maxHeight);
        transform.position = position;
    }

    /// <summary>
    /// Показ текущей высоты камеры в редакторе
    /// </summary>
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        // Показываем текущую высоту камеры
        string heightInfo = $"Height: {transform.position.y:F1}\nMin: {_minHeight} Max: {_maxHeight}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, heightInfo);

        // Рисуем линию до земли
        Gizmos.color = Color.cyan;
        Vector3 groundPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Gizmos.DrawLine(transform.position, groundPosition);
        Gizmos.DrawWireCube(groundPosition + Vector3.up * 0.1f, new Vector3(0.5f, 0.1f, 0.5f));
#endif
    }

    /// <summary>
    /// Отображение управления в GUI (опционально)
    /// </summary>
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
        // Восстанавливаем оригинальный цвет
        GUI.color = originalColor;
    }
}