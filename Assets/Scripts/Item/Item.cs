using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private int _value = 1;

    private Renderer _itemRenderer;
    private Collider _itemCollider;

    public event Action<Item> Collected;

    public bool CanBeCollected { get; private set; } = true;
    public int GetValue => _value;

    private void Awake()
    {
        TryGetComponent(out _itemRenderer);
        TryGetComponent(out _itemCollider);
    }

    public void Collect()
    {
        if (CanBeCollected == false)
            return;

        CanBeCollected = false;
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
        CanBeCollected = true;
        transform.SetParent(null);
        gameObject.SetActive(true);

        if (_itemRenderer != null)
            _itemRenderer.enabled = true;

        if (_itemCollider != null)
            _itemCollider.enabled = true;
    }

    public void ResetForPool()// если не нужен - удалить !
    {
        CanBeCollected = true;
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}