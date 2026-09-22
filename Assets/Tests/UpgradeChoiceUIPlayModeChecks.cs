#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class UpgradeChoiceUIPlayModeChecks : MonoBehaviour
{
    private int checks;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M7.2 FAIL: " + message);
        checks++;
        Debug.Log("M7.2 PASS: " + message);
    }

    private IEnumerator Start()
    {
        yield return null;
        var ui = FindAnyObjectByType<UpgradeChoiceUI>();
        var upgrades = FindAnyObjectByType<UpgradeManager>();
        Check(ui != null && upgrades != null, "choice UI and upgrade manager are available");
        Check(ui.CurrentChoices.Count == 3, "exactly three readable choices are defined");
        Check(!ui.IsShowing && Time.timeScale > 0f, "choice panel starts hidden without pausing gameplay");
        int before = upgrades.AppliedUpgradeCount;
        ui.ShowChoices();
        Check(ui.IsShowing && Mathf.Approximately(Time.timeScale, 0f), "showing choices pauses gameplay");
        Check(ui.ChoiceButtonCount == 3, "three touch-capable buttons are built");
        Button choiceButton = null;
        foreach (var candidate in ui.GetComponentsInChildren<Button>(true))
            if (candidate.name == "Upgrade Choice 1")
                choiceButton = candidate;
        Check(choiceButton != null, "first choice is wired to a UI button");
        choiceButton.onClick.Invoke();
        Check(!ui.IsShowing && Mathf.Approximately(Time.timeScale, 1f) &&
            upgrades.AppliedUpgradeCount == before + 1,
            "selecting one choice applies it and resumes gameplay");
        Debug.Log("M7.2 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
