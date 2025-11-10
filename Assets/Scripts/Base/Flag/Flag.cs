using UnityEngine;
using System;

public class Flag : MonoBehaviour
{
    [Header("Flag References")]
    [SerializeField] private MeshRenderer _flagRenderer;
    [SerializeField] private Collider _flagCollider;

    [Header("Flag Materials")]
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private Material _validMaterial;
    [SerializeField] private Material _invalidMaterial;

    private BaseController _ownerBase;

    public event Action<Vector3> FlagPositionChanged;
    public event Action<Vector3> FlagSettled;
    public event Action FlagRemoved;

    public FlagState CurrentState { get; private set; } = FlagState.Hide;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        InitializeAndValidateDependencies();
    }

    public void Initialize(BaseController ownerBase) =>
        _ownerBase = ownerBase;

    public void HandleClickInteraction()
    {
        if (CurrentState == FlagState.Setted)
        {
            StartMoving();
            Debug.Log($"Flag: Started moving from input");
        }
    }

    public void StartMoving()
    {
        if (CurrentState == FlagState.Move)
            return;

        SetState(FlagState.Move);
        _flagCollider.enabled = false;
        UpdateVisuals();
    }

    public void SetPosition(Vector3 position, bool isValidPosition = true)
    {
        transform.position = position;

        if (CurrentState == FlagState.Move)
        {
            UpdateVisuals(isValidPosition);
            FlagPositionChanged?.Invoke(position);
        }
    }

    public void PlaceFlag()
    {
        if (CurrentState != FlagState.Move)
            return;

        SetState(FlagState.Setted);
        _flagCollider.enabled = true;
        UpdateVisuals(true);
        FlagSettled?.Invoke(transform.position);
    }

    public void PlaceFlagDirectly(Vector3 position)
    {
        transform.position = position;
        SetState(FlagState.Setted);
        _flagCollider.enabled = true;
        UpdateVisuals(true);
        FlagSettled?.Invoke(position);
    }

    public void RemoveFlag()
    {
        SetState(FlagState.Hide);
        FlagRemoved?.Invoke();
    }

    private void SetState(FlagState newState)
    {
        CurrentState = newState;
        gameObject.SetActive(newState != FlagState.Hide);
    }

    private void UpdateVisuals(bool isValidPosition = true)
    {
        if (_flagRenderer == null)
            return;

        switch (CurrentState)
        {
            case FlagState.Move:
                _flagRenderer.material = isValidPosition ? _previewMaterial : _invalidMaterial;
                break;

            case FlagState.Setted:
            case FlagState.DeliveryResourcesToFlag:
                _flagRenderer.material = _validMaterial;
                break;

            case FlagState.Hide:
                break;
        }
    }

    private void InitializeAndValidateDependencies()
    {
        if (_flagRenderer == null)
            Debug.LogError("MeshRenderer not found in SimpleFlag!");

        if (_flagCollider == null)
            Debug.LogError("Collider not found in SimpleFlag!");
    }
}