using System;
using UnityEngine;

public enum RoomState { NotEntered, Active, Completed, Exited }
public enum RoomClearCondition { OnEntry, DefeatEnemies, SolvePuzzle }
public enum RoomRole { Start, Combat, TimePuzzle, Trap, Treasure, CombatChallenge, Elite, Boss }

// Owns one encounter's clear condition; RoomManager owns progression.
[DefaultExecutionOrder(400)]
public sealed class Room : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private string displayName = "Room";
    [SerializeField] private RoomRole role;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private RoomClearCondition clearCondition;
    [SerializeField] private GameObject content;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Door exitDoor;
    [SerializeField] private Health[] enemies = Array.Empty<Health>();
    [SerializeField] private DualPressureDoor puzzle;
    [SerializeField] private bool temporalZone = true;
    [SerializeField] private bool requiresRuneKeyForExit;
    [SerializeField] private RuneKeyInventory runeKeyInventory;
    [SerializeField] private bool keepContentVisibleAfterLeave;
    private TimeLoopManager loop;
    public string DisplayName => displayName;
    public RoomRole Role => role;
    public Transform CameraAnchor => cameraAnchor != null ? cameraAnchor : transform;
    public GameObject Content => content;
    public RoomClearCondition ClearCondition => clearCondition;
    public RoomState State { get; private set; }
    public bool RequiresTemporalLoop => temporalZone;
    public bool KeepsContentVisibleAfterLeave => keepContentVisibleAfterLeave;
    public Transform SpawnPoint => spawnPoint;
    public Door ExitDoor => exitDoor;
    public event Action<Room> StateChanged;

    public bool IsConfigured => content != null && content != gameObject && content.transform.IsChildOf(transform) &&
        spawnPoint != null && exitDoor != null &&
        (clearCondition != RoomClearCondition.DefeatEnemies || (enemies != null && enemies.Length > 0 && Array.TrueForAll(enemies, e => e != null))) &&
        (clearCondition != RoomClearCondition.SolvePuzzle || puzzle != null);

    public void Initialize(TimeLoopManager timeLoop)
    {
        loop = timeLoop;
        content.SetActive(false);
        State = RoomState.NotEntered;
        foreach (Health enemy in enemies) if (enemy != null) enemy.Changed += OnEnemyChanged;
        loop.LoopRewound += Evaluate;
    }

    public void ConfigureRuneKeyGate(RuneKeyInventory inventory) => runeKeyInventory = inventory;

    public void Enter()
    {
        content.SetActive(true);
        SetState(RoomState.Active);
        exitDoor.Close();
        Evaluate();
    }

    public void Leave()
    {
        SetState(RoomState.Exited);
        if (!keepContentVisibleAfterLeave) content.SetActive(false);
    }

    private void OnEnemyChanged(Health _) => Evaluate();
    private void LateUpdate()
    {
        if (clearCondition == RoomClearCondition.SolvePuzzle) Evaluate();
    }

    public void Evaluate()
    {
        if (State != RoomState.Active || loop == null || (temporalZone && !loop.IsRunning)) return;
        if (clearCondition == RoomClearCondition.DefeatEnemies)
            foreach (Health enemy in enemies) if (enemy != null && !enemy.IsDead) return;
        if (clearCondition == RoomClearCondition.SolvePuzzle && !puzzle.IsSolved) return;
        if (requiresRuneKeyForExit && (runeKeyInventory == null || !runeKeyInventory.HasRuneKey)) return;
        SetState(RoomState.Completed);
        exitDoor.Open();
    }

    private void SetState(RoomState state)
    {
        if (State == state) return;
        State = state;
        StateChanged?.Invoke(this);
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        if (State != RoomState.Active && State != RoomState.Completed) return;
        SetState(RoomState.Active);
        exitDoor.Close();
    }

    private void OnDestroy()
    {
        foreach (Health enemy in enemies) if (enemy != null) enemy.Changed -= OnEnemyChanged;
        if (loop != null) loop.LoopRewound -= Evaluate;
    }
}
