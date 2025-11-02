using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;

    [Header("Renderers")]
    [SerializeField] private List<MeshRenderer> _meshRenderers;

    private List<Material> _originalMaterials = new List<Material>();

    private void Start()
    {
        if (_meshRenderers.Count > 0 && _defaultMaterial == null)
            _defaultMaterial = _meshRenderers[0].material;

        foreach (var renderer in _meshRenderers)
            _originalMaterials.Add(renderer.material);
    }

    private void OnValidate()
    {
        if (_meshRenderers == null || _meshRenderers.Count == 0)
        {
            TryGetComponent(out MeshRenderer mainRenderer);
            if (mainRenderer != null)
                _meshRenderers = new List<MeshRenderer> { mainRenderer };
        }
    }

    public void SetDefaultMaterial()
    {  
        // ДОБАВЬТЕ ПРОВЕРКУ:
        if (_meshRenderers == null || _meshRenderers.Count == 0)
            return;

        foreach (var renderer in _meshRenderers)
            if (_defaultMaterial != null)
                renderer.material = _defaultMaterial;
    }

    public void SetAlternativeMaterial()
    {
        foreach (var renderer in _meshRenderers)
            if (_selectedMaterial != null)
                renderer.material = _selectedMaterial;
    }

    public void SetSelected(bool isSelected)
    {
        // ДОБАВЬТЕ ПРОВЕРКУ:
        if (_meshRenderers == null || _meshRenderers.Count == 0)
            return;

        if (isSelected)
            SetAlternativeMaterial();
        else
            SetDefaultMaterial();
    }
}