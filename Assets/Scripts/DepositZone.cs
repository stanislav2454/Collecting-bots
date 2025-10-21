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

    private void Awake()
    {
        _zoneRenderer = GetComponent<Renderer>();
        UpdateVisuals(true);
    }

    private void OnTriggerEnter(Collider other)// зачем нужен ? если не используется - удалить !
    {
        BotController bot = other.GetComponent<BotController>();

        if (bot != null)
        {
            // Бот сам обработает депозит через свое состояние
        }
    }

    public string GetZoneInfo() =>// зачем нужен ? если не используется - удалить !
         $"Deposit Zone: {_totalItemsDeposited} items, {_totalPointsEarned} points";

    public bool ProcessDeposit(BotInventory botInventory)
    {
        if (botInventory == null || botInventory.CurrentCount == 0)
            return false;

        int itemsToDeposit = botInventory.CurrentCount;
        int pointsEarned = itemsToDeposit * _pointsPerItem;

        _totalItemsDeposited += itemsToDeposit;
        _totalPointsEarned += pointsEarned;

        botInventory.ClearInventory();

        StartCoroutine(DepositEffect());

        return true;
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

    private void OnDrawGizmos()
    {
        if (_debugVisual == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _depositRadius);
    }
}