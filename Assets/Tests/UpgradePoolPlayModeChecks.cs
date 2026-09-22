#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UpgradePoolPlayModeChecks : MonoBehaviour
{
    private int checks;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M7.3 FAIL: " + message);
        checks++;
        Debug.Log("M7.3 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var upgrades = FindAnyObjectByType<UpgradeManager>();
        var ui = FindAnyObjectByType<UpgradeChoiceUI>();
        Check(upgrades != null && ui != null, "upgrade pool and choice UI are available");
        IReadOnlyList<UpgradeData> pool = upgrades.UpgradePool;
        Check(pool.Count >= 8 && pool.Count <= 12, "pool contains a focused set of 8-12 upgrades");
        var ids = new HashSet<string>();
        foreach (UpgradeData data in pool)
            Check(data != null && !string.IsNullOrEmpty(data.id) && ids.Add(data.id) &&
                !string.IsNullOrEmpty(data.title) && !string.IsNullOrEmpty(data.description),
                "pool entry has unique id, title and description: " + (data == null ? "null" : data.id));
        Check(pool[0].type == UpgradeType.AttackDamage && pool[1].type == UpgradeType.MovementSpeed &&
            pool[2].type == UpgradeType.MaxHealth && pool[3].type == UpgradeType.DashCooldown &&
            pool[4].type == UpgradeType.Heal && pool[5].type == UpgradeType.LoopDuration &&
            pool[6].type == UpgradeType.GhostDamage && pool[7].type == UpgradeType.TemporalAbility,
            "pool covers the intended progression categories");
        Check(ui.CurrentChoices.Count == 3 && ui.CurrentChoices[0].id == pool[0].id &&
            ui.CurrentChoices[1].id == pool[1].id && ui.CurrentChoices[2].id == pool[2].id,
            "M7.2 choice UI consumes the upgrade pool definitions");
        Debug.Log("M7.3 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
