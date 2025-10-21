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
    //private Material _originalBotMaterial;
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
        if (Input.GetMouseButtonDown(0))//todo
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
    {// Сбрасываем предыдущее выделение
        DeselectAllBots();
        //// Сбрасываем предыдущее выделение
        //if (_selectedBot != null)
        //    ResetBotVisual(_selectedBot);

        _selectedBot = bot;

        // Применяем материал выделения
        Renderer botRenderer = _selectedBot.GetComponent<Renderer>();
        if (botRenderer != null && _selectedBotMaterial != null)
        {            // Сохраняем оригинальный материал в словарь
            if (_originalBotMaterials.ContainsKey(_selectedBot) == false)
                _originalBotMaterials[_selectedBot] = botRenderer.material;

            // _originalBotMaterial = botRenderer.material;
            botRenderer.material = _selectedBotMaterial;
        }

        Debug.Log($"Selected bot: {bot.gameObject.name}");
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
        if (Input.GetKeyDown(KeyCode.Escape))//todo
            DeselectAllBots();
    }

    private void ResetBotVisual(BotController bot)
    {
        if (bot != null)
        {
            Renderer botRenderer = bot.GetComponent<Renderer>();
            //if (botRenderer != null && _originalBotMaterial != null)
            //    botRenderer.material = _originalBotMaterial;
            if (botRenderer != null && _originalBotMaterials.ContainsKey(bot))
            {// Возвращаем оригинальный материал
                botRenderer.material = _originalBotMaterials[bot];
                _originalBotMaterials.Remove(bot);
            }
            else if (botRenderer != null)
            { // Fallback: если материала нет в словаре, используем стандартную логику
                UpdateBotVisual(bot);
            }
        }
    }

    private void ToggleAI()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//todo
        bool anyAIEnabled = false;

        foreach (var bot in bots)
        { // Пропускаем выделенного бота чтобы не сбрасывать его материал
            if (bot != _selectedBot)
            {
                bot.SetAIEnabled(!bot.EnableAI);
                UpdateBotVisual(bot);
            }

            if (bot.EnableAI)
                //if (bot._enableAI)
                anyAIEnabled = true;
        }
        string status = anyAIEnabled ? "ENABLED" : "DISABLED";
        Debug.Log($"All bots AI: {status}");
    }

    private void UpdateBotVisual(BotController bot)
    {
        // Не обновляем визуал если бот выделен
        if (bot == _selectedBot) 
            return;
        
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
        BotController[] bots = FindObjectsOfType<BotController>();//todo
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
    {// Сбрасываем выделение перед сбросом всех ботов
        DeselectAllBots();
        _originalBotMaterials.Clear();

        BotController[] bots = FindObjectsOfType<BotController>();//todo
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
        GUILayout.Label("LMB: Select bot");
        GUILayout.Label("ESC: Deselect bot");

        BotController[] bots = FindObjectsOfType<BotController>();// todo

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