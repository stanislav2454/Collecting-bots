using UnityEngine;

public class AITestHandler : MonoBehaviour
{
    private void Update()
    {
        HandleAITesting();
    }

    private void HandleAITesting()
    {
        if (Input.GetKeyDown(KeyCode.F1))        
            ToggleAI();        

        if (Input.GetKeyDown(KeyCode.F2))        
            ShowBotStatus();        
    }

    private void ToggleAI()
    {
        BotController[] bots = FindObjectsOfType<BotController>();
        foreach (var bot in bots)
        {
            bot.enableAI = !bot.enableAI;
            Debug.Log($"Bot {bot.gameObject.name} AI: {bot.enableAI}");
        }
    }

    private void ShowBotStatus()
    {
        BotController[] bots = FindObjectsOfType<BotController>();
        Debug.Log("=== BOT STATUS ===");

        foreach (var bot in bots)
        {
            Debug.Log($"Bot: {bot.gameObject.name} | State: {bot.CurrentState} | Inventory: {bot.botInventory.CurrentCount}/{bot.botInventory.maxCapacity} | AI: {bot.enableAI}");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 410, 300, 120));

        GUILayout.Label("=== AI TEST CONTROLS ===");
        GUILayout.Label("F1: Toggle AI for all bots");
        GUILayout.Label("F2: Show bot status");

        BotController[] bots = FindObjectsOfType<BotController>();
        if (bots.Length > 0)
        {
            GUILayout.Label($"Total bots: {bots.Length}");
            GUILayout.Label($"First bot state: {bots[0].CurrentState}");
        }

        GUILayout.EndArea();
    }
}