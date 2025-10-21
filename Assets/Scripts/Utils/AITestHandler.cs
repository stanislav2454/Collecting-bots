using System.Collections.Generic;
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
    [SerializeField] private Material _selectedBotMaterial;

    private BotController _selectedBot;
    private Dictionary<BotController, Material> _originalBotMaterials = new Dictionary<BotController, Material>();

    private void Update()
    {
        HandleAITesting();
        HandleBotSelection();
        HandleBotDeselection();
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

    private void HandleBotSelection()
    {
        if (Input.GetMouseButtonDown(0))//todo magic number
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                BotController bot = hit.collider.GetComponent<BotController>();
                if (bot != null)
                    SelectBot(bot);
            }
        }
    }

    private void SelectBot(BotController bot)
    {
        DeselectAllBots();

        _selectedBot = bot;

        Renderer botRenderer = _selectedBot.GetComponent<Renderer>();

        if (botRenderer != null && _selectedBotMaterial != null)
        {
            if (_originalBotMaterials.ContainsKey(_selectedBot) == false)
                _originalBotMaterials[_selectedBot] = botRenderer.material;

            botRenderer.material = _selectedBotMaterial;
        }
    }

    private void DeselectAllBots()
    {
        if (_selectedBot != null)
        {
            ResetBotVisual(_selectedBot);
            _selectedBot = null;
        }
    }

    private void HandleBotDeselection()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //todo magic string
            DeselectAllBots();
    }

    private void ResetBotVisual(BotController bot)
    {
        if (bot != null)
        {
            Renderer botRenderer = bot.GetComponent<Renderer>();

            if (botRenderer != null && _originalBotMaterials.ContainsKey(bot))
            {
                botRenderer.material = _originalBotMaterials[bot];
                _originalBotMaterials.Remove(bot);
            }
            else if (botRenderer != null)
            {
                UpdateBotVisual(bot);
            }
        }
    }

    private void ToggleAI()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//todo //ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        bool anyAIEnabled = false;

        foreach (var bot in bots)
        {
            if (bot != _selectedBot)
            {
                bot.SetAIEnabled(!bot.EnableAI);
                UpdateBotVisual(bot);
            }

            if (bot.EnableAI)
                anyAIEnabled = true;
        }
    }

    private void UpdateBotVisual(BotController bot)
    {
        if (bot == _selectedBot)
            return;

        Renderer botRenderer = bot.GetComponent<Renderer>();

        if (botRenderer != null)
        {
            if (_aiDisabledMaterial != null && _aiEnabledMaterial != null)
                botRenderer.material = bot.EnableAI ? _aiEnabledMaterial : _aiDisabledMaterial;
            else
                botRenderer.material.color = bot.EnableAI ? Color.blue : Color.gray;
        }

        BotVisualIndicator indicator = bot.GetComponent<BotVisualIndicator>();

        if (indicator == null)
            indicator = bot.gameObject.AddComponent<BotVisualIndicator>();

        indicator.UpdateAIStatus(bot.EnableAI, bot.CurrentState);
    }

    private void ShowBotStatus()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//todo возможно стоит отказаться от этой функции, зачем она ?

        if (bots.Length == 0)
        {
            Debug.LogWarning("No bots found in scene!");
            return;
        }

        foreach (var bot in bots)
            Debug.Log(bot.GetBotInfo());
    }

    private void ResetAllBots()
    {
        DeselectAllBots();
        _originalBotMaterials.Clear();

        BotController[] bots = FindObjectsOfType<BotController>();//todo //ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        foreach (var bot in bots)
        {
            bot.SetAIEnabled(true);
            UpdateBotVisual(bot);
            bot.StopMovement();
        }
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;
        GUI.color = Color.red;

        GUILayout.BeginArea(new Rect(10, 410, 300, 120));

        GUILayout.Label("=== AI TEST CONTROLS ===");
        GUILayout.Label("F1: Toggle AI for all bots");
        GUILayout.Label("F2: Show bot status");
        GUILayout.Label("F3: Reset all bots");
        GUILayout.Label("LMB: Select bot");
        GUILayout.Label("ESC: Deselect bot");

        BotController[] bots = FindObjectsOfType<BotController>();// todo//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

        if (bots.Length > 0)
        {
            int activeBots = 0;
            foreach (var bot in bots)
                if (bot.EnableAI) activeBots++;

            GUILayout.Label($"Total bots: {bots.Length}");
            GUILayout.Label($"Active bots: {activeBots}/{bots.Length}");
            GUILayout.Label($"First bot: {bots[0].CurrentState}");

            if (_selectedBot != null)
            {
                GUILayout.Label($"Selected: {_selectedBot.gameObject.name}");
                GUILayout.Label($"State: {_selectedBot.CurrentState}");
                GUILayout.Label(
                    $"Inventory: {_selectedBot.BotInventory.CurrentCount}/" +
                    $"{_selectedBot.BotInventory.MaxCapacity}");
            }
            else
            {
                GUILayout.Label("No bot selected");
            }
        }

        GUILayout.EndArea();
        GUI.color = originalColor;
    }
}