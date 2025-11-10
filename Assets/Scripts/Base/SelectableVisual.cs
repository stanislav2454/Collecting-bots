using UnityEngine;

public class SelectableVisual : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private MaterialChanger _materialChanger;

    [Header("Selection Settings")]
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;
    public bool IsSelected { get; private set; } = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_viewTransform != null)
            _originalViewScale = _viewTransform.localScale;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (IsSelected == selected)
            return;

        IsSelected = selected;

        if (selected)
        {
            if (_viewTransform != null)
                _viewTransform.localScale = _originalViewScale * _selectedScaleMultiplier;
            _materialChanger?.SetSelected(true);
        }
        else
        {
            if (_viewTransform != null)
                _viewTransform.localScale = _originalViewScale;
            _materialChanger?.SetSelected(false);
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_viewTransform != null)
            _originalViewScale = _viewTransform.localScale;
#endif
    }
}