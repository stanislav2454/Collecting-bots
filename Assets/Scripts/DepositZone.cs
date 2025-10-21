using UnityEngine;

public class DepositZone : MonoBehaviour
{
    [Header("Deposit Zone Settings")]
    [SerializeField] private float _depositRadius = 2f;
    [SerializeField] private int _pointsPerItem = 10;
    [SerializeField] private bool _debugVisual = true;


    [Header("Visual Feedback")]
    [SerializeField] private Material _activeMaterial;
    [SerializeField] private Material _inactiveMaterial;

    private Renderer _zoneRenderer;
    private int _totalItemsDeposited = 0;
    private int _totalPointsEarned = 0;

    // public Vector3 Position => transform.position;

    private void Awake()
    {
        _zoneRenderer = GetComponent<Renderer>();
        UpdateVisuals(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        BotController bot = other.GetComponent<BotController>();

        if (bot != null)
        {
            Debug.Log($"Bot {bot.gameObject.name} entered deposit zone");
            // Бот сам обработает депозит через свое состояние
        }
    }

    public string GetZoneInfo() =>
         $"Deposit Zone: {_totalItemsDeposited} items, {_totalPointsEarned} points";

    public bool ProcessDeposit(BotInventory botInventory)
    {
        if (botInventory == null || botInventory.CurrentCount == 0)
            return false;

        int itemsToDeposit = botInventory.CurrentCount;
        int pointsEarned = itemsToDeposit * _pointsPerItem;

        // Сохраняем статистику
        _totalItemsDeposited += itemsToDeposit;
        _totalPointsEarned += pointsEarned;

        // Очищаем инвентарь бота
        botInventory.ClearInventory();

        Debug.Log($"Deposited {itemsToDeposit} items for {pointsEarned} points");
        Debug.Log($"Total: {_totalItemsDeposited} items, {_totalPointsEarned} points");

        // Визуальная обратная связь
        StartCoroutine(DepositEffect());

        return true;
        //if (botInventory == null || botInventory.IsEmpty)
        //    return;

        //int itemsCount = botInventory.CurrentCount;
        //int totalValue = CalculateTotalValue(botInventory);

        //botInventory.ClearInventory();

        //Debug.Log($"Deposited {itemsCount} items with total value: {totalValue}");
    }

    private System.Collections.IEnumerator DepositEffect()
    {
        UpdateVisuals(false);
        yield return new WaitForSeconds(0.5f);
        UpdateVisuals(true);
    }

    private void UpdateVisuals(bool isActive)
    {
        if (_zoneRenderer != null && _activeMaterial != null && _inactiveMaterial != null)
            _zoneRenderer.material = isActive ? _activeMaterial : _inactiveMaterial;
    }

    //private int CalculateTotalValue(BotInventory botInventory) =>
    //     // В будущем можно учитывать стоимость каждого предмета
    //     botInventory.CurrentCount * 10; // Базовая стоимость    

    private void OnDrawGizmos()// Для отладки
    {
        if (_debugVisual == false)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _depositRadius);

        const int VertPosMultyplier = 3;
        Gizmos.color = new Color(0, 1, 0);
        //Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position + Vector3.up * VertPosMultyplier, _depositRadius);
        //Gizmos.color = Color.magenta;
        //Gizmos.DrawWireSphere(transform.position, _depositRadius);
        //Gizmos.DrawIcon(transform.position + Vector3.up * 3, "DepositIcon", true);
    }
}