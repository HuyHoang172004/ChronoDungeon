#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class PauseFlowPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message) { if (!ok) throw new System.Exception("M9.2 FAIL: " + message); checks++; Debug.Log("M9.2 PASS: " + message); }

    private IEnumerator Start()
    {
        yield return null;
        var pause = FindAnyObjectByType<PauseManager>(); var ui = FindAnyObjectByType<PauseFlowUI>();
        var loop = FindAnyObjectByType<TimeLoopManager>(); var run = FindAnyObjectByType<RunStateManager>();
        Check(pause != null && ui != null && loop != null && run != null, "pause flow dependencies are available");
        float before = loop.remainingTime; pause.Pause();
        Check(pause.IsPaused && ui.IsVisible && Mathf.Approximately(Time.timeScale, 0f), "pause opens UI and freezes time");
        Check(Mathf.Approximately(loop.remainingTime, before) && run.State == RunState.Running, "pause preserves loop timer and run state");
        pause.Resume();
        Check(!pause.IsPaused && !ui.IsVisible && Mathf.Approximately(Time.timeScale, 1f), "resume closes UI and restores gameplay");
        pause.Pause(); pause.Resume();
        Check(run.State == RunState.Running && !pause.IsPaused, "repeated pause/resume does not corrupt the run");
        Debug.Log("M9.2 ALL CHECKS PASSED; checks=" + checks); Destroy(gameObject);
    }
    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
