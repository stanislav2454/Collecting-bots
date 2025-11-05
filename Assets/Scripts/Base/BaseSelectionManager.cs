using System.Collections.Generic;
using UnityEngine;

public class BaseSelectionManager : MonoBehaviour
{
    private List<BaseController> _allBases = new List<BaseController>();
    private BaseController _currentlySelectedBase;

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
    }

    private void OnBaseDeselected(BaseController deselectedBase)
    {
        if (_currentlySelectedBase == deselectedBase)
            _currentlySelectedBase = null;
    }
}