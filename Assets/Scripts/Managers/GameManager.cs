using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private GameObject victoryPanel;
    public bool IsGameOver { get; private set; }
    public bool IsVictory { get; private set; }
    public event Action VictoryTriggered;
    public event Action GameOverTriggered;
    public event Action RunReset;

    private void Awake()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    // Connected to the player's Health death event in GameScene.
    public void GameOver()
    {
        if (IsGameOver || playerHealth == null || !playerHealth.IsDead) return;
        IsGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        GameOverTriggered?.Invoke();
    }

    public void Restart()
    {
        BeginNewRunState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.path);
    }

    public void BeginNewRunState()
    {
        if (upgradeManager != null) upgradeManager.ResetRun();
        IsGameOver = false;
        IsVictory = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        Time.timeScale = 1f;
        RunReset?.Invoke();
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ReplayRun()
    {
        BeginNewRunState();
        SceneManager.LoadScene(gameObject.scene.path);
    }

    public void Victory()
    {
        if (IsGameOver || IsVictory) return;
        IsVictory = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        VictoryTriggered?.Invoke();
    }

    private void OnDestroy()
    {
        if (IsGameOver || IsVictory) Time.timeScale = 1f;
    }
}
