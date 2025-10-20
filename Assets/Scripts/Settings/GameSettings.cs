using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Bot Collector/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Bot Settings")]
    public float botMoveSpeed = 3.5f;
    public float botRotationSpeed = 120f;
    public float botStoppingDistance = 1f;

    [Header("Debug Settings")]
    public bool showDebugGizmos = true;
    public Color debugRayColor = Color.blue;
}