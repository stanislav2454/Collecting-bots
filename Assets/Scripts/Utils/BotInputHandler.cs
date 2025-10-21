using UnityEngine;
using System.Collections.Generic;

public class BotInputHandler : MonoBehaviour
{
    [Header("Control Mode")]
    [SerializeField] private bool _manualControlMode = false;
    [SerializeField] private KeyCode _toggleControlModeKey = KeyCode.Tab;

    [Header("Input Keys - Basic Controls")]
    [SerializeField] private KeyCode _botSpawnKey = KeyCode.B;
    [SerializeField] private KeyCode _botDeselectionKey = KeyCode.Escape;
    [SerializeField] private KeyCode _cameraResetKey = KeyCode.R;

    [Header("Input Keys - AI Controls")]
    [SerializeField] private KeyCode _toggleAIKey = KeyCode.F1;
    [SerializeField] private KeyCode _resetAllBotsKey = KeyCode.F3;

    [Header("Layer Names")]
    [SerializeField] private string _groundLayerName = "Ground";
    [SerializeField] private string _botLayerName = "Bot";

    [Header("Visual Feedback")]
    [SerializeField] private Material _selectedBotMaterial;
    [SerializeField] private Material _aiEnabledMaterial;
    [SerializeField] private Material _aiDisabledMaterial;
    [SerializeField] private Material _manualControlMaterial;

    [Header("UI Settings")]
    [SerializeField] private int _headerFontSize = 14;
    [SerializeField] private int _normalFontSize = 12;
    [SerializeField] private int _highlightFontSize = 16;
    [SerializeField] private int _modePanelFontSize = 18;

    private BotController _selectedBot;
    private Dictionary<BotController, Material> _originalBotMaterials = new Dictionary<BotController, Material>();
    private LayerMask _groundLayer;
    private LayerMask _botLayer;

    private IBotService _botService;
    private IItemService _itemService;// где используется ?
    private IDepositService _depositService;// где используется ?

    private void Start()
    {
        _groundLayer = 1 << LayerMask.NameToLayer(_groundLayerName);
        _botLayer = 1 << LayerMask.NameToLayer(_botLayerName);

        if (LayerMask.NameToLayer(_groundLayerName) == -1)
            Debug.LogError($"Layer '{_groundLayerName}' not found! Please create this layer in Project Settings.");

        if (LayerMask.NameToLayer(_botLayerName) == -1)
            Debug.LogError($"Layer '{_botLayerName}' not found! Please create this layer in Project Settings.");

        InitializeServices();
    }

    private void InitializeServices()
    {
        if (ServiceLocator.TryGet<IBotService>(out _botService))
        {
            _botService.BotSelected += OnBotSelected;
            _botService.BotDeselected += OnBotDeselected;
        }

        ServiceLocator.TryGet<IItemService>(out _itemService);
        ServiceLocator.TryGet<IDepositService>(out _depositService);
    }

    private void Update()
    {
        HandleControlModeToggle();
        HandleBotSelection();
        HandleBotMovement();
        HandleBotSpawning();
        HandleBotDeselection();
        HandleAITesting();
        HandleCameraReset();
    }

    private void HandleControlModeToggle()
    {
        if (Input.GetKeyDown(_toggleControlModeKey))
        {
            _manualControlMode = !_manualControlMode;

            if (_selectedBot != null)
                UpdateSelectedBotVisual();
        }
    }

    private void UpdateSelectedBotVisual()
    {
        if (_selectedBot == null)
            return;

        _selectedBot.TryGetComponent(out Renderer botRenderer);// ОБРАЗЕЦ использования TryGetComponent, вместо GetComponent

        if (botRenderer != null)
        {
            if (_manualControlMode)
            {
                if (_originalBotMaterials.ContainsKey(_selectedBot) == false)
                    _originalBotMaterials[_selectedBot] = botRenderer.material;

                if (_manualControlMaterial != null)
                    botRenderer.material = _manualControlMaterial;
            }
            else
            {
                if (!_originalBotMaterials.ContainsKey(_selectedBot))
                    _originalBotMaterials[_selectedBot] = botRenderer.material;

                if (_selectedBotMaterial != null)
                    botRenderer.material = _selectedBotMaterial;
            }
        }
    }

    private void HandleBotSelection()
    {
        if (Input.GetMouseButtonDown(0))//сделать: магическое число
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _botLayer))
            {
                BotController bot = hit.collider.GetComponent<BotController>();

                if (bot != null)
                {
                    SelectBot(bot);
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
                DeselectAllBots();
        }
    }

    private void HandleBotMovement()
    {
        if (Input.GetMouseButtonDown(1) && _selectedBot != null)//todo: magic number
        {
            if (_manualControlMode)
                _selectedBot.SetAIEnabled(false);

            if (_selectedBot.EnableAI == false)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
                    _selectedBot.MoveToPosition(hit.point);
            }
        }
    }

    private void HandleBotSpawning()
    {
        if (Input.GetKeyDown(_botSpawnKey))
            SpawnBotAtRandomPosition();
    }

    private void HandleBotDeselection()
    {
        if (Input.GetKeyDown(_botDeselectionKey))
            DeselectAllBots();
    }

    private void HandleCameraReset()
    {
        if (Input.GetKeyDown(_cameraResetKey))
            ResetCamera();
    }

    private void HandleAITesting()
    {
        if (Input.GetKeyDown(_toggleAIKey))
            ToggleAI();

        if (Input.GetKeyDown(_resetAllBotsKey))
            ResetAllBots();
    }

    private void ToggleAI()
    {
        if (_botService != null)
        {
            BotController[] bots = _botService.GetAllBots();
            bool anyAIEnabled = false;

            foreach (var bot in bots)
            {
                if (_manualControlMode && bot == _selectedBot)
                    continue;

                bot.SetAIEnabled(!bot.EnableAI);
                UpdateBotVisual(bot);

                if (bot.EnableAI)
                    anyAIEnabled = true;
            }
        }
        else
        {
            ToggleAIFallback();
        }
    }

    private void ToggleAIFallback()
    {
        BotController[] bots = FindObjectsOfType<BotController>();//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую
        bool anyAIEnabled = false;

        foreach (var bot in bots)
        {
            if (_manualControlMode && bot == _selectedBot)
                continue;

            bot.SetAIEnabled(!bot.EnableAI);
            UpdateBotVisual(bot);

            if (bot.EnableAI)
                anyAIEnabled = true;
        }
    }

    private void ResetAllBots()
    {
        DeselectAllBots();
        _originalBotMaterials.Clear();

        if (_botService != null)
        {
            _botService.ResetAllBots();
        }
        else
        {
            BotController[] bots = FindObjectsOfType<BotController>();//ресурсозатратно и ненадежно => переделать на передачу ссылки напрямую

            foreach (var bot in bots)
            {
                bot.SetAIEnabled(true);
                UpdateBotVisual(bot);
                bot.StopMovement();
            }
        }
    }

    private void SelectBot(BotController bot)
    {
        if (_botService != null)
        {
            _botService.SelectBot(bot);
        }
        else
        {
            DeselectAllBots();
            _selectedBot = bot;

            if (_manualControlMode)
                _selectedBot.SetAIEnabled(false);

            UpdateSelectedBotVisual();
        }
    }

    private void DeselectAllBots()
    {
        if (_botService != null)
        {
            _botService.DeselectAllBots();
        }
        else
        {
            if (_selectedBot != null)
            {
                if (_manualControlMode)
                    _selectedBot.SetAIEnabled(true);

                ResetBotVisual(_selectedBot);
                _selectedBot = null;
            }
        }
    }

    private void OnBotSelected(BotController bot)
    {
        _selectedBot = bot;

        if (_manualControlMode)
            _selectedBot.SetAIEnabled(false);

        UpdateSelectedBotVisual();
    }

    private void OnBotDeselected(BotController bot)
    {
        if (_selectedBot == bot)
        {
            if (_manualControlMode)
                _selectedBot.SetAIEnabled(true);

            ResetBotVisual(_selectedBot);
            _selectedBot = null;
        }
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

    private void UpdateBotVisual(BotController bot)
    {
        if (bot == _selectedBot && _manualControlMode)
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
        {
            indicator = bot.gameObject.AddComponent<BotVisualIndicator>();
        }
        indicator.UpdateAIStatus(bot.EnableAI, bot.CurrentState);
    }

    private void SpawnBotAtRandomPosition()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        if (_botService != null)
            _botService.SpawnBot(spawnPosition);
    }

    private Vector3 GetRandomSpawnPosition() =>
         new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

    private void ResetCamera()
    {
        Camera.main.transform.position = new Vector3(0, 15, -10);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
    }

    private void OnDestroy()
    {
        if (_botService != null)
        {
            _botService.BotSelected -= OnBotSelected;
            _botService.BotDeselected -= OnBotDeselected;
        }
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = _headerFontSize;

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = _normalFontSize;

        GUIStyle highlightStyle = new GUIStyle(GUI.skin.label);
        highlightStyle.fontStyle = FontStyle.Bold;
        highlightStyle.fontSize = _highlightFontSize;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300));

        GUILayout.Label("=== BOT COLLECTOR CONTROLS ===", headerStyle);

        if (_manualControlMode)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("🚀 РЕЖИМ: РУЧНОЕ УПРАВЛЕНИЕ", highlightStyle);
            GUILayout.Label("• Вы можете перемещать выделенного бота", GUI.skin.label);
            GUILayout.Label("• AI у выделенного бота отключен", GUI.skin.label);
        }
        else
        {
            GUI.color = Color.cyan;
            GUILayout.Label("🤖 РЕЖИМ: АВТОМАТИЧЕСКИЙ", highlightStyle);
            GUILayout.Label("• Боты работают автономно", GUI.skin.label);
            GUILayout.Label("• Выделение для информации", GUI.skin.label);
        }

        GUI.color = Color.white;
        GUILayout.Space(10);

        GUILayout.Label("Основные управления:", headerStyle);
        GUILayout.Label("LMB: Выбрать бота", normalStyle);
        GUILayout.Label("RMB: Переместить выделенного бота", normalStyle);
        GUILayout.Label("B: Создать нового бота", normalStyle);
        GUILayout.Label($"R: Сбросить камеру", normalStyle);
        GUILayout.Label("ESC: Снять выделение", normalStyle);
        GUILayout.Label($"TAB: Переключить режим (сейчас: {(_manualControlMode ? "РУЧНОЙ" : "АВТО")})", normalStyle);

        if (_selectedBot != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Выделенный бот:", headerStyle);
            GUILayout.Label($"Имя: {_selectedBot.gameObject.name}");
            GUILayout.Label($"AI: {(_selectedBot.EnableAI ? "ВКЛ" : "ВЫКЛ")}");
            GUILayout.Label($"Управление: {(_manualControlMode ? "РУЧНОЕ" : "АВТО")}");
            GUILayout.Label($"Состояние: {_selectedBot.CurrentState}");
            GUILayout.Label($"Инвентарь: {_selectedBot.BotInventory.CurrentCount}/{_selectedBot.BotInventory.MaxCapacity}");
        }

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 320, 400, 250));
        GUILayout.Label("=== AI CONTROLS ===", headerStyle);
        GUILayout.Label("F1: Вкл/Выкл AI для всех ботов");
        GUILayout.Label("F2: Показать статус ботов");
        GUILayout.Label("F3: Сбросить всех ботов");
        GUILayout.Label("F8: Тест полного цикла");

        BotController[] bots = FindObjectsOfType<BotController>();
        if (bots.Length > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Статистика ботов:", headerStyle);

            int activeBots = 0;
            int manualBots = 0;
            int collectingBots = 0;
            int movingBots = 0;

            foreach (var bot in bots)
            {
                if (bot.EnableAI) activeBots++;
                if (bot == _selectedBot && _manualControlMode) manualBots++;
                if (bot.CurrentState == BotState.Collect || bot.CurrentState == BotState.MoveToItem) collectingBots++;
                if (bot.CurrentState == BotState.MoveToItem || bot.CurrentState == BotState.MoveToDeposit) movingBots++;
            }

            GUILayout.Label($"Всего ботов: {bots.Length}");
            GUILayout.Label($"AI активен: {activeBots}");
            GUILayout.Label($"В ручном режиме: {manualBots}");
            GUILayout.Label($"Собирают: {collectingBots}");
            GUILayout.Label($"В движении: {movingBots}");

            if (_selectedBot != null)
            {
                GUILayout.Space(5);
                GUILayout.Label("Состояние выделенного:", headerStyle);
                GUILayout.Label($"• {_selectedBot.CurrentState}");
            }
        }

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 80));

        GUIStyle modeStyle = new GUIStyle(GUI.skin.box);
        modeStyle.alignment = TextAnchor.MiddleCenter;
        modeStyle.fontSize = _modePanelFontSize;
        modeStyle.fontStyle = FontStyle.Bold;

        if (_manualControlMode)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("🚀 РУЧНОЙ РЕЖИМ", modeStyle);
            GUI.color = Color.white;
            GUILayout.Label("Управление одним ботом", GUI.skin.label);
        }
        else
        {
            GUI.color = Color.cyan;
            GUILayout.Label("🤖 АВТО РЕЖИМ", modeStyle);
            GUI.color = Color.white;
            GUILayout.Label("Автономная работа", GUI.skin.label);
        }

        GUILayout.EndArea();

        GUI.color = originalColor;
    }
}