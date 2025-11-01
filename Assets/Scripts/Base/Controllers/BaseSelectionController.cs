using UnityEngine;
using System;

public class BaseSelectionController : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private MaterialChanger _materialChanger;
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private float _selectedScaleMultiplier = 1.1f;

    private Vector3 _originalViewScale;
    private bool _isSelected = false;

    public event Action<bool> SelectionChanged; // true - selected, false - deselected
    public bool IsSelected => _isSelected;

    private void Start()
    {
        InitializeSelection();
    }

    public void SetSelected(bool selected, bool notifyOthers = true)
    {
        if (_isSelected == selected)
            return;

        _isSelected = selected;

        UpdateVisualState();

        if (notifyOthers)
            SelectionChanged?.Invoke(selected);
    }

    public void SelectBase() => 
        SetSelected(true);
    public void DeselectBase() => 
        SetSelected(false);
    public void ToggleSelection() => 
        SetSelected(!_isSelected);

    private void InitializeSelection()
    {
        if (_viewTransform != null)
        {
            _originalViewScale = _viewTransform.localScale;
        }
        SetSelected(false, false);
    }

    private void UpdateVisualState()
    {
        if (_viewTransform != null)
        {
            _viewTransform.localScale = _isSelected ?
                _originalViewScale * _selectedScaleMultiplier :
                _originalViewScale;
        }

        _materialChanger?.SetSelected(_isSelected);
    }

    // Обработчик клика мыши
    private void OnMouseDown()
    {
        ToggleSelection();
    }

    // Валидация зависимостей
    private void OnValidate()
    {
        if (_materialChanger == null)
        {
            // Попробуем найти на этом же GameObject
            TryGetComponent(out _materialChanger);
            if (_materialChanger == null)
                Debug.LogError("MaterialChanger not found in BaseSelectionController!");
        }

        if (_viewTransform == null)
        {
            // Попробуем найти дочерний объект с именем "View"
            _viewTransform = transform.Find("View");
            if (_viewTransform == null)
                Debug.LogError("ViewTransform not found in BaseSelectionController!");
        }
    }

    // Очистка событий
    private void OnDestroy()
    {
        SelectionChanged = null;
    }
}