#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class UpgradeFrameworkPlayModeChecks : MonoBehaviour
{
    private int checks;
    private UpgradeManager upgrades;
    private TimeLoopManager loop;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M7.1 FAIL: " + message);
        checks++;
        Debug.Log("M7.1 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        upgrades = FindAnyObjectByType<UpgradeManager>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        Check(upgrades != null && upgrades.AppliedUpgradeCount == 0,
            "Upgrade manager starts with a clean current-run state");
        float attack = upgrades.CurrentAttackDamage;
        float speed = upgrades.CurrentMoveSpeed;
        float dash = upgrades.CurrentDashCooldown;
        float maxHealth = upgrades.CurrentMaxHealth;
        upgrades.ApplyUpgrade(UpgradeType.AttackDamage, 10f);
        upgrades.ApplyUpgrade(UpgradeType.MovementSpeed, 1f);
        upgrades.ApplyUpgrade(UpgradeType.DashCooldown, .2f);
        upgrades.ApplyUpgrade(UpgradeType.MaxHealth, 20f);
        Check(upgrades.AppliedUpgradeCount == 4 && Mathf.Approximately(upgrades.CurrentAttackDamage, attack + 10f) &&
            Mathf.Approximately(upgrades.CurrentMoveSpeed, speed + 1f) && Mathf.Approximately(upgrades.CurrentDashCooldown, dash - .2f) &&
            Mathf.Approximately(upgrades.CurrentMaxHealth, maxHealth + 20f),
            "Upgrade data applies attack, movement, dash and max-health effects");
        typeof(TimeLoopManager).GetMethod("Rewind", System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance).Invoke(loop, null);
        yield return null;
        Check(upgrades.AppliedUpgradeCount == 4 && Mathf.Approximately(upgrades.CurrentAttackDamage, attack + 10f) &&
            Mathf.Approximately(upgrades.CurrentMoveSpeed, speed + 1f),
            "Loop rewind preserves upgrades for the current run");
        upgrades.ResetRun();
        Check(upgrades.AppliedUpgradeCount == 0 && Mathf.Approximately(upgrades.CurrentAttackDamage, attack) &&
            Mathf.Approximately(upgrades.CurrentMoveSpeed, speed) && Mathf.Approximately(upgrades.CurrentDashCooldown, dash) &&
            Mathf.Approximately(upgrades.CurrentMaxHealth, maxHealth),
            "Explicit new-run reset restores baseline stats and clears stacks");
        Debug.Log("M7.1 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
