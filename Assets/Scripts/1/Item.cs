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
//using UnityEngine;

//public class Item : MonoBehaviour, IItem
//{
//    [Header("Item Settings")]
//    [SerializeField] private ItemData _itemData;

//    [Header("Visual Components")]
//    [SerializeField] private Renderer _itemRenderer;
//    [SerializeField] private Collider _itemCollider;

//    public string ItemId => _itemData?.itemId ?? "unknown";
//    public string ItemName => _itemData?.itemName ?? "Unknown Item";
//    public bool CanBeCollected { get; private set; } = true;

//    public ItemData ItemData
//    { // Свойства для доступа с инкапсуляцией
//        get => _itemData;
//        set
//        {
//            _itemData = value;// зачем прямой доступ? - нарушение инкапсуляции !
//            UpdateItemVisuals();
//        }
//    }

//    private void Start()
//    {
//        InitializeItem();
//    }

//    private void OnValidate()
//    {
//        if (_itemData != null && _itemRenderer != null)
//            UpdateItemVisuals();
//    }

//    private void OnDestroy()
//    {
//        if (_itemRenderer != null && _itemRenderer.material != null)
//            DestroyImmediate(_itemRenderer.material);
//    }

//    public void OnCollected()
//    {
//        if (CanBeCollected == false)
//            return;

//        CanBeCollected = false;

//        if (_itemRenderer != null)
//            _itemRenderer.enabled = false;

//        if (_itemCollider != null)
//            _itemCollider.enabled = false;

//        if (_itemData != null && _itemData.respawnTime > 0)
//            Invoke(nameof(OnRespawn), _itemData.respawnTime);// переделать на корутину!
//    }

//    public int GetValue() =>// зачем прямой доступ? - нарушение инкапсуляции !
//        _itemData?.value ?? 1;

//    public ItemType GetItemType() =>// зачем прямой доступ? - нарушение инкапсуляции !
//         _itemData?.itemType ?? ItemType.Resource;

//    public void OnRespawn()
//    {
//        CanBeCollected = true;

//        if (_itemRenderer != null)
//            _itemRenderer.enabled = true;

//        if (_itemCollider != null)
//            _itemCollider.enabled = true;
//    }

//    private void InitializeItem()
//    {
//        if (_itemRenderer == null)
//            _itemRenderer = GetComponent<Renderer>();// доделать с атрибутом или переделать на Debug.LogError()

//        if (_itemCollider == null)
//            _itemCollider = GetComponent<Collider>();// доделать с атрибутом или переделать на Debug.LogError()

//        UpdateItemVisuals();

//        if (_itemData != null && _itemRenderer != null)
//        {
//            Material material = _itemRenderer.material;
//            if (material != null)
//                material.color = _itemData.itemColor;
//        }

//        gameObject.layer = LayerMask.NameToLayer("Items");
//    }

//    private void UpdateItemVisuals()
//    {
//        if (_itemData != null && _itemRenderer != null)
//        {
//            Material material = new Material(_itemRenderer.material);
//            material.color = _itemData.itemColor;
//            _itemRenderer.material = material;

//            if (_itemData.visualPrefab != null)
//                InstantiateVisualPrefab();
//        }
//    }

//    private void InstantiateVisualPrefab()
//    {
//        foreach (Transform child in transform)
//            if (child.CompareTag("Visual"))
//                Destroy(child.gameObject);

//        GameObject visual = Instantiate(_itemData.visualPrefab, transform);
//        visual.tag = "Visual";
//        visual.transform.localPosition = Vector3.zero;
//        visual.transform.localRotation = Quaternion.identity;
//    }

//    private void OnDrawGizmos()
//    {
//        if (Application.isPlaying == false)
//            return;

//        Color originalColor = GUI.color;

//        if (CanBeCollected)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireSphere(transform.position, 0.5f);
//        }
//        else
//        {
//            Gizmos.color = Color.gray;
//            Gizmos.DrawWireSphere(transform.position, 0.3f);
//        }

//#if UNITY_EDITOR
//        GUI.color = new Color(1f, 0.5f, 0f);

//        string info = $"{ItemName}\nValue: {GetValue()}\nType: {GetItemType()}";
//        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, info);

//        GUI.color = originalColor;
//#endif
//    }
//}