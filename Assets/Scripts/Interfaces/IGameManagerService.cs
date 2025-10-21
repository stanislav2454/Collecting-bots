using System;

public interface IGameManagerService
{
    // Управление состоянием игры
    bool IsGameRunning { get; }
    void StartGame();
    void PauseGame();
    void ResumeGame();
    void ResetGame();

    // Настройки
    GameSettings GetGameSettings();
    void UpdateGameSettings(GameSettings settings);

    // Статистика игры
    int GetTotalBotsSpawned();
    int GetTotalItemsCollected();
    int GetTotalPointsEarned();
    float GetGameTime();

    // События
    event Action OnGameStarted;
    event Action OnGamePaused;
    event Action OnGameResumed;
    event Action OnGameReset;
    event Action<int> OnPointsEarned; // points
}