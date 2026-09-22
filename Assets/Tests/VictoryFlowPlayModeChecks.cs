#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public sealed class VictoryFlowPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string message) { if (!ok) throw new System.Exception("M9.4 FAIL: " + message); checks++; Debug.Log("M9.4 PASS: " + message); }
    private IEnumerator Start()
    {
        yield return null; var game=FindAnyObjectByType<GameManager>(); var ui=FindAnyObjectByType<VictoryFlowUI>(); var run=FindAnyObjectByType<RunStateManager>();
        Check(game!=null && ui!=null && run!=null, "Victory flow dependencies are available");
        game.Victory(); Check(game.IsVictory && ui.IsVisible && Mathf.Approximately(Time.timeScale,0f), "Victory screen opens and pauses the run");
        game.BeginNewRunState(); Check(!game.IsVictory && !ui.IsVisible && Mathf.Approximately(Time.timeScale,1f) && run.State==RunState.Running, "new run clears Victory state");
        Check(typeof(GameManager).GetMethod("ReplayRun") != null, "Replay Run action is available");
        Debug.Log("M9.4 ALL CHECKS PASSED; checks="+checks); Destroy(gameObject);
    }
    private void OnDestroy()=>Time.timeScale=1f;
}
#endif
