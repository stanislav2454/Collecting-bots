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
    [SerializeField] private KeyCode _botStatusKey = KeyCode.F2;
    [SerializeField] private KeyCode _resetAllBotsKey = KeyCode.F3;
    [SerializeField] private KeyCode _cycleTestKey = KeyCode.F8;

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

    // Ссылки на сервисы
    private IBotService _botService;
    private IItemService _itemService;
    private IDepositService _depositService;

    private void Start()
    {
        // Инициализируем LayerMask
        _groundLayer = 1 << LayerMask.NameToLayer(_groundLayerName);
        _botLayer = 1 << LayerMask.NameToLayer(_botLayerName);

        // Проверяем слои
        if (LayerMask.NameToLayer(_groundLayerName) == -1)
            Debug.LogError($"Layer '{_groundLayerName}' not found! Please create this layer in Project Settings.");
        if (LayerMask.NameToLayer(_botLayerName) == -1)
            Debug.LogError($"Layer '{_botLayerName}' not found! Please create this layer in Project Settings.");

        // Получаем сервисы
        InitializeServices();

        Debug.Log("BotInputHandler initialized with services");
    }

    private void InitializeServices()
    {
        // Пытаемся получить сервисы безопасно
        if (ServiceLocator.TryGet<IBotService>(out _botService))
        {
            Debug.Log("BotService connected successfully");

            // Подписываемся на события BotService
            _botService.OnBotSelected += OnBotSelected;
            _botService.OnBotDeselected += OnBotDeselected;
        }
        else
        {
            Debug.LogWarning("BotService not available - using fallback mode");
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

    #region Control Mode
    private void HandleControlModeToggle()
    {
        if (Input.GetKeyDown(_toggleControlModeKey))
        {
            _manualControlMode = !_manualControlMode;
            Debug.Log($"Control mode: {(_manualControlMode ? "MANUAL" : "AUTO")}");

            // При переключении режима обновляем визуал выделенного бота
            if (_selectedBot != null)
            {
                UpdateSelectedBotVisual();
            }
        }
    }

    private void UpdateSelectedBotVisual()
    {
        Debug.Log($"=== UPDATE VISUAL START ===");
        Debug.Log($"Selected Bot: {_selectedBot?.gameObject.name}");
        Debug.Log($"Selected Bot Material: {_selectedBotMaterial}");
        Debug.Log($"Manual Control Material: {_manualControlMaterial}");

        if (_selectedBot == null)
        {
            Debug.LogWarning("No selected bot!");
            return;
        }

        Renderer botRenderer = _selectedBot.GetComponent<Renderer>();
        Debug.Log($"Renderer: {botRenderer}");

        if (botRenderer != null)
        {
            if (_manualControlMode)
            {
                // РУЧНОЙ РЕЖИМ: используем manual control material
                Debug.Log($"Applying MANUAL CONTROL material");
                if (!_originalBotMaterials.ContainsKey(_selectedBot))
                {
                    _originalBotMaterials[_selectedBot] = botRenderer.material;
                    Debug.Log($"Saved original material: {botRenderer.material.name}");
                }

                if (_manualControlMaterial != null)
                {
                    botRenderer.material = _manualControlMaterial;
                    Debug.Log($"New material: {botRenderer.material.name}");
                }
                else
                {
                    Debug.LogError("Manual Control Material is not assigned!");
                }
            }
            else
            {
                // АВТО РЕЖИМ: используем selected bot material
                Debug.Log($"Applying SELECTED BOT material");
                if (!_originalBotMaterials.ContainsKey(_selectedBot))
                {
                    _originalBotMaterials[_selectedBot] = botRenderer.material;
                    Debug.Log($"Saved original material: {botRenderer.material.name}");
                }

                if (_selectedBotMaterial != null)
                {
                    botRenderer.material = _selectedBotMaterial;
                    Debug.Log($"New material: {botRenderer.material.name}");
                }
                else
                {
                    Debug.LogError("Selected Bot Material is not assigned!");
                }
            }
        }
        else
        {
            Debug.LogError($"Missing Renderer! Renderer: {botRenderer}");
        }
        Debug.Log($"=== UPDATE VISUAL END ===");
    }
    #endregion

    #region Basic Bot Controls
    private void HandleBotSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"=== MOUSE CLICK START ===");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.Log($"Bot Layer: {_botLayer.value} (name: {_botLayerName})");

            // Используем правильный LayerMask для ботов
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _botLayer))
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name}");
                Debug.Log($"Hit layer: {hit.collider.gameObject.layer} ({LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                BotController bot = hit.collider.GetComponent<BotController>();
                Debug.Log($"Bot Controller: {bot}");

                if (bot != null)
                {
                    SelectBot(bot);
                    return;
                }
            }
            else
            {
                Debug.Log($"No bot hit with layer mask: {_botLayer.value}");
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
            {
                Debug.Log($"Ground hit - deselecting bots");
                DeselectAllBots();
            }
            Debug.Log($"=== MOUSE CLICK END ===");
        }
    }

    private void HandleBotMovement()
    {
        if (Input.GetMouseButtonDown(1) && _selectedBot != null)
        {
            // В ручном режиме принудительно выключаем AI у выделенного бота
            if (_manualControlMode)
            {
                _selectedBot.SetAIEnabled(false);
            }

            // Двигаем только если AI выключен (вручную или через режим)
            if (!_selectedBot.EnableAI)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
                {
                    _selectedBot.MoveToPosition(hit.point);
                    Debug.Log($"Manual move: {_selectedBot.gameObject.name} to {hit.point}");
                }
            }
            else
            {
                Debug.Log($"Cannot move - {_selectedBot.gameObject.name} AI is enabled");
            }
        }
    }

    private void HandleBotSpawning()
    {
        if (Input.GetKeyDown(_botSpawnKey))
        {
            SpawnBotAtRandomPosition();
        }
    }

    private void HandleBotDeselection()
    {
        if (Input.GetKeyDown(_botDeselectionKey))
            DeselectAllBots();
    }

    private void HandleCameraReset()
    {
        if (Input.GetKeyDown(_cameraResetKey))
        {
            ResetCamera();
        }
    }
    #endregion

    #region AI Controls
    private void HandleAITesting()
    {
        if (Input.GetKeyDown(_toggleAIKey))
            ToggleAI();

        if (Input.GetKeyDown(_botStatusKey))
            ShowBotStatus();

        if (Input.GetKeyDown(_resetAllBotsKey))
            ResetAllBots();

        if (Input.GetKeyDown(_cycleTestKey))
            TestFullCycle();
    }

    private void ToggleAI()
    {
        // Используем BotService если доступен
        if (_botService != null)
        {
            BotController[] bots = _botService.GetAllBots();
            bool anyAIEnabled = false;

            foreach (var bot in bots)
            {
                if (_manualControlMode && bot == _selectedBot)
                {
                    // В ручном режиме выделенный бот управляется вручную
                    continue;
                }

                bot.SetAIEnabled(!bot.EnableAI);
                UpdateBotVisual(bot);

                if (bot.EnableAI) anyAIEnabled = true;
            }

            Debug.Log($"All bots AI: {(anyAIEnabled ? "ENABLED" : "DISABLED")}");
        }
        else
        {
            // Fallback на старую систему
            ToggleAIFallback();
        }
    }

    private void ToggleAIFallback()
    {
        BotController[] bots = FindObjectsOfType<BotController>();
        bool anyAIEnabled = false;

        foreach (var bot in bots)
        {
            if (_manualControlMode && bot == _selectedBot)
            {
                continue;
            }

            bot.SetAIEnabled(!bot.EnableAI);
            UpdateBotVisual(bot);

            if (bot.EnableAI) anyAIEnabled = true;
        }

        Debug.Log($"All bots AI (fallback): {(anyAIEnabled ? "ENABLED" : "DISABLED")}");
    }

    private void ShowBotStatus()
    {
        BotController[] bots = _botService?.GetAllBots() ?? FindObjectsOfType<BotController>();
        Debug.Log("=== BOT STATUS ===");

        foreach (var bot in bots)
            Debug.Log(GetEnhancedBotInfo(bot));
    }

    private void ResetAllBots()
    {
        DeselectAllBots();
        _originalBotMaterials.Clear();

        // Используем BotService если доступен
        if (_botService != null)
        {
            _botService.ResetAllBots();
        }
        else
        {
            // Fallback на старую систему
            BotController[] bots = FindObjectsOfType<BotController>();
            foreach (var bot in bots)
            {
                bot.SetAIEnabled(true);
                UpdateBotVisual(bot);
                bot.StopMovement();
            }
            Debug.Log("All bots reset to default state (fallback)");
        }
    }

    private void TestFullCycle()
    {
        BotController[] bots = _botService?.GetAllBots() ?? FindObjectsOfType<BotController>();
        Debug.Log($"=== FULL CYCLE TEST ({bots.Length} bots) ===");

        int botsWithItems = 0;
        int botsInCycle = 0;

        foreach (var bot in bots)
        {
            if (bot.BotInventory.CurrentCount > 0) botsWithItems++;
            if (bot.EnableAI && bot.CurrentState != BotState.Idle) botsInCycle++;

            Debug.Log(GetBotCycleInfo(bot));
        }

        Debug.Log($"Bots in cycle: {botsInCycle}, Bots with items: {botsWithItems}");
    }
    #endregion

    #region Bot Selection Visuals
    private void SelectBot(BotController bot)
    {
        Debug.Log($"=== SELECT BOT START ===");
        Debug.Log($"Bot: {bot?.gameObject.name}");
        Debug.Log($"Bot Renderer: {bot?.GetComponent<Renderer>()}");
        Debug.Log($"Manual Control Mode: {_manualControlMode}");

        // Используем BotService если доступен
        if (_botService != null)
        {
            _botService.SelectBot(bot);
        }
        else
        {
            // Fallback на прямую работу
            DeselectAllBots();
            _selectedBot = bot;

            // В ручном режиме выключаем AI у выделенного бота
            if (_manualControlMode)
            {
                Debug.Log($"Setting AI disabled for manual control");
                _selectedBot.SetAIEnabled(false);
            }

            UpdateSelectedBotVisual();
            Debug.Log($"Selected bot: {bot.gameObject.name} | Control: {(_manualControlMode ? "MANUAL" : "AUTO")}");
        }
    }

    private void DeselectAllBots()
    {
        // Используем BotService если доступен
        if (_botService != null)
        {
            _botService.DeselectAllBots();
        }
        else
        {
            // Fallback на прямую работу
            if (_selectedBot != null)
            {
                // При снятии выделения возвращаем стандартное управление
                if (_manualControlMode)
                {
                    _selectedBot.SetAIEnabled(true);
                }

                ResetBotVisual(_selectedBot);
                _selectedBot = null;
            }
        }
    }

    // Обработчики событий от BotService
    private void OnBotSelected(BotController bot)
    {
        _selectedBot = bot;

        // В ручном режиме выключаем AI у выделенного бота
        if (_manualControlMode)
        {
            _selectedBot.SetAIEnabled(false);
        }

        UpdateSelectedBotVisual();
        Debug.Log($"Bot selected via service: {bot.gameObject.name}");
    }

    private void OnBotDeselected(BotController bot)
    {
        if (_selectedBot == bot)
        {
            // При снятии выделения возвращаем стандартное управление
            if (_manualControlMode)
            {
                _selectedBot.SetAIEnabled(true);
            }

            ResetBotVisual(_selectedBot);
            _selectedBot = null;
        }
        Debug.Log($"Bot deselected via service: {bot.gameObject.name}");
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
        if (bot == _selectedBot && _manualControlMode) return;

        Renderer botRenderer = bot.GetComponent<Renderer>();
        if (botRenderer != null)
        {
            if (_aiDisabledMaterial != null && _aiEnabledMaterial != null)
            {
                botRenderer.material = bot.EnableAI ? _aiEnabledMaterial : _aiDisabledMaterial;
            }
            else
            {
                botRenderer.material.color = bot.EnableAI ? Color.blue : Color.gray;
            }
        }

        BotVisualIndicator indicator = bot.GetComponent<BotVisualIndicator>();
        if (indicator == null)
        {
            indicator = bot.gameObject.AddComponent<BotVisualIndicator>();
        }
        indicator.UpdateAIStatus(bot.EnableAI, bot.CurrentState);
    }
    #endregion

    #region Utility Methods
    private void SpawnBotAtRandomPosition()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Используем BotService если доступен
        if (_botService != null)
        {
            _botService.SpawnBot(spawnPosition);
        }
        else
        {
            // Fallback на старую систему
            BotManager botManager = FindObjectOfType<BotManager>();
            if (botManager != null)
            {
                GameObject newBot = botManager.SpawnBot();
                if (newBot != null)
                    AddColliderToBot(newBot);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Простая реализация случайной позиции
        return new Vector3(
            Random.Range(-5f, 5f),
            0,
            Random.Range(-5f, 5f)
        );
    }

    private void AddColliderToBot(GameObject bot)
    {
        if (bot.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
            bot.layer = LayerMask.NameToLayer(_botLayerName);
        }
    }

    private void ResetCamera()
    {
        Camera.main.transform.position = new Vector3(0, 15, -10);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        Debug.Log("Camera reset to default position");
    }

    private string GetBotCycleInfo(BotController bot)
    {
        string inventoryStatus = bot.BotInventory.IsFull ? "FULL" : $"{bot.BotInventory.CurrentCount}/{bot.BotInventory.MaxCapacity}";
        return $"Bot: {bot.gameObject.name} | State: {bot.CurrentState} | Inventory: {inventoryStatus}";
    }

    private string GetEnhancedBotInfo(BotController bot)
    {
        string aiStatus = bot.EnableAI ? "ON" : "OFF";
        string controlStatus = (bot == _selectedBot && _manualControlMode) ? "MANUAL" : "AUTO";
        return $"Bot: {bot.gameObject.name} | AI: {aiStatus} | Control: {controlStatus} | State: {bot.CurrentState} | Inventory: {bot.BotInventory.CurrentCount}/{bot.BotInventory.MaxCapacity}";
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Отписываемся от событий
        if (_botService != null)
        {
            _botService.OnBotSelected -= OnBotSelected;
            _botService.OnBotDeselected -= OnBotDeselected;
        }
    }
    #endregion
    //private void Start()
    //{
    //    // Правильно инициализируем LayerMask из имен слоев
    //    _groundLayer = 1 << LayerMask.NameToLayer(_groundLayerName);
    //    _botLayer = 1 << LayerMask.NameToLayer(_botLayerName);

    //    // Проверяем, что слои существуют
    //    if (LayerMask.NameToLayer(_groundLayerName) == -1)
    //        Debug.LogError($"Layer '{_groundLayerName}' not found! Please create this layer in Project Settings.");
    //    if (LayerMask.NameToLayer(_botLayerName) == -1)
    //        Debug.LogError($"Layer '{_botLayerName}' not found! Please create this layer in Project Settings.");
    //}

    //private void Update()
    //{
    //    HandleControlModeToggle();
    //    HandleBotSelection();
    //    HandleBotMovement();
    //    HandleBotSpawning();
    //    HandleBotDeselection();
    //    HandleAITesting();
    //    HandleCameraReset(); // Теперь используется
    //}

    //#region Control Mode
    //private void HandleControlModeToggle()
    //{
    //    if (Input.GetKeyDown(_toggleControlModeKey))
    //    {
    //        _manualControlMode = !_manualControlMode;
    //        Debug.Log($"Control mode: {(_manualControlMode ? "MANUAL" : "AUTO")}");

    //        // При переключении режима обновляем визуал выделенного бота
    //        if (_selectedBot != null)
    //        {
    //            UpdateSelectedBotVisual();
    //        }
    //    }
    //}

    //private void UpdateSelectedBotVisual()
    //{
    //    Debug.Log($"=== UPDATE VISUAL START ===");
    //    Debug.Log($"Selected Bot: {_selectedBot?.gameObject.name}");
    //    Debug.Log($"Selected Bot Material: {_selectedBotMaterial}");
    //    Debug.Log($"Manual Control Material: {_manualControlMaterial}");

    //    if (_selectedBot == null)
    //    {
    //        Debug.LogWarning("No selected bot!");
    //        return;
    //    }

    //    Renderer botRenderer = _selectedBot.GetComponent<Renderer>();
    //    Debug.Log($"Renderer: {botRenderer}");

    //    if (botRenderer != null)
    //    {
    //        if (_manualControlMode)
    //        {
    //            // РУЧНОЙ РЕЖИМ: используем manual control material
    //            Debug.Log($"Applying MANUAL CONTROL material");
    //            if (!_originalBotMaterials.ContainsKey(_selectedBot))
    //            {
    //                _originalBotMaterials[_selectedBot] = botRenderer.material;
    //                Debug.Log($"Saved original material: {botRenderer.material.name}");
    //            }

    //            if (_manualControlMaterial != null)
    //            {
    //                botRenderer.material = _manualControlMaterial;
    //                Debug.Log($"New material: {botRenderer.material.name}");
    //            }
    //            else
    //            {
    //                Debug.LogError("Manual Control Material is not assigned!");
    //            }
    //        }
    //        else
    //        {
    //            // АВТО РЕЖИМ: используем selected bot material
    //            Debug.Log($"Applying SELECTED BOT material");
    //            if (!_originalBotMaterials.ContainsKey(_selectedBot))
    //            {
    //                _originalBotMaterials[_selectedBot] = botRenderer.material;
    //                Debug.Log($"Saved original material: {botRenderer.material.name}");
    //            }

    //            if (_selectedBotMaterial != null)
    //            {
    //                botRenderer.material = _selectedBotMaterial;
    //                Debug.Log($"New material: {botRenderer.material.name}");
    //            }
    //            else
    //            {
    //                Debug.LogError("Selected Bot Material is not assigned!");
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError($"Missing Renderer! Renderer: {botRenderer}");
    //    }
    //    Debug.Log($"=== UPDATE VISUAL END ===");
    //}
    //#endregion

    //#region Basic Bot Controls
    //private void HandleBotSelection()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Debug.Log($"=== MOUSE CLICK START ===");
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;
    //        Debug.Log($"Bot Layer: {_botLayer.value} (name: {_botLayerName})");

    //        // Используем правильный LayerMask для ботов
    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _botLayer))
    //        {
    //            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
    //            Debug.Log($"Hit layer: {hit.collider.gameObject.layer} ({LayerMask.LayerToName(hit.collider.gameObject.layer)})");

    //            BotController bot = hit.collider.GetComponent<BotController>();
    //            Debug.Log($"Bot Controller: {bot}");

    //            if (bot != null)
    //            {
    //                SelectBot(bot);
    //                return;
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log($"No bot hit with layer mask: {_botLayer.value}");
    //        }

    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
    //        {
    //            Debug.Log($"Ground hit - deselecting bots");
    //            DeselectAllBots();
    //        }
    //        Debug.Log($"=== MOUSE CLICK END ===");
    //    }
    //}

    //private void HandleBotMovement()
    //{
    //    if (Input.GetMouseButtonDown(1) && _selectedBot != null)
    //    {
    //        // В ручном режиме принудительно выключаем AI для выделенного бота
    //        if (_manualControlMode)
    //        {
    //            _selectedBot.SetAIEnabled(false);
    //        }

    //        // Двигаем только если AI выключен (вручную или через режим)
    //        if (!_selectedBot.EnableAI)
    //        {
    //            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //            RaycastHit hit;

    //            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _groundLayer))
    //            {
    //                _selectedBot.MoveToPosition(hit.point);
    //                Debug.Log($"Manual move: {_selectedBot.gameObject.name} to {hit.point}");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log($"Cannot move - {_selectedBot.gameObject.name} AI is enabled");
    //        }
    //    }
    //}

    //private void HandleBotSpawning()
    //{
    //    if (Input.GetKeyDown(_botSpawnKey))
    //    {
    //        BotManager botManager = FindObjectOfType<BotManager>();
    //        if (botManager != null)
    //        {
    //            GameObject newBot = botManager.SpawnBot();
    //            if (newBot != null)
    //                AddColliderToBot(newBot);
    //        }
    //    }
    //}

    //private void HandleBotDeselection()
    //{
    //    if (Input.GetKeyDown(_botDeselectionKey))
    //        DeselectAllBots();
    //}
    //#endregion

    //#region AI Controls
    //private void HandleAITesting()
    //{
    //    if (Input.GetKeyDown(_toggleAIKey))
    //        ToggleAI();

    //    if (Input.GetKeyDown(_botStatusKey))
    //        ShowBotStatus();

    //    if (Input.GetKeyDown(_resetAllBotsKey))
    //        ResetAllBots();

    //    if (Input.GetKeyDown(_cycleTestKey))
    //        TestFullCycle();
    //}

    //private void ToggleAI()
    //{
    //    // В ручном режиме переключаем AI только у НЕвыделенных ботов
    //    BotController[] bots = FindObjectsOfType<BotController>();
    //    bool anyAIEnabled = false;

    //    foreach (var bot in bots)
    //    {
    //        if (_manualControlMode && bot == _selectedBot)
    //        {
    //            // В ручном режиме выделенный бот управляется вручную
    //            continue;
    //        }

    //        bot.SetAIEnabled(!bot.EnableAI);
    //        UpdateBotVisual(bot);

    //        if (bot.EnableAI) anyAIEnabled = true;
    //    }

    //    Debug.Log($"All bots AI: {(anyAIEnabled ? "ENABLED" : "DISABLED")}");
    //}

    //private void ShowBotStatus()
    //{
    //    BotController[] bots = FindObjectsOfType<BotController>();
    //    Debug.Log("=== BOT STATUS ===");

    //    foreach (var bot in bots)
    //        Debug.Log(GetEnhancedBotInfo(bot));
    //}

    //private void ResetAllBots()
    //{
    //    DeselectAllBots();
    //    _originalBotMaterials.Clear();

    //    BotController[] bots = FindObjectsOfType<BotController>();
    //    foreach (var bot in bots)
    //    {
    //        bot.SetAIEnabled(true);
    //        UpdateBotVisual(bot);
    //        bot.StopMovement();
    //    }
    //    Debug.Log("All bots reset to default state");
    //}

    //private void TestFullCycle()
    //{
    //    BotController[] bots = FindObjectsOfType<BotController>();
    //    Debug.Log($"=== FULL CYCLE TEST ({bots.Length} bots) ===");

    //    int botsWithItems = 0;
    //    int botsInCycle = 0;

    //    foreach (var bot in bots)
    //    {
    //        if (bot.BotInventory.CurrentCount > 0) botsWithItems++;
    //        if (bot.EnableAI && bot.CurrentState != BotState.Idle) botsInCycle++;

    //        Debug.Log(GetBotCycleInfo(bot));
    //    }

    //    Debug.Log($"Bots in cycle: {botsInCycle}, Bots with items: {botsWithItems}");
    //}
    //#endregion

    //#region Bot Selection Visuals
    //private void SelectBot(BotController bot)
    //{
    //    Debug.Log($"=== SELECT BOT START ===");
    //    Debug.Log($"Bot: {bot?.gameObject.name}");
    //    Debug.Log($"Bot Renderer: {bot?.GetComponent<Renderer>()}");
    //    Debug.Log($"Manual Control Mode: {_manualControlMode}");

    //    DeselectAllBots();
    //    _selectedBot = bot;

    //    // В ручном режиме выключаем AI у выделенного бота
    //    if (_manualControlMode)
    //    {
    //        Debug.Log($"Setting AI disabled for manual control");
    //        _selectedBot.SetAIEnabled(false);
    //    }

    //    UpdateSelectedBotVisual();
    //    Debug.Log($"Selected bot: {bot.gameObject.name} | Control: {(_manualControlMode ? "MANUAL" : "AUTO")}");
    //}

    //private void DeselectAllBots()
    //{
    //    if (_selectedBot != null)
    //    {
    //        // При снятии выделения возвращаем стандартное управление
    //        if (_manualControlMode)
    //        {
    //            _selectedBot.SetAIEnabled(true);
    //        }

    //        ResetBotVisual(_selectedBot);
    //        _selectedBot = null;
    //    }
    //}

    //private void ResetBotVisual(BotController bot)
    //{
    //    if (bot != null)
    //    {
    //        Renderer botRenderer = bot.GetComponent<Renderer>();
    //        if (botRenderer != null && _originalBotMaterials.ContainsKey(bot))
    //        {
    //            Debug.Log($"Resetting visual for bot: {bot.gameObject.name}");
    //            botRenderer.material = _originalBotMaterials[bot];
    //            _originalBotMaterials.Remove(bot);
    //        }
    //        else if (botRenderer != null)
    //        {
    //            UpdateBotVisual(bot);
    //        }
    //    }
    //}

    //private void UpdateBotVisual(BotController bot)
    //{
    //    if (bot == _selectedBot && _manualControlMode) return;

    //    Renderer botRenderer = bot.GetComponent<Renderer>();
    //    if (botRenderer != null)
    //    {
    //        if (_aiDisabledMaterial != null && _aiEnabledMaterial != null)
    //        {
    //            botRenderer.material = bot.EnableAI ? _aiEnabledMaterial : _aiDisabledMaterial;
    //        }
    //        else
    //        {
    //            botRenderer.material.color = bot.EnableAI ? Color.blue : Color.gray;
    //        }
    //    }

    //    BotVisualIndicator indicator = bot.GetComponent<BotVisualIndicator>();
    //    if (indicator == null)
    //    {
    //        indicator = bot.gameObject.AddComponent<BotVisualIndicator>();
    //    }
    //    indicator.UpdateAIStatus(bot.EnableAI, bot.CurrentState);
    //}
    //#endregion

    //#region Utility Methods
    //private void AddColliderToBot(GameObject bot)
    //{
    //    if (bot.GetComponent<Collider>() == null)
    //    {
    //        CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
    //        collider.height = 2f;
    //        collider.radius = 0.5f;
    //        collider.center = new Vector3(0, 1f, 0);
    //        bot.layer = LayerMask.NameToLayer(_botLayerName);
    //    }
    //}

    //private string GetBotCycleInfo(BotController bot)
    //{
    //    string inventoryStatus = bot.BotInventory.IsFull ?
    //        "FULL" : $"{bot.BotInventory.CurrentCount}/{bot.BotInventory.MaxCapacity}";
    //    return $"Bot: {bot.gameObject.name} | State: {bot.CurrentState} | Inventory: {inventoryStatus}";
    //}

    //private string GetEnhancedBotInfo(BotController bot)
    //{
    //    string aiStatus = bot.EnableAI ? "ON" : "OFF";
    //    string controlStatus = (bot == _selectedBot && _manualControlMode) ? "MANUAL" : "AUTO";
    //    return $"Bot: {bot.gameObject.name} | AI: {aiStatus} | Control: {controlStatus}" +
    //        $" | State: {bot.CurrentState} | Inventory: {bot.BotInventory.CurrentCount}/{bot.BotInventory.MaxCapacity}";
    //}
    //#endregion

    //#region Camera Controls
    //private void HandleCameraReset()
    //{
    //    if (Input.GetKeyDown(_cameraResetKey))
    //    {
    //        ResetCamera();
    //    }
    //}

    //private void ResetCamera()
    //{
    //    // Сброс камеры в начальную позицию
    //    Camera.main.transform.position = new Vector3(0, 15, -10);
    //    Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
    //    Debug.Log("Camera reset to default position");
    //}
    //#endregion

    private void OnGUI()
    {
        Color originalColor = GUI.color;

        // Стиль для заголовков
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = _headerFontSize;

        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = _normalFontSize;

        // Стиль для важной информации
        GUIStyle highlightStyle = new GUIStyle(GUI.skin.label);
        highlightStyle.fontStyle = FontStyle.Bold;
        highlightStyle.fontSize = _highlightFontSize;

        // Basic Controls GUI
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));

        // ОТОБРАЖЕНИЕ РЕЖИМА УПРАВЛЕНИЯ С ЦВЕТОМ
        GUILayout.Label("=== BOT COLLECTOR CONTROLS ===", headerStyle);

        // Яркое отображение режима управления
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
        GUILayout.Label($"R: Сбросить камеру", normalStyle); // Теперь используется
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

        // AI Controls GUI
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

        // Дополнительная панель - текущий режим в углу экрана
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