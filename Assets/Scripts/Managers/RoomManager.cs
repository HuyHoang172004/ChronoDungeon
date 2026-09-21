using System;
using UnityEngine;

[DefaultExecutionOrder(500)]
public sealed class RoomManager : MonoBehaviour
{
    [SerializeField] private Room[] rooms;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private TimeLoopManager loop;
    public Room CurrentRoom { get; private set; }
    public int CurrentIndex { get; private set; } = -1;
    public int RoomCount => rooms == null ? 0 : rooms.Length;
    public bool IsComplete { get; private set; }
    public PlayerMovement Player => player;
    public event Action ProgressChanged;
    private int transitionFrame = -1;

    private void Start()
    {
        if (player == null || loop == null || rooms == null || rooms.Length == 0 ||
            !Array.TrueForAll(rooms, r => r != null && r.IsConfigured))
        {
            Debug.LogError("RoomManager requires player, loop and configured rooms.", this);
            enabled = false;
            return;
        }
        foreach (Room room in rooms)
        {
            room.Initialize(loop);
            room.StateChanged += OnRoomStateChanged;
        }
        Enter(0);
    }

    private void Enter(int index)
    {
        transitionFrame = Time.frameCount;
        if (CurrentRoom != null) CurrentRoom.Leave();
        CurrentIndex = index;
        CurrentRoom = rooms[index];
        // Cancel the previous dash/input before capturing the new spawn.
        var dash = player.GetComponent<PlayerDash>();
        if (dash != null) dash.ResetToInitialState();
        player.SetMoveDirection(Vector2.zero);
        var body = player.GetComponent<Rigidbody2D>();
        player.transform.position = CurrentRoom.SpawnPoint.position;
        body.position = CurrentRoom.SpawnPoint.position;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        CurrentRoom.Enter();
        loop.BeginEncounter(player.gameObject, CurrentRoom.gameObject);
        ProgressChanged?.Invoke();
    }

    public bool TryAdvance(Room source, PlayerMovement actor)
    {
        if (!enabled || actor != player || IsComplete || CurrentRoom == null || source != CurrentRoom ||
            !loop.IsRunning || transitionFrame == Time.frameCount || CurrentRoom.State != RoomState.Completed ||
            !CurrentRoom.ExitDoor.IsOpen) return false;
        if (CurrentIndex + 1 < rooms.Length) Enter(CurrentIndex + 1);
        else
        {
            IsComplete = true;
            loop.enabled = false;
            ProgressChanged?.Invoke();
        }
        return true;
    }

    private void OnRoomStateChanged(Room _) => ProgressChanged?.Invoke();
    private void OnDestroy()
    {
        if (rooms == null) return;
        foreach (Room room in rooms) if (room != null) room.StateChanged -= OnRoomStateChanged;
    }
}
