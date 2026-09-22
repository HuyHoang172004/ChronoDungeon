#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class ChronoGuardianFoundationPlayModeChecks : MonoBehaviour
{
    private int checks;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M8.1 FAIL: " + message);
        checks++;
        Debug.Log("M8.1 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var boss = FindObjectsByType<ChronoGuardian>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        var hud = FindAnyObjectByType<ChronoGuardianHUD>();
        var arena = FindAnyObjectByType<ChronoGuardianArena>();
        Check(boss != null && hud != null && arena != null, "boss, HUD and arena foundation are present");
        Check(Mathf.Approximately(boss.Health.maxHealth, boss.BossHealth) &&
            Mathf.Approximately(boss.Health.currentHealth, boss.BossHealth),
            "boss health is initialized to its authored maximum");
        Check(arena.IsConfigured && arena.Boss == boss && arena.ArenaSize.x > 0f && arena.ArenaSize.y > 0f,
            "boss arena has a configured boss reference and bounds");
        var bossRoom = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Boss) bossRoom = candidate;
        bossRoom.Content.SetActive(true);
        Check(hud.IsVisible && boss.IntroShown, "boss intro activates the HUD on boss enable");
        boss.Health.TakeDamage(100f);
        boss.ResetToInitialState();
        Check(Mathf.Approximately(boss.Health.currentHealth, boss.BossHealth) && !boss.Health.IsDead,
            "boss reset lifecycle restores full health");
        Debug.Log("M8.1 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
