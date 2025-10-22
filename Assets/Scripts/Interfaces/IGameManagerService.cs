//using System;

//public interface IGameManagerService// зачем нужен ? если не используется - удалить !
//{//после рефакторинга УДАЛИТЬ!
//    public event Action GameStarted;
//    public event Action GamePaused;
//    public event Action GameResumed;
//    public event Action GameReset;
//    public event Action<int> PointsEarned;

//    public bool IsGameRunning { get; }
//    public void StartGame();
//    public void PauseGame();
//    public void ResumeGame();
//    public void ResetGame();
//    public GameSettings GetGameSettings();
//    public void UpdateGameSettings(GameSettings settings);
//    public int GetTotalBotsSpawned();
//    public int GetTotalItemsCollected();
//    public int GetTotalPointsEarned();
//    public float GetGameTime();
//}