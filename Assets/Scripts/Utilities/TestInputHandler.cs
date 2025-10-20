using UnityEngine;

public class TestInputHandler : MonoBehaviour
{
    private const int MouseButtonRight = 1;
    private const int MouseButtonLeft = 0;

    [Header("InputKeys")]
    [SerializeField] private KeyCode _botSpawn = KeyCode.B;
    [SerializeField] private KeyCode _botDeselection = KeyCode.Escape;

    [Header("Testing")]
    public LayerMask groundLayer = 1;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public LayerMask botLayer = 1 << 6;// модификаторДоступа+именаСчертойИлиБольшойБуквы

    [Header("Selection Visual")]
    public Material selectedMaterial;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    public Material defaultMaterial;// модификаторДоступа+именаСчертойИлиБольшойБуквы

    private BotController selectedBot;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    private Renderer selectedBotRenderer;// модификаторДоступа+именаСчертойИлиБольшойБуквы
    private Material originalBotMaterial;// модификаторДоступа+именаСчертойИлиБольшойБуквы

    private void Update()
    {
        HandleBotSelection();
        HandleBotMovement();
        HandleBotSpawning();
        HandleBotDeselection();
    }

    private void HandleBotSelection()
    {
        if (Input.GetMouseButtonDown(MouseButtonLeft)) // Левая кнопка мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Сначала проверяем попадание в бота
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, botLayer))
            {
                BotController bot = hit.collider.GetComponent<BotController>();
                if (bot != null)
                {
                    SelectBot(bot);
                    return; // Не проверяем землю если попали в бота
                }
            }

            // Если не попали в бота, проверяем землю для сброса выделения
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                DeselectBot();
            }
        }
    }

    private void HandleBotMovement()
    {
        if (Input.GetMouseButtonDown(MouseButtonRight) && selectedBot != null) // Правая кнопка мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                selectedBot.MoveToPosition(hit.point);
              //  Debug.Log($"Moving {selectedBot.gameObject.name} to: {hit.point}");
            }
        }
    }

    private void HandleBotSpawning()
    {
        if (Input.GetKeyDown(_botSpawn))
        {
            BotManager botManager = FindObjectOfType<BotManager>();// todo
            if (botManager != null)
            {
                GameObject newBot = botManager.SpawnBot();
                if (newBot != null)
                    AddColliderToBot(newBot);
            }
        }
    }

    private void HandleBotDeselection()
    {
        if (Input.GetKeyDown(_botDeselection))
            DeselectBot();
    }

    private void SelectBot(BotController bot)
    { // Сбрасываем предыдущее выделение
        DeselectBot();

        selectedBot = bot;
        selectedBotRenderer = bot.GetComponent<Renderer>();

        // Сохраняем оригинальный материал и применяем выделенный
        if (selectedBotRenderer != null && selectedMaterial != null)
        {
            originalBotMaterial = selectedBotRenderer.material;
            selectedBotRenderer.material = selectedMaterial;
        }

      //  Debug.Log($"Selected bot: {bot.gameObject.name}");
    }

    private void DeselectBot()
    {
        if (selectedBot != null && selectedBotRenderer != null && originalBotMaterial != null)
        {
            selectedBotRenderer.material = originalBotMaterial;
        }

        selectedBot = null;
        selectedBotRenderer = null;
        originalBotMaterial = null;

        // Debug.Log("Bot deselected");
    }

    // Метод для добавления коллайдера боту
    private void AddColliderToBot(GameObject bot)
    {
        if (bot.GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = bot.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);

            // Устанавливаем слой бота
            bot.layer = 6; // Bot layer
        }
    }

    private void OnGUI()
    {
        Color originalColor = GUI.color;
        GUI.color = Color.red;
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Label("=== BOT COLLECTOR TEST ===");
        GUILayout.Label("LMB: Select bot");
        GUILayout.Label("RMB: Move selected bot");
        GUILayout.Label("B: Spawn new bot");
        GUILayout.Label("R: Reset camera");
        GUILayout.Label("ESC: Quit");

        if (selectedBot != null)
            GUILayout.Label($"Selected: {selectedBot.gameObject.name}");

        GUILayout.EndArea();

        GUI.color = originalColor;
    }
}