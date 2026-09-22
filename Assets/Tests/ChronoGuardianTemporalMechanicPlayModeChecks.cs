#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class ChronoGuardianTemporalMechanicPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M8.3 FAIL: " + message);
        checks++;
        Debug.Log("M8.3 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var shields = FindObjectsByType<ChronoGuardianTemporalShield>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Check(shields.Length > 0, "Chrono Guardian temporal shield is present");
        var shield = shields[0];
        var boss = shield.GetComponent<ChronoGuardian>();
        var bossRoom = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Boss) bossRoom = candidate;
        bossRoom.Content.SetActive(true);
        shield.ResetToInitialState();
        Check(shield.IsShieldActive && !shield.IsVulnerable, "boss begins protected by a readable temporal shield");
        Check(!shield.TryReceiveDamage(false), "current Player damage is blocked while shield is active");
        Check(shield.IsShieldActive, "Player attack alone does not break the shield");
        Check(!shield.TryReceiveDamage(true) && shield.IsVulnerable,
            "Ghost cooperation breaks the shield and exposes a vulnerability window");
        Check(shield.TryReceiveDamage(false), "Player damage is accepted after Ghost cooperation");
        shield.ResetToInitialState();
        Check(shield.IsShieldActive && !boss.Health.IsDead, "rewind reset restores shield state without killing the boss");
        Debug.Log("M8.3 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
