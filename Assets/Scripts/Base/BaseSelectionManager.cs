using System.Collections.Generic;
using UnityEngine;

public class BaseSelectionManager : MonoBehaviour
{
    private static BaseSelectionManager _instance;
    public static BaseSelectionManager Instance => _instance;

    private BaseController _currentlySelectedBase;
    private List<BaseController> _allBases = new List<BaseController>();

    public BaseController CurrentlySelectedBase => _currentlySelectedBase;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
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

    private void OnBaseSelected(BaseController selectedBase)
    {
        if (_currentlySelectedBase != null && _currentlySelectedBase != selectedBase)
            _currentlySelectedBase.DeselectBase();

        _currentlySelectedBase = selectedBase;
        Debug.Log($"Base selected: {selectedBase.name}");
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