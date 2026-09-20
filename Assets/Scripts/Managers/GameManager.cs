using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.path);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnDestroy()
    {
        if (IsGameOver) Time.timeScale = 1f;
    }
}
