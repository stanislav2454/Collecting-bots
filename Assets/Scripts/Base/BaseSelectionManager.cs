using System.Collections.Generic;
using UnityEngine;

public class BaseSelectionManager : MonoBehaviour
{
    [Header("InputKeys")]
    [SerializeField] private KeyCode _selectBase = KeyCode.Mouse0;

    [Header("Dependencies")]
    [SerializeField] private Raycaster _raycaster;

    private List<BaseController> _allBases = new List<BaseController>();

    public BaseController CurrentlySelectedBase { get; private set; }

    private void Update()
    {
        HandleBaseSelection();
    }

    private void OnDestroy()
    {
        _allBases.Clear();
        CurrentlySelectedBase = null;
    }

    public void RegisterBase(BaseController baseController)
    {
        if (_allBases.Contains(baseController) == false)
            _allBases.Add(baseController);
    }

    public void UnregisterBase(BaseController baseController)
    {
        if (_allBases.Contains(baseController))
        {
            _allBases.Remove(baseController);

            if (CurrentlySelectedBase == baseController)
            {
                CurrentlySelectedBase.SetSelected(false);
                CurrentlySelectedBase = null;
            }
        }
    }

    private void HandleBaseSelection()
    {
        if (Input.GetKeyDown(_selectBase))
        {
            if (_raycaster.TryGetBaseUnderMouse(out var baseController))
                SelectBase(baseController);
            else
                DeselectCurrentBase();
        }
    }

    private void SelectBase(BaseController baseController)
    {
        if (CurrentlySelectedBase == baseController)
            return;

        DeselectCurrentBase();

        CurrentlySelectedBase = baseController;
        CurrentlySelectedBase.SetSelected(true);
    }

    private void DeselectCurrentBase()
    {
        if (CurrentlySelectedBase != null)
        {
            CurrentlySelectedBase.SetSelected(false);
            CurrentlySelectedBase = null;
        }
    }
}