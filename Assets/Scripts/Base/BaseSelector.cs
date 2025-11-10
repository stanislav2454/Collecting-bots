using System.Collections.Generic;
using UnityEngine;

public class BaseSelector : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Raycaster _raycaster;

    private List<BaseController> _allBases = new List<BaseController>();

    public BaseController CurrentlySelectedBase { get; private set; }

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

    public void SelectBase(BaseController baseController)
    {
        if (CurrentlySelectedBase == baseController)
            return;

        DeselectCurrentBase();

        CurrentlySelectedBase = baseController;
        CurrentlySelectedBase.SetSelected(true);

        Debug.Log($"BaseSelector: Selected {baseController.name}");
    }

    public void DeselectCurrentBase()
    {
        if (CurrentlySelectedBase != null)
        {
            CurrentlySelectedBase.SetSelected(false);
            CurrentlySelectedBase = null;
            Debug.Log("BaseSelector: Deselected all bases");
        }
    }
}