using UnityEngine;

public class FlagInputHandler : MonoBehaviour
{
    [Header("InputKeys")]
    [SerializeField] private KeyCode _flagPlacement = KeyCode.Mouse1;

    [Space(5)]
    [Header("Dependencies")]
    [SerializeField] private BaseSelectionManager _selectionManager;
    [SerializeField] private Raycaster _raycaster;

    private void Update()
    {
        HandleFlagPlacement();
    }

    private void HandleFlagPlacement()
    {
        if (_selectionManager == null || _raycaster == null || Input.GetKeyDown(_flagPlacement) == false)
            return;

        var selectedBase = _selectionManager.CurrentlySelectedBase;
        if (selectedBase == null || selectedBase.CanBuildNewBase == false)
            return;

        if (_raycaster.TryGetGroundUnderMouse(out Vector3 groundPoint))
            selectedBase.TrySetFlag(groundPoint);
    }
}