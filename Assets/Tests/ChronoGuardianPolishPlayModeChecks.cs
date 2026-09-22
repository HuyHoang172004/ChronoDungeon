#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class ChronoGuardianPolishPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M8.5 FAIL: " + message);
        checks++;
        Debug.Log("M8.5 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var polish = FindObjectsByType<ChronoGuardianPolish>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        var boss = polish.GetComponent<ChronoGuardian>();
        var gameManager = FindAnyObjectByType<GameManager>();
        var victory = FindAnyObjectByType<VictoryFlowUI>();
        var room = FindAnyObjectByType<Room>();
        foreach (var candidate in FindObjectsByType<Room>(FindObjectsSortMode.None))
            if (candidate.Role == RoomRole.Boss) room = candidate;
        room.Content.SetActive(true);
        Check(polish != null && gameManager != null && victory != null, "boss polish, GameManager and Victory UI are present");
        boss.Health.TakeDamage(boss.Health.currentHealth * .6f);
        Check(polish != null, "boss hit feedback component accepts damage");
        boss.Health.TakeDamage(boss.Health.currentHealth);
        Check(polish.DeathStarted, "boss death sequence starts on lethal damage");
        for (int i = 0; i < 700; i++) yield return null;
        Check(gameManager.IsVictory && victory.IsVisible && Mathf.Approximately(Time.timeScale, 0f),
            "death sequence triggers paused Victory flow");
        Debug.Log("M8.5 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
