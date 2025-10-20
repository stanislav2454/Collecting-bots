using UnityEngine;

public class DepositZone : MonoBehaviour
{
    [Header("Deposit Zone Settings")]
    [SerializeField] private float depositRadius = 2f;
    [SerializeField] private bool debugVisual = true;

    private void OnTriggerEnter(Collider other)
    {
        BotController bot = other.GetComponent<BotController>();
        if (bot != null)
        {
            Debug.Log($"Bot {bot.gameObject.name} entered deposit zone");
        }
    }
    
    private void OnDrawGizmos()// Для отладки
    {
        if (debugVisual == false)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 3, "DepositIcon", true);
    }
}