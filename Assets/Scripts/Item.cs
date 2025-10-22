using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    public event Action<Item> Collected;
    public event Action<Item> Respawned;

    [Header("Item Settings")]
    [SerializeField] private float _respawnTime = 5f;
    [SerializeField] private int _value = 1;

    private Renderer _itemRenderer;
    private Collider _itemCollider;
    private bool _canBeCollected = true;

    public bool CanBeCollected => _canBeCollected;
    public int Value => _value;

    private void Awake()
    {
        if (TryGetComponent(out _itemRenderer) && TryGetComponent(out _itemCollider))
        {
            // Компоненты найдены
        }
    }

    public void Collect()
    {
        if (_canBeCollected == false) 
            return;

        _canBeCollected = false;
        SetVisualsActive(false);
        Collected?.Invoke(this);

        Invoke(nameof(Respawn), _respawnTime);
    }

    private void Respawn()
    {
        _canBeCollected = true;
        SetVisualsActive(true);
        Respawned?.Invoke(this);
    }

    private void SetVisualsActive(bool active)
    {
        if (_itemRenderer != null) 
            _itemRenderer.enabled = active;

        if (_itemCollider != null) 
            _itemCollider.enabled = active;
    }
}