using UnityEngine;
using System;

public class BaseSelectionController : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private MaterialChanger _materialChanger;
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;

    public event Action<bool> SelectionChanged;

    public bool IsSelected { get; private set; } = false;

    private void Start()
    {
        InitializeSelection();
    }

    private void OnValidate()
    {
        if (_materialChanger == null)
        {
            TryGetComponent(out _materialChanger);

            if (_materialChanger == null)
                Debug.LogError("MaterialChanger not found in BaseSelectionController!");
        }

        if (_viewTransform == null)
        {
            _viewTransform = transform.Find("View");

            if (_viewTransform == null)
                Debug.LogError("ViewTransform not found in BaseSelectionController!");
        }
    }

    private void OnDestroy()
    {
        SelectionChanged = null;
    }

    private void OnMouseDown()
    {
        ToggleSelection();
    }

    public void SetSelected(bool selected, bool notifyOthers = true)
    {
        if (IsSelected == selected)
            return;

        IsSelected = selected;

        UpdateVisualState();

        if (notifyOthers)
            SelectionChanged?.Invoke(selected);
    }

    public void SelectBase() =>
        SetSelected(true);

    public void DeselectBase() =>
        SetSelected(false);

    public void ToggleSelection() =>
        SetSelected(IsSelected ? false : true);

    private void InitializeSelection()
    {
        if (_viewTransform != null)
            _originalViewScale = _viewTransform.localScale;

        SetSelected(false, false);
    }

    private void UpdateVisualState()
    {
        if (_viewTransform != null)
        {
            _viewTransform.localScale = IsSelected ?
                _originalViewScale * _selectedScaleMultiplier :
                _originalViewScale;
        }

        _materialChanger?.SetSelected(IsSelected);
    }
}