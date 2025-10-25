using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private int _value = 1;

    private Renderer _itemRenderer;
    private Collider _itemCollider;
    private bool _isRegistered = false;

    public event Action<Item> Collected;

    public bool CanBeCollected { get; private set; } = true;
    public int GetValue => _value;

    private void Awake()
    {
        TryGetComponent(out _itemRenderer);
        TryGetComponent(out _itemCollider);
    }

    public void RegisterWithPosition()
    {
        if (_isRegistered == false && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RegisterResource(this);
            _isRegistered = true;
            Debug.Log($"✅ Ресурс зарегистрирован с позицией: {transform.position}");
        }
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
        Vector3 oldPosition = transform.position;

        transform.SetParent(botTransform);
        transform.localPosition = new Vector3(0, BotConstants.BotCarryHeight, 0);
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
    }

    public void PrepareForRespawn()
    {
        CanBeCollected = true;
        _isRegistered = false;
        transform.SetParent(null);

        gameObject.SetActive(true);

        if (_itemRenderer != null)
            _itemRenderer.enabled = true;
        if (_itemCollider != null)
            _itemCollider.enabled = true;

        Debug.Log($"🔄 Ресурс подготовлен к респавну: {name}, CanBeCollected: {CanBeCollected}, Active: {gameObject.activeInHierarchy}");
    }

    public void ResetForPool()
    {
        CanBeCollected = true;
        _isRegistered = false;
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}