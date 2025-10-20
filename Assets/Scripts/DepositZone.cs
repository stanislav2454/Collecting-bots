using UnityEngine;

public class DepositZone : MonoBehaviour
{
    [Header("Deposit Zone Settings")]
    public float depositRadius = 2f;
    public bool debugVisual = true;

    private void OnTriggerEnter(Collider other)
    {
        BotController bot = other.GetComponent<BotController>();
        if (bot != null)
        {
            Debug.Log($"Bot {bot.gameObject.name} entered deposit zone");
        }
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        if (debugVisual == false)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, depositRadius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 3, "DepositIcon", true);
    }
}