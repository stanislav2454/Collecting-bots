using UnityEngine;
using UnityEngine.AI;

public class BotManager : MonoBehaviour
{
    [Header("Bot Prefab")]
    public GameObject botPrefab;

    [Header("Spawn Settings")]
    public int initialBotsCount = 1;
    public Vector3 spawnArea = new Vector3(5f, 0f, 5f);

    private void Start()
    {
        SpawnInitialBots();
    }

    private void SpawnInitialBots()
    {
        for (int i = 0; i < initialBotsCount; i++)
        {
            SpawnBot();
        }
    }

    public GameObject SpawnBot()
    {
        if (botPrefab == null)
        {
            Debug.LogError("Bot prefab not assigned in BotManager!");
            return null;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity, transform);
        bot.name = $"Bot_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

        // Гарантируем что у бота есть коллайдер
        if (bot.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
        }

        bot.layer = 6; // Bot layer// Устанавливаем слой бота

        Debug.Log($"Spawned bot: {bot.name} at position {spawnPosition}");
        return bot;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.z, spawnArea.z));

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            return hit.position;

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, 0.1f, spawnArea.z * 2));
    }
}