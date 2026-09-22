#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class UpgradeRewardIntegrationPlayModeChecks : MonoBehaviour
{
    private int checks;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M7.4 FAIL: " + message);
        checks++;
        Debug.Log("M7.4 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var rewards = FindAnyObjectByType<UpgradeRewardManager>();
        var ui = FindAnyObjectByType<UpgradeChoiceUI>();
        var roomManager = FindAnyObjectByType<RoomManager>();
        Check(rewards != null && ui != null && roomManager != null, "reward manager, UI and room progression are available");
        Check(rewards.RewardCount == 0 && !rewards.IsRewardOpen, "run starts without an open reward");
        Check(!rewards.OpenReward(RoomRole.Elite), "wrong room role cannot open a reward");
        var treasure = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Treasure) treasure = candidate;
        var currentRoomField = typeof(RoomManager).GetField("<CurrentRoom>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        currentRoomField.SetValue(roomManager, treasure);
        treasure.Enter();
        Check(rewards.RewardCount == 1 && rewards.IsRewardOpen && Mathf.Approximately(Time.timeScale, 0f),
            "treasure completion opens one paused upgrade reward");
        ui.SelectChoice(0);
        Check(!rewards.IsRewardOpen && Mathf.Approximately(Time.timeScale, 1f),
            "selecting the room reward closes the panel and resumes gameplay");
        Debug.Log("M7.4 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
