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
    }

    public void Restart()
    {
        if (upgradeManager != null) upgradeManager.ResetRun();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.path);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
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
