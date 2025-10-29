using System.Collections.Generic;
using UnityEngine;

public class BaseFlag : MonoBehaviour
{
    [Header("Flag Settings")]
    [SerializeField] private float _pulseAmplitude = 0.3f;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _minAlpha = 0.2f;
    [SerializeField] private float _maxAlpha = 0.8f;
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private Material _validMaterial;
    [SerializeField] private Material _invalidMaterial;

    private BaseController _ownerBase;
    private List<Renderer> _childRenderers = new List<Renderer>();
    private bool _isPreview = false;
    private bool _isValidPosition = true;
    private float _pulseTimer = 0f;
    private Vector3 _basePosition;
    private List<Material> _originalMaterials = new List<Material>();
    private List<Material> _currentMaterialInstances = new List<Material>();

    public Vector3 Position => transform.position;// 👈

    private void Update()
    {
        if (_isPreview)
        {
            _pulseTimer += Time.deltaTime * _pulseSpeed;

            float bounceHeight = Mathf.Sin(_pulseTimer) * _pulseAmplitude;
            transform.position = _basePosition + Vector3.up * bounceHeight;

            float alpha = Mathf.Lerp(_maxAlpha, _minAlpha, (Mathf.Sin(_pulseTimer) + 1f) / 2f);
            UpdateMaterialAlpha(alpha);
        }
    }

    public void Initialize(BaseController ownerBase, Vector3 position, bool isPreview = false)
    {
        _ownerBase = ownerBase;
        _isPreview = isPreview;
        _basePosition = position;

        transform.position = position;
        transform.rotation = Quaternion.identity;

        InitializeRenderer();
        ApplyMaterial();
    }

    private void InitializeRenderer()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        _childRenderers.AddRange(renderers);

        foreach (var renderer in _childRenderers)
            _originalMaterials.Add(renderer.material);

        if (_childRenderers.Count == 0)
            Debug.LogWarning("Flag prefab must have Renderer components in children!");
    }

    public void UpdatePosition(Vector3 newPosition, bool isValid)
    {
        _basePosition = newPosition;
        _isValidPosition = isValid;

        if (_isPreview == false)
            transform.position = newPosition;

        ApplyMaterial();
    }

    private void ApplyMaterial()
    {
        if (_childRenderers.Count == 0)
            return;

        ClearMaterialInstances();

        Material targetMaterial = GetTargetMaterial();

        foreach (var renderer in _childRenderers)
        {
            if (targetMaterial != null)
            {
                if (_isPreview && _previewMaterial != null)
                {
                    Material materialInstance = new Material(targetMaterial);
                    _currentMaterialInstances.Add(materialInstance);
                    renderer.material = materialInstance;
                    SetupMaterialForTransparency(materialInstance);
                }
                else
                {
                    renderer.material = targetMaterial;
                }
            }
            else
            {
                int index = _childRenderers.IndexOf(renderer);

                if (index < _originalMaterials.Count)
                    renderer.material = _originalMaterials[index];
            }
        }
    }

    private Material GetTargetMaterial()
    {
        if (_isPreview && _previewMaterial != null)
            return _previewMaterial;
        else if (_isValidPosition == false && _invalidMaterial != null)
            return _invalidMaterial;
        else if (_validMaterial != null)
            return _validMaterial;

        return null;
    }

    private void SetupMaterialForTransparency(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void UpdateMaterialAlpha(float alpha)
    {
        foreach (var renderer in _childRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }
        }
    }

    private void ClearMaterialInstances()
    {
        foreach (var materialInstance in _currentMaterialInstances)
            if (materialInstance != null)
                Destroy(materialInstance);

        _currentMaterialInstances.Clear();
    }

    public void Remove()
    {
        ClearMaterialInstances();

        if (gameObject != null)
            Destroy(gameObject);
    }
}