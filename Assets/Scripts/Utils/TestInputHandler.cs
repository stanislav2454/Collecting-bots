using UnityEngine;

public class TestInputHandler : MonoBehaviour
{
    private const int MouseButtonRight = 1;
    private const int MouseButtonLeft = 0;

    [Header("InputKeys")]
    [SerializeField] private KeyCode _botSpawn = KeyCode.B;
    [SerializeField] private KeyCode _botDeselection = KeyCode.Escape;

    [Header("Testing")]
    public LayerMask groundLayer = 1;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public LayerMask botLayer = 1 << 6;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    //.layer = LayerMask.NameToLayer("Items"); ОБРАЗЕЦ для переделки полей с слоями LayerMask
    [Header("Selection Visual")]
    public Material selectedMaterial;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    public Material defaultMaterial;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !

    private BotController selectedBot;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    private Renderer selectedBotRenderer;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !
    private Material originalBotMaterial;//  - нарушение код конвенции(публик с большой, приват с нижней черты и маленькой)и инкапсуляции !

    private void Update()
    {
        HandleBotSelection();
        HandleBotMovement();
        HandleBotDeselection();
    }

    private void HandleBotSelection()
    {
        if (Input.GetMouseButtonDown(MouseButtonLeft))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, botLayer))
            {
                BotController bot = hit.collider.GetComponent<BotController>();
                if (bot != null)
                {
                    SelectBot(bot);
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                DeselectBot();
        }
    }

    private void HandleBotMovement()
    {
        if (Input.GetMouseButtonDown(MouseButtonRight) && selectedBot != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                selectedBot.MoveToPosition(hit.point);
        }
    }


    private void HandleBotDeselection()
    {
        if (Input.GetKeyDown(_botDeselection))
            DeselectBot();
    }

    private void SelectBot(BotController bot)
    {
        DeselectBot();

        selectedBot = bot;
        selectedBotRenderer = bot.GetComponent<Renderer>();

        if (selectedBotRenderer != null && selectedMaterial != null)
        {
            originalBotMaterial = selectedBotRenderer.material;
            selectedBotRenderer.material = selectedMaterial;
        }
    }

    private void DeselectBot()
    {
        if (selectedBot != null && selectedBotRenderer != null && originalBotMaterial != null)
            selectedBotRenderer.material = originalBotMaterial;

        selectedBot = null;
        selectedBotRenderer = null;
        originalBotMaterial = null;
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