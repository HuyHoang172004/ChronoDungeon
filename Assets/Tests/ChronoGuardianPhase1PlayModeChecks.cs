#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class ChronoGuardianPhase1PlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M8.2 FAIL: " + message);
        checks++;
        Debug.Log("M8.2 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var phase = FindObjectsByType<ChronoGuardianPhase1>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        var boss = phase.GetComponent<ChronoGuardian>();
        var arena = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Boss) arena = candidate;
        arena.Content.SetActive(true);
        phase.transform.position = FindAnyObjectByType<PlayerMovement>().transform.position;
        Check(phase != null && boss != null, "Phase 1 attack component is present on Chrono Guardian");
        Check(phase.ImpactRadius > 0f && phase.WindupDuration > 0f, "attack has authored range and windup timing");
        for (int i = 0; i < 45; i++) yield return null;
        Check(phase.IsWindingUp || phase.AttackCount > 0, "boss enters readable windup or completes an attack");
        for (int i = 0; i < 75; i++) yield return null;
        Check(phase.AttackCount > 0, "telegraphed attack reaches a single impact");
        phase.ResetToInitialState();
        Check(!phase.IsWindingUp && phase.AttackCount == 0, "phase attack resets cleanly with the loop lifecycle");
        Debug.Log("M8.2 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
