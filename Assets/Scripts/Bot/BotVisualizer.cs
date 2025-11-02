using UnityEngine;
using System;

public class BotVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool _enableVisualization = true;
    [SerializeField] private bool _showSceneGizmos = true;
    [SerializeField] private bool _showGameGUI = true;

    [Space]
    [SerializeField] private float _iconHeight = 3f;
    [SerializeField] private float _labelHeight = 2f;
    [SerializeField] private float _sphereRadius = 0.5f;
    [SerializeField] private float _carrySphereRadius = 0.3f;

    private Bot _bot;
    private BotMovementController _movement;
    private Color _currentStateColor;
    private string _currentStateIcon;

    public event Action<bool> VisualizationToggled;//todo: is never used
    public event Action<bool> SceneGizmosToggled;//todo: is never used
    public event Action<bool> GameGUIToggled;//todo: is never used

    public void Initialize(Bot bot, BotMovementController movement)
    {
        _bot = bot;
        _movement = movement;
    }

    public void UpdateVisualization(Color stateColor, string stateIcon)
    {
        _currentStateColor = stateColor;
        _currentStateIcon = stateIcon;
    }

    private void OnDrawGizmos()
    {
        if (_enableVisualization == false || _showSceneGizmos == false)
            return;

        if (Application.isPlaying == false || _bot == null)
            return;

        Gizmos.color = _currentStateColor;

        Vector3 iconPosition = transform.position + Vector3.up * _iconHeight;
        Gizmos.DrawIcon(iconPosition, _currentStateIcon, true);

        if (_movement != null && _movement.IsMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, _movement.transform.position + Vector3.up);
        }

        Gizmos.color = _currentStateColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, _sphereRadius);

        if (_bot.IsCarryingResource)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.2f, _carrySphereRadius);
        }
    }

    private void OnGUI()
    {
        if (_enableVisualization == false || _showGameGUI == false)
            return;

        if (Application.isPlaying == false || _bot == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * _labelHeight);
        if (screenPos.z <= 0)
            return;

        string stateText = $"{_bot.CurrentStateType}";

        if (_bot.IsCarryingResource)
            stateText += " 📦";

        if (_bot.AssignedResource != null)
            stateText += $"\nTarget: {_bot.AssignedResource.name}";

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = _currentStateColor;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 30, 180, 70), stateText, style);
    }
}