using UnityEngine;

public class ZoneVisualizer : MonoBehaviour
{
    [Header("Zone Visualization")]
    [SerializeField] private bool _showZone = true;
    [SerializeField] private Color _zoneColor = Color.green;
    [SerializeField] private float _zoneOpacity = 0.3f;
    [SerializeField] private PrimitiveType _primitiveType = PrimitiveType.Cube;

    private GameObject _zoneVisual;
    private Renderer _zoneRenderer;
    private Collider _zoneCollider;
    [SerializeField] private Material _zoneMaterial;

    private void OnDestroy()
    {
        DestroyZoneVisual();
    }

    public void CreateOrUpdateZone(Vector3 size, Vector3 offset)
    {
        if (_showZone == false)
        {
            DestroyZoneVisual();
            return;
        }

        if (_zoneVisual == null || NeedsRecreation())
        {
            DestroyZoneVisual();
            CreateZoneVisual();
        }

        UpdateZoneVisual(size, offset);
    }

    public void SetZoneVisible(bool visible)
    {
        _showZone = visible;

        if (_zoneVisual != null)
            _zoneVisual.SetActive(visible);
    }

    public void SetPrimitiveType(PrimitiveType primitiveType)
    {
        if (_primitiveType != primitiveType)
        {
            _primitiveType = primitiveType;

            if (_zoneVisual != null)
                DestroyZoneVisual();
        }
    }

    private bool NeedsRecreation()
    {
        if (_zoneVisual == null || _zoneCollider == null)
            return false;

        return _primitiveType switch
        {
            PrimitiveType.Sphere when (_zoneCollider is SphereCollider) == false =>
            true,
            PrimitiveType.Cube when !(_zoneCollider is BoxCollider) == false =>
            true,
            PrimitiveType.Capsule when !(_zoneCollider is CapsuleCollider) == false =>
            true,
            _ => false
        };
    }

    private void CreateZoneVisual()
    {
        _zoneVisual = GameObject.CreatePrimitive(_primitiveType);
        _zoneVisual.name = $"{gameObject.name}_ZoneVisual";
        _zoneVisual.transform.SetParent(transform);

        DestroyImmediate(_zoneVisual.GetComponent<Collider>());

        _zoneRenderer = _zoneVisual.GetComponent<Renderer>();
        _zoneMaterial = CreateTransparentMaterial();
        _zoneRenderer.material = _zoneMaterial;

        if (_zoneVisual.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }


        SetupMaterialForPrimitive();
    }

    private void SetupMaterialForPrimitive()
    {
        if (_zoneMaterial == null)
            return;

        if (_primitiveType == PrimitiveType.Sphere || _primitiveType == PrimitiveType.Capsule)
            _zoneMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
    }

    private Material CreateTransparentMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;

        return material;
    }

    private void UpdateZoneVisual(Vector3 size, Vector3 offset)
    {
        if (_zoneVisual == null)
            return;

        _zoneVisual.transform.localPosition = offset;

        if (_primitiveType == PrimitiveType.Sphere)
            _zoneVisual.transform.localScale = Vector3.one * size.x;
        else
            _zoneVisual.transform.localScale = size;

        _zoneVisual.transform.rotation = Quaternion.identity;

        Color finalColor = _zoneColor;
        finalColor.a = _zoneOpacity;
        _zoneMaterial.color = finalColor;

        _zoneVisual.SetActive(_showZone);
    }

    private void DestroyZoneVisual()
    {
        if (_zoneVisual != null)
        {
            DestroyImmediate(_zoneVisual);
            _zoneVisual = null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && _zoneVisual != null)
            if (NeedsRecreation())
                DestroyZoneVisual();
    }
#endif
}