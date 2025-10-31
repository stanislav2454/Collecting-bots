using UnityEngine;
using System;

public class SimpleFlag : MonoBehaviour
{
    [Header("Flag References")]
    [SerializeField] private MeshRenderer _flagRenderer;
    [SerializeField] private Collider _flagCollider;

    [Header("Flag Materials")]
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private Material _validMaterial;
    [SerializeField] private Material _invalidMaterial;

    private FlagState _currentState = FlagState.Hide;// todo
    private BaseController _ownerBase;

    public event Action<Vector3> FlagPositionChanged;
    public event Action<Vector3> FlagSettled;
    public event Action FlagRemoved;

    public FlagState CurrentState => _currentState;// todo
    public Vector3 Position => transform.position;

    private void Awake()
    {
        InitializeAndValidateDependencies();
    }

    private void OnValidate()
    {
        InitializeAndValidateDependencies();
    }

    private void OnMouseDown()
    {
        if (_currentState == FlagState.Setted)
            StartMoving();
    }

    public void Initialize(BaseController ownerBase) =>
        _ownerBase = ownerBase;

    public void StartMoving()
    {
        if (_currentState == FlagState.Move)
            return;

        SetState(FlagState.Move);
        _flagCollider.enabled = false;
        UpdateVisuals();
    }

    public void SetPosition(Vector3 position, bool isValidPosition = true)
    {
        transform.position = position;

        if (_currentState == FlagState.Move)
        {
            UpdateVisuals(isValidPosition);
            FlagPositionChanged?.Invoke(position);
        }
    }

    public void PlaceFlag()
    {
        if (_currentState != FlagState.Move)
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

    public void SetDeliveryState()//?
    {
        SetState(FlagState.DeliveryResourcesToFlag);
        UpdateVisuals(true);
    }

    public void RemoveFlag()
    {
        SetState(FlagState.Hide);
        FlagRemoved?.Invoke();
    }

    private void SetState(FlagState newState)
    {
        _currentState = newState;
        gameObject.SetActive(newState != FlagState.Hide);
    }

    private void UpdateVisuals(bool isValidPosition = true)
    {
        if (_flagRenderer == null)
            return;

        switch (_currentState)
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