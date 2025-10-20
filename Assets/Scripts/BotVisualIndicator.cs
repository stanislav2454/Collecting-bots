using UnityEngine;

public class BotVisualIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float _iconSize = 0.3f;
    [SerializeField] private int _fontSize = 50;

    private GameObject _statusIcon;
    private TextMesh _statusText;
    private BotController _botController;

    private void Start()
    {
        _botController = GetComponent<BotController>();
        CreateStatusIndicator();
    }

    private void Update()
    {
        if (_botController != null && _statusIcon != null)
        {
            UpdateAIStatus(_botController.EnableAI, _botController.CurrentState);

            if (Camera.main != null)// Поворачиваем индикатор к камере
            {
                _statusIcon.transform.LookAt(Camera.main.transform);
                _statusIcon.transform.Rotate(0, 180, 0);
            }
        }
    }

    private void OnDestroy()
    {
        if (_statusIcon != null)
            DestroyImmediate(_statusIcon);
    }

    public void UpdateAIStatus(bool aiEnabled, BotState state)
    {
        if (_statusIcon == null)
            return;

        Renderer iconRenderer = _statusIcon.GetComponent<Renderer>();

        if (iconRenderer != null)
        {
            if (aiEnabled)
            {
                iconRenderer.material.color = GetStateColor(state);
                if (_statusText != null)
                {
                    _statusText.text = state.ToString();
                    _statusText.color = Color.white;
                }
            }
            else
            {
                iconRenderer.material.color = Color.gray;
                if (_statusText != null)
                {
                    _statusText.text = "DISABLED";
                    _statusText.color = Color.red;
                }
            }
        }
    }

    private void CreateStatusIndicator()
    {// Создаем индикатор статуса
        _statusIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _statusIcon.name = "StatusIndicator";
        _statusIcon.transform.SetParent(transform);
        _statusIcon.transform.localPosition = new Vector3(0, 3f, 0);
        _statusIcon.transform.localScale = Vector3.one * _iconSize;

        DestroyImmediate(_statusIcon.GetComponent<Collider>());

        // Создаем текстовый элемент
        GameObject textObject = new GameObject("StatusText");
        textObject.transform.SetParent(_statusIcon.transform);
        textObject.transform.localPosition = new Vector3(0, -0.6f, 0);
        textObject.transform.localScale = Vector3.one * 0.1f;

        _statusText = textObject.AddComponent<TextMesh>();
        _statusText.anchor = TextAnchor.MiddleCenter;
        _statusText.alignment = TextAlignment.Center;
        _statusText.fontSize = _fontSize;
        _statusText.characterSize = 0.1f;

        // Важно: устанавливаем материал для текста
        _statusText.color = Color.white;

        // Убедимся что есть шрифт
        if (_statusText.font == null)// Используем дефолтный шрифт        
            _statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        UpdateAIStatus(_botController.EnableAI, _botController.CurrentState);
    }

    private Color GetStateColor(BotState state)
    {
        switch (state)
        {//.color = new Color(1f, 0.5f, 0f); // оранжевый
            case BotState.Idle: return Color.yellow;
            case BotState.Search: return Color.cyan;
            case BotState.MoveToItem: return Color.blue;
            case BotState.Collect: return Color.green;
            case BotState.MoveToDeposit: return Color.magenta;
            case BotState.Deposit: return Color.white;
            case BotState.Wait: return Color.gray;
            default: return new Color(1f, 0.5f, 0f);
        }
    }
}