using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public GameSettings gameSettings;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeGame();
    }

    private void InitializeGame()
    {
        if (gameSettings == null)
            Debug.LogError("GameSettings not assigned in GameManager!");
    }
}