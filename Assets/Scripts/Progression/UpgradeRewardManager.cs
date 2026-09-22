using UnityEngine;

[DefaultExecutionOrder(600)]
[DisallowMultipleComponent]
public sealed class UpgradeRewardManager : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private UpgradeChoiceUI choiceUI;
    private readonly System.Collections.Generic.HashSet<Room> rewardedRooms = new System.Collections.Generic.HashSet<Room>();

    public int RewardCount { get; private set; }
    public bool IsRewardOpen => choiceUI != null && choiceUI.IsShowing;

    private void Awake()
    {
        if (roomManager == null) roomManager = FindAnyObjectByType<RoomManager>();
        if (choiceUI == null) choiceUI = FindAnyObjectByType<UpgradeChoiceUI>();
    }

    private void OnEnable()
    {
        if (roomManager != null) roomManager.ProgressChanged += OnProgressChanged;
    }

    private void OnProgressChanged() 
    {
        Room room = roomManager == null ? null : roomManager.CurrentRoom;
        if (room == null || room.State != RoomState.Completed) return;
        if (room.Role != RoomRole.Treasure && room.Role != RoomRole.Elite && room.Role != RoomRole.CombatChallenge) return;
        OpenReward(room);
    }

    public bool OpenReward(RoomRole role)
    {
        if (roomManager == null || roomManager.CurrentRoom == null || roomManager.CurrentRoom.Role != role)
            return false;
        return OpenReward(roomManager.CurrentRoom);
    }

    private bool OpenReward(Room room)
    {
        if (room == null || rewardedRooms.Contains(room) || choiceUI == null) return false;
        rewardedRooms.Add(room);
        RewardCount++;
        choiceUI.ShowChoices();
        return true;
    }

    private void OnDisable()
    {
        if (roomManager != null) roomManager.ProgressChanged -= OnProgressChanged;
    }
}
