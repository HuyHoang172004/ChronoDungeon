#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class ChronoGuardianFinalePlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M8.4 FAIL: " + message);
        checks++;
        Debug.Log("M8.4 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var finale = FindObjectsByType<ChronoGuardianFinale>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        var bossRoom = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Boss) bossRoom = candidate;
        bossRoom.Content.SetActive(true);
        finale.BeginFinale();
        Check(finale.IsFinaleActive, "finale phase can be entered explicitly");
        for (int i = 0; i < 30; i++) yield return null;
        Check(finale.IsHazardWindingUp, "finale starts a readable temporal hazard windup");
        for (int i = 0; i < 45; i++) yield return null;
        Check(finale.HazardCount > 0 && finale.IsVulnerableWindow,
            "hazard resolves once and opens a clear vulnerability window");
        finale.ResetToInitialState();
        Check(!finale.IsFinaleActive && !finale.IsHazardWindingUp && finale.HazardCount == 0,
            "finale hazard and pressure state reset cleanly");
        Debug.Log("M8.4 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
