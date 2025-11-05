using System.Collections.Generic;
using UnityEngine;

public class BaseSelectionManager : MonoBehaviour
{
    //private static BaseSelectionManager _instance;//TODO: убрать static !!!
    //public static BaseSelectionManager Instance => _instance;//TODO: убрать static !!!

    private List<BaseController> _allBases = new List<BaseController>();
    private BaseController _currentlySelectedBase;

    public BaseController CurrentlySelectedBase => _currentlySelectedBase;

    // Временный фасад для обратной совместимости
    [System.Obsolete("Use direct reference instead of Singleton pattern")]
    public static BaseSelectionManager Instance
    {
        get
        {
            var manager = FindAnyObjectByType<BaseSelectionManager>();
            if (manager != null)
                Debug.LogWarning("Using deprecated Instance property. Use direct reference instead.");
            return manager;
        }
    }

    private void Awake()
    {
        //if (_instance != null && _instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //_instance = this;
    }

    public void RegisterBase(BaseController baseController)
    {
        if (_allBases.Contains(baseController) == false)
        {
            _allBases.Add(baseController);
            baseController.BaseSelected += OnBaseSelected;
            baseController.BaseDeselected += OnBaseDeselected;
        }
    }

    public void UnregisterBase(BaseController baseController)
    {
        if (_allBases.Contains(baseController))
        {
            _allBases.Remove(baseController);
            baseController.BaseSelected -= OnBaseSelected;
            baseController.BaseDeselected -= OnBaseDeselected;

            if (_currentlySelectedBase == baseController)
                _currentlySelectedBase = null;
        }
    }

    // Явные методы для доступа вместо статических
    public void SelectBase(BaseController baseController) => 
        OnBaseSelected(baseController);
    public void DeselectAll() => 
        DeselectAllBases();

    private void OnBaseSelected(BaseController selectedBase)
    {
        if (_currentlySelectedBase != null && _currentlySelectedBase != selectedBase)
            _currentlySelectedBase.DeselectBase();

        _currentlySelectedBase = selectedBase;
        Debug.Log($"Base selected: {selectedBase.name}, Priority: {selectedBase.CurrentPriority}");
    }

    private void OnBaseDeselected(BaseController deselectedBase)
    {
        if (_currentlySelectedBase == deselectedBase)
        {
            _currentlySelectedBase = null;
            Debug.Log($"Base deselected: {deselectedBase.name}");
        }
    }

    public void DeselectAllBases()
    {
        foreach (var baseController in _allBases)
            baseController.DeselectBase();

        _currentlySelectedBase = null;
    }

    private void OnDestroy()
    {
        foreach (var baseController in _allBases)
        {
            if (baseController != null)
            {
                baseController.BaseSelected -= OnBaseSelected;
                baseController.BaseDeselected -= OnBaseDeselected;
            }
        }
    }
}