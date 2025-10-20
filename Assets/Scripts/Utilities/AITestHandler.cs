using UnityEngine;

public class AITestHandler : MonoBehaviour
{
    [Header("InputKeys Settings")]
    [SerializeField] private KeyCode _toggleAIKey = KeyCode.F1;
    [SerializeField] private KeyCode _botStatusKey = KeyCode.F2;
    [SerializeField] private KeyCode _resetAllBotsKey = KeyCode.F3;

    [Header("Visual Feedback")]
    [SerializeField] private Material _aiEnabledMaterial;
    [SerializeField] private Material _aiDisabledMaterial;

    private void Update()
    {
        HandleAITesting();
    }

    private void HandleAITesting()
    {
        if (Input.GetKeyDown(_toggleAIKey))
            ToggleAI();

        if (Input.GetKeyDown(_botStatusKey))
            ShowBotStatus();

        if (Input.GetKeyDown(_resetAllBotsKey))
            ResetAllBots();
    }

    private void ToggleAI()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//todo
        bool anyAIEnabled = false;

        foreach (var bot in bots)
        { // Используем метод вместо прямого доступа к полю
            bot.SetAIEnabled(bot.EnableAI == false);
            // bot._enableAI = bot._enableAI == false;

            UpdateBotVisual(bot);

            if (bot.EnableAI)
                //if (bot._enableAI)
                anyAIEnabled = true;
        }
        string status = anyAIEnabled ? "ENABLED" : "DISABLED";
        Debug.Log($"All bots AI: {status}");
    }

    private void UpdateBotVisual(BotController bot)
    {// не будет работать, т.к. этот компонент не находиться на боте ?
        Renderer botRenderer = bot.GetComponent<Renderer>();

        if (botRenderer != null)
        {
            if (_aiDisabledMaterial != null && _aiEnabledMaterial != null)
            {
                botRenderer.material = bot.EnableAI ? _aiEnabledMaterial : _aiDisabledMaterial;
              //  Debug.Log($"Changed material for {bot.gameObject.name} to {(bot.EnableAI ? "ENABLED" : "DISABLED")}");
            }
            else
            {
                botRenderer.material.color = bot.EnableAI ? Color.blue : Color.gray;
                Debug.Log($"Changed color for {bot.gameObject.name} to {(bot.EnableAI ? "BLUE" : "GRAY")}");
            }
        }

        // Добавляем дополнительную визуализацию - текстовый элемент
        BotVisualIndicator indicator = bot.GetComponent<BotVisualIndicator>();
        if (indicator == null)
        {
            indicator = bot.gameObject.AddComponent<BotVisualIndicator>();
            Debug.Log($"✅ Added BotVisualIndicator to {bot.gameObject.name}");
        }

        indicator.UpdateAIStatus(bot.EnableAI, bot.CurrentState);
    }

    private void ShowBotStatus()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//
        Debug.Log("=== BOT STATUS ===");

        if (bots.Length == 0)
        {
            Debug.LogWarning("No bots found in scene!");
            return;
        }

        foreach (var bot in bots)
            Debug.Log(bot.GetBotInfo());
        //{string aiStatus = bot.EnableAI ? "ACTIVE" : "DISABLED";
        //    Debug.Log($"Bot: {bot.gameObject.name} | AI: {aiStatus} | State: {bot.CurrentState} | Inventory: {bot.botInventory.CurrentCount}/{bot.botInventory.maxCapacity}");}
    }

    private void ResetAllBots()
    {
        BotController[] bots = FindObjectsOfType<BotController>();
        foreach (var bot in bots)
        {
            bot.SetAIEnabled(true); // Используем метод
            UpdateBotVisual(bot);
            bot.StopMovement();
        }
        Debug.Log("All bots reset to default state");
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;
        GUI.color = Color.red; // Устанавливаем красный цвет для всего последующего GUI

        GUILayout.BeginArea(new Rect(10, 410, 300, 120));

        GUILayout.Label("=== AI TEST CONTROLS ===");
        GUILayout.Label("F1: Toggle AI for all bots");
        GUILayout.Label("F2: Show bot status");
        GUILayout.Label("F3: Reset all bots");

        BotController[] bots = FindObjectsOfType<BotController>();// todo

        if (bots.Length > 0)
        {
            int activeBots = 0;
            foreach (var bot in bots)
                if (bot.EnableAI) activeBots++;

            GUILayout.Label($"Total bots: {bots.Length}");
            GUILayout.Label($"Active bots: {activeBots}/{bots.Length}");
            GUILayout.Label($"First bot: {bots[0].CurrentState}");
        }

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}