using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ResourceScanner : ZoneVisualizer
{
    [Header("Scanner Settings")]
    [SerializeField] private float _scanRadius = 20f;
    [SerializeField] private float _scanInterval = 2f;

    [Header("Scanner Visualization")]
    [SerializeField] private bool _showScannerZone = true;
    [SerializeField] private PrimitiveType _scannerPrimitiveType = PrimitiveType.Sphere;
    [SerializeField] private Color _scannerZoneColor = Color.blue;
    [SerializeField] private float _scannerZoneOpacity = 0.2f;

    private List<Item> _detectedResources = new List<Item>();
    private Coroutine _scanningCoroutine;
    private ZoneVisualizer _scannerZoneVisualizer;

    public event Action<Item> ResourceFound;
    public event Action<Item> ResourceLost;

    private void Start()
    {
        CreateScannerZone();
        StartScanning();
    }

    private void OnDestroy()
    {
        StopScanning();
        CleanupScannerZone();
    }

    public void StartScanning()
    {
        if (_scanningCoroutine == null)
            _scanningCoroutine = StartCoroutine(ScanningCoroutine());
    }

    public void StopScanning()
    {
        if (_scanningCoroutine != null)
        {
            StopCoroutine(_scanningCoroutine);
            _scanningCoroutine = null;
        }
    }

    private void CleanupScannerZone()
    {
        if (_scannerZoneVisualizer != null)
        {
            Destroy(_scannerZoneVisualizer);
            _scannerZoneVisualizer = null;
        }
    }

    private IEnumerator ScanningCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_scanInterval);

        while (true)
        {
            PerformScan();
            yield return waitForSeconds;
        }
    }

    private void PerformScan()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius);

        var currentResources = new List<Item>();

        foreach (var collider in hitColliders)
        {
            if (collider.TryGetComponent(out Item item))
            {
                currentResources.Add(item);

                if (_detectedResources.Contains(item) == false)
                {
                    _detectedResources.Add(item);
                    ResourceFound?.Invoke(item);
                }
            }
        }

        foreach (var resource in _detectedResources.ToArray())
        {
            if (currentResources.Contains(resource) == false)
            {
                _detectedResources.Remove(resource);
                ResourceLost?.Invoke(resource);
            }
        }
    }

    private void CreateScannerZone()
    {
        _scannerZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        UpdateScannerZoneVisualization();
    }

    private void UpdateScannerZoneVisualization()
    {
        if (_scannerZoneVisualizer != null)
        {
            _scannerZoneVisualizer.SetPrimitiveType(_scannerPrimitiveType);

            Color colorWithOpacity = _scannerZoneColor;
            colorWithOpacity.a = _scannerZoneOpacity;
            _scannerZoneVisualizer.SetZoneColor(colorWithOpacity);

            Vector3 zoneSize = Vector3.one * _scanRadius * 2f;
            _scannerZoneVisualizer.CreateOrUpdateZone(zoneSize, Vector3.zero);
            _scannerZoneVisualizer.SetZoneVisible(_showScannerZone);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}