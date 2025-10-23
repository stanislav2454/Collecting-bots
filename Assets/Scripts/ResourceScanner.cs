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
    [SerializeField] private PrimitiveType _scannerPrimitiveType = PrimitiveType.Sphere; // ← НОВОЕ ПОЛЕ

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
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && _scannerZoneVisualizer != null)
        {
            _scannerZoneVisualizer.SetPrimitiveType(_scannerPrimitiveType);

            Vector3 zoneSize = Vector3.one * _scanRadius * 2f;
            _scannerZoneVisualizer.CreateOrUpdateZone(zoneSize, Vector3.zero);
        }
    }
#endif

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
            if (collider.TryGetComponent(out Item item) && item.CanBeCollected)
            {
                currentResources.Add(item);

                if (_detectedResources.Contains(item) == false)
                {
                    _detectedResources.Add(item);
                    ResourceFound?.Invoke(item);
                }
            }
        }

        _detectedResources.RemoveAll(resource =>
            currentResources.Contains(resource) == false || IsResourceAvailable(resource) == false);
    }

    private bool IsResourceAvailable(Item item)
    {
        if (item == null || !item.CanBeCollected)
            return false;

        if (item.transform.parent != null)
            return false;

        if (item.TryGetComponent<Collider>(out var collider) && collider.enabled == false)
            return false;

        return true;
    }

    private void CreateScannerZone()
    {
        _scannerZoneVisualizer = gameObject.AddComponent<ZoneVisualizer>();
        _scannerZoneVisualizer.SetPrimitiveType(_scannerPrimitiveType);

        Vector3 zoneSize = Vector3.one * _scanRadius * 2f;
        _scannerZoneVisualizer.CreateOrUpdateZone(zoneSize, Vector3.zero);
        _scannerZoneVisualizer.SetZoneVisible(_showScannerZone);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}