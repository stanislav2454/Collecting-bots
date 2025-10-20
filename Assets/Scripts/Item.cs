using UnityEngine;

public class Item : MonoBehaviour, IItem
{
    [Header("Item Settings")]
    [SerializeField] private ItemData _itemData;

    [Header("Visual Components")]
    [SerializeField] private Renderer _itemRenderer;
    [SerializeField] private Collider _itemCollider;

    public string ItemId => _itemData?.itemId ?? "unknown";
    public string ItemName => _itemData?.itemName ?? "Unknown Item";
    public bool CanBeCollected { get; private set; } = true;

    public ItemData ItemData
    { // Свойства для доступа с инкапсуляцией
        get => _itemData;
        set
        {
            _itemData = value;// ? инкапсуляция ?
            UpdateItemVisuals();
        }
    }

    private void Start()
    {
        InitializeItem();
    }

    private void OnValidate()
    {// Автоматическое обновление в редакторе при изменении ItemData
        if (_itemData != null && _itemRenderer != null)
            UpdateItemVisuals();
    }

    private void OnDestroy()
    { // Очистка созданных материалов
        if (_itemRenderer != null && _itemRenderer.material != null)
            DestroyImmediate(_itemRenderer.material);
    }

    public void OnCollected()
    {
        if (CanBeCollected == false)
            return;

        CanBeCollected = false;

        // Визуально скрываем предмет
        if (_itemRenderer != null)
            _itemRenderer.enabled = false;

        if (_itemCollider != null)
            _itemCollider.enabled = false;

        Debug.Log($"Item {ItemName} collected");

        // Запускаем респаун через некоторое время
        if (_itemData != null && _itemData.respawnTime > 0)
            Invoke(nameof(OnRespawn), _itemData.respawnTime);// переделать через корутину!
    }

    public int GetValue() =>// ? why ?
        _itemData?.value ?? 1;

    public ItemType GetItemType() =>// ? why ?
         _itemData?.itemType ?? ItemType.Resource;

    public void OnRespawn()
    {
        CanBeCollected = true;

        // Восстанавливаем визуал
        if (_itemRenderer != null)
            _itemRenderer.enabled = true;

        if (_itemCollider != null)
            _itemCollider.enabled = true;

        Debug.Log($"Item {ItemName} respawned");
    }

    private void InitializeItem()
    {
        if (_itemRenderer == null)
            _itemRenderer = GetComponent<Renderer>();// доделать

        if (_itemCollider == null)
            _itemCollider = GetComponent<Collider>();// доделать

        UpdateItemVisuals();

        // Устанавливаем цвет предмета из ItemData
        if (_itemData != null && _itemRenderer != null)
        {
            Material material = _itemRenderer.material;
            if (material != null)
                material.color = _itemData.itemColor;
        }

        //// Устанавливаем слой для предметов
        //gameObject.layer = 7; // Item layer
        gameObject.layer = LayerMask.NameToLayer("Items");
    }

    private void UpdateItemVisuals()
    {
        if (_itemData != null && _itemRenderer != null)
        {
            // Создаем новый материал чтобы не менять оригинальный asset
            Material material = new Material(_itemRenderer.material);
            material.color = _itemData.itemColor;
            _itemRenderer.material = material;

            // Если есть префаб для визуала - можно его инстанциировать
            if (_itemData.visualPrefab != null)
                InstantiateVisualPrefab(); // Дополнительная логика для кастомных визуалов
        }
    }

    private void InstantiateVisualPrefab()
    {
        // Очищаем старые визуальные children
        foreach (Transform child in transform)
            if (child.CompareTag("Visual"))
                Destroy(child.gameObject);

        // Создаем новый визуал
        GameObject visual = Instantiate(_itemData.visualPrefab, transform);
        visual.tag = "Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        if (CanBeCollected)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        // Показываем информацию о предмете
#if UNITY_EDITOR
        string info = $"{ItemName}\nValue: {GetValue()}\nType: {GetItemType()}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, info);
#endif
    }
}