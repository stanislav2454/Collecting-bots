using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public GameSettings gameSettings;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
       // Debug.Log("Game Manager Initialized");

        if (gameSettings == null)
            Debug.LogWarning("GameSettings not assigned in GameManager!");
    }

    //private void Update()
    //{  // Базовая логика выхода
    //    if (Input.GetKeyDown(KeyCode.Escape))// клавиша "Escape" - уже занята !
    //        Application.Quit();
    //}
}