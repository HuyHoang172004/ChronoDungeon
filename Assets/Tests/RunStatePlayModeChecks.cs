#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class RunStatePlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M9.1 FAIL: " + message);
        checks++;
        Debug.Log("M9.1 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var run = FindAnyObjectByType<RunStateManager>();
        var rooms = FindAnyObjectByType<RoomManager>();
        var upgrades = FindAnyObjectByType<UpgradeManager>();
        var game = FindAnyObjectByType<GameManager>();
        Check(run != null && rooms != null && upgrades != null && game != null, "run state dependencies are available");
        Check(run.State == RunState.Running && run.HasActiveRun && rooms.CurrentIndex == 0,
            "new scene starts a running run at the first room");
        float baseline = upgrades.CurrentAttackDamage;
        upgrades.ApplyUpgrade(UpgradeType.AttackDamage, 10f);
        Check(upgrades.CurrentAttackDamage == baseline + 10f, "upgrade is retained in the active run");
        game.Victory();
        Check(run.State == RunState.Victory && !run.HasActiveRun && Mathf.Approximately(Time.timeScale, 0f),
            "Victory ends the active run and pauses gameplay");
        run.BeginNewRun();
        Check(run.State == RunState.Running && run.HasActiveRun && upgrades.AppliedUpgradeCount == 0 &&
            Mathf.Approximately(upgrades.CurrentAttackDamage, baseline),
            "begin-new-run resets run upgrades and resumes gameplay");
        var player = FindAnyObjectByType<PlayerMovement>();
        var playerHealth = player.GetComponent<Health>();
        playerHealth.TakeDamage(9999f);
        game.GameOver();
        Check(run.State == RunState.GameOver && !run.HasActiveRun && Mathf.Approximately(Time.timeScale, 0f),
            "player death transitions the run to Game Over");
        run.BeginNewRun();
        Check(run.State == RunState.Running && !playerHealth.IsDead && Mathf.Approximately(Time.timeScale, 1f),
            "begin-new-run resets Game Over health and state");
        Debug.Log("M9.1 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
