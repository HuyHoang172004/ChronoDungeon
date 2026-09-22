using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PauseManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private float previousTimeScale = 1f;
    public bool IsPaused { get; private set; }
    public event Action<bool> PauseChanged;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        if (IsPaused || (gameManager != null && (gameManager.IsGameOver || gameManager.IsVictory))) return;
        previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;
        PauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = previousTimeScale;
        PauseChanged?.Invoke(false);
    }

    public void RestartRun()
    {
        IsPaused = false;
        if (gameManager != null) gameManager.Restart();
    }

    public void MainMenu()
    {
        IsPaused = false;
        if (gameManager != null) gameManager.MainMenu();
    }

    private void OnDestroy() { if (IsPaused) Time.timeScale = 1f; }
}
