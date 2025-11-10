using UnityEngine;

public class BaseZoneVisualizer : MonoBehaviour
{
    [Header("Base Zones")]
    [SerializeField] private bool _showZones = true;
    [SerializeField] private float _heightVisualizationZone = 0.1f;
    [SerializeField] private Color _spawnZoneColor = Color.blue;
    [SerializeField] private Color _unloadZoneColor = Color.green;

    private ZoneVisualizer _spawnZoneVisualizer;
    private ZoneVisualizer _unloadZoneVisualizer;

    [field: SerializeField] public float SpawnZoneRadius { get; private set; } = 3f;
    [field: SerializeField] public float UnloadZoneRadius { get; private set; } = 1.5f;

    private void Start()
    {
        CreateZoneVisuals();
        SetZonesVisibility(_showZones);
    }

    private void OnDestroy()
    {
        if (_spawnZoneVisualizer != null)
            Destroy(_spawnZoneVisualizer);

        if (_unloadZoneVisualizer != null)
            Destroy(_unloadZoneVisualizer);
    }

    private void CreateZoneVisuals()
    {
        _spawnZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        _unloadZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();

        ConfigureZoneVisualizers();
    }

    private void ConfigureZoneVisualizers()
    {
        ApplyZoneVisualizationSettings(_spawnZoneVisualizer, SpawnZoneRadius, _spawnZoneColor);
        ApplyZoneVisualizationSettings(_unloadZoneVisualizer, UnloadZoneRadius, _unloadZoneColor);
    }

    private void ApplyZoneVisualizationSettings(ZoneVisualizer visualizer, float radius, Color color)
    {
        const float RadiusToDiameterMultiplier = 2f;

        if (visualizer != null)
        {
            Vector3 zoneSize = Vector3.one * radius * RadiusToDiameterMultiplier;
            zoneSize.y = _heightVisualizationZone;
            visualizer.CreateOrUpdateZone(zoneSize, Vector3.zero);
            visualizer.SetZoneColor(color);
        }
    }

    public void SetZonesVisibility(bool visible)
    {
        _showZones = visible;

        if (_spawnZoneVisualizer != null)
            _spawnZoneVisualizer.SetZoneVisible(_showZones);

        if (_unloadZoneVisualizer != null)
            _unloadZoneVisualizer.SetZoneVisible(_showZones);
    }

    public void SetZoneColors(Color spawnZoneColor, Color unloadZoneColor)
    {
        _spawnZoneColor = spawnZoneColor;
        _unloadZoneColor = unloadZoneColor;

        if (_spawnZoneVisualizer != null)
            _spawnZoneVisualizer.SetZoneColor(_spawnZoneColor);

        if (_unloadZoneVisualizer != null)
            _unloadZoneVisualizer.SetZoneColor(_unloadZoneColor);
    }
}