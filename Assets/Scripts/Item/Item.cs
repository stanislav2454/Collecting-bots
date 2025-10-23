using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private int _value = 1;

    private bool _canBeCollected = true;
    private Renderer _itemRenderer;
    private Collider _itemCollider;

    public event Action<Item> Collected;

    public bool CanBeCollected => _canBeCollected;

    private void Awake()
    {
        TryGetComponent(out _itemRenderer);
        TryGetComponent(out _itemCollider);
    }

    public void Collect()
    {
        if (_canBeCollected == false)
            return;

        _canBeCollected = false;
        Collected?.Invoke(this);
    }

    public void AttachToBot(Transform botTransform)
    {
        transform.SetParent(botTransform);
        transform.localPosition = new Vector3(0, BotConstants.BotCarryHeight, 0);
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(true);
    }

    public void PrepareForRespawn()
    {
        _canBeCollected = true;
        transform.SetParent(null);

        if (_itemRenderer != null)
            _itemRenderer.enabled = true;
        if (_itemCollider != null)
            _itemCollider.enabled = true;
    }
}