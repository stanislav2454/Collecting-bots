using UnityEngine;

public class SelectableVisual : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private MaterialChanger _materialChanger;

    [Header("Selection Settings")]
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;
    private bool _isSelected = false;

    public bool IsSelected => _isSelected;

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
        if (_isSelected == selected)
            return;

        _isSelected = selected;

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

        Debug.Log($"[SelectableVisual] Selection state: {selected}");
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_viewTransform != null)
            _originalViewScale = _viewTransform.localScale;
#endif
    }
}