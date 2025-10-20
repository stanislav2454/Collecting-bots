using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public bool debugVisual = true;

    private void OnDrawGizmos()
    {
        if (debugVisual == false)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 2, "ItemIcon", true);
    }
}