using System;
using UnityEngine;

public enum RunState { Booting, Running, GameOver, Victory }

[DefaultExecutionOrder(700)]
[DisallowMultipleComponent]
public sealed class RunStateManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Health playerHealth;

    public RunState State { get; private set; } = RunState.Booting;
    public int CurrentRoomIndex => roomManager == null ? -1 : roomManager.CurrentIndex;
    public bool HasActiveRun => State == RunState.Running;
    public event Action<RunState> StateChanged;
    private bool subscribed;

    private void Awake()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (roomManager == null) roomManager = FindAnyObjectByType<RoomManager>();
        if (upgradeManager == null) upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (playerHealth == null)
        {
            var player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) playerHealth = player.GetComponent<Health>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        if (subscribed) return;
        if (gameManager != null)
        {
            gameManager.GameOverTriggered += HandleGameOver;
            gameManager.VictoryTriggered += HandleVictory;
            gameManager.RunReset += HandleRunReset;
        }
        if (roomManager != null) roomManager.ProgressChanged += HandleRoomProgress;
        if (playerHealth != null) playerHealth.Changed += HandlePlayerHealth;
        subscribed = true;
    }

    private void Start()
    {
        Subscribe();
        SetState(RunState.Running);
    }

    private void HandleRoomProgress()
    {
        if (State == RunState.Booting) SetState(RunState.Running);
    }

    private void HandlePlayerHealth(Health health)
    {
        if (health.IsDead && gameManager != null && gameManager.IsGameOver) HandleGameOver();
    }

    private void HandleGameOver() => SetState(RunState.GameOver);
    private void HandleVictory() => SetState(RunState.Victory);
    private void HandleRunReset() => SetState(RunState.Running);

    public void BeginNewRun()
    {
        if (gameManager != null) gameManager.BeginNewRunState();
        else if (upgradeManager != null) upgradeManager.ResetRun();
        SetState(RunState.Running);
    }

    private void SetState(RunState next)
    {
        if (State == next) return;
        State = next;
        StateChanged?.Invoke(next);
    }

    private void OnDisable()
    {
        if (!subscribed) return;
        if (gameManager != null)
        {
            gameManager.GameOverTriggered -= HandleGameOver;
            gameManager.VictoryTriggered -= HandleVictory;
            gameManager.RunReset -= HandleRunReset;
        }
        if (roomManager != null) roomManager.ProgressChanged -= HandleRoomProgress;
        if (playerHealth != null) playerHealth.Changed -= HandlePlayerHealth;
        subscribed = false;
    }
}
