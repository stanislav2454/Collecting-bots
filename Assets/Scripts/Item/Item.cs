using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    private Renderer _itemRenderer;
    private Collider _itemCollider;

    public event Action<Item> Collected;

    [field: Header("Item Settings")]
    [field: SerializeField] public int Value { get; private set; } = 1;
    public bool CanBeCollected { get; private set; } = true;//- У ресурса не должно быть CanBeCollected. 

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
}