using UnityEngine;

public class CycleTestHandler : MonoBehaviour
{
    [Header("Cycle Testing")]
    [SerializeField] private KeyCode _testCycleKey = KeyCode.F8;
    [SerializeField] private bool _logDetailedCycle = true;

    private void Update()
    {
        if (Input.GetKeyDown(_testCycleKey))
            TestFullCycle();
    }

    private void TestFullCycle()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//

        int botsWithItems = 0;
        int botsInCycle = 0;
        int idleBots = 0;
        int searchingBots = 0;
        int collectingBots = 0;
        int movingBots = 0;

        foreach (var bot in bots)
        {
            if (bot.BotInventory.CurrentCount > 0)
                botsWithItems++;

            if (bot.EnableAI && bot.CurrentState != BotState.Idle)
                botsInCycle++;

            if (_logDetailedCycle)
            {
                switch (bot.CurrentState)
                {
                    case BotState.Idle:
                        idleBots++;
                        break;

                    case BotState.Search:
                        searchingBots++;
                        break;

                    case BotState.Collect:
                        collectingBots++;
                        break;

                    case BotState.MoveToItem://?

                    case BotState.MoveToDeposit:
                        movingBots++;
                        break;
                }
            }
        }
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;
        GUI.color = Color.green;

        GUILayout.BeginArea(new Rect(320, 410, 350, 120));

        GUILayout.Label("=== CYCLE TESTING ===");
        GUILayout.Label("F8: Test full cycle status");

        BotController[] bots = FindObjectsOfType<BotController>();
        int botsInCycle = 0;
        int botsWithItems = 0;

        foreach (var bot in bots)
        {
            if (bot.EnableAI && bot.CurrentState != BotState.Idle)
                botsInCycle++;
            if (bot.BotInventory.CurrentCount > 0)
                botsWithItems++;
        }

        GUILayout.Label($"Bots in cycle: {botsInCycle}/{bots.Length}");
        GUILayout.Label($"Bots with items: {botsWithItems}/{bots.Length}");
        GUILayout.Label($"Active collectors: {botsInCycle - botsWithItems}");

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}