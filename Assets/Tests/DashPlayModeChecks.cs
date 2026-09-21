#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class DashPlayModeChecks : MonoBehaviour
{
    private PlayerMovement player;
    private PlayerDash dash;
    private PlayerTimelineRecorder recorder;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private int checks;
    private bool background;
    private bool dashReplayed;
    private GhostPlayback observedGhost;

    private void Check(bool ok, string message)
    {
        if (!ok) { StopAllCoroutines(); throw new System.Exception("M3.2 FAIL: " + message); }
        checks++;
        Debug.Log("M3.2 PASS: " + message);
    }

    private IEnumerator WaitForLoop(int source)
    {
        float deadline = Time.realtimeSinceStartup + 30f;
        while (loop.loopIndex == source && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == source + 1, "Natural loop rewind completes");
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        yield return new WaitForSeconds(0.5f);

        player = FindAnyObjectByType<PlayerMovement>();
        dash = player != null ? player.GetComponent<PlayerDash>() : null;
        recorder = FindAnyObjectByType<PlayerTimelineRecorder>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        Check(player != null && dash != null && recorder != null && loop != null && ghosts != null,
            "PlayerDash, recorder, loop and Ghost manager are available");
        Check(dash.DashDistance > 0f && dash.DashCooldown > 0f && FindAnyObjectByType<DashButton>() != null,
            "Dash distance, cooldown and mobile Dash button are configured");

        player.SetMoveDirection(Vector2.right);
        Vector2 start = player.GetComponent<Rigidbody2D>().position;
        dash.Dash();
        Vector2 after = player.GetComponent<Rigidbody2D>().position;
        Check(Vector2.Distance(start, after) >= dash.DashDistance - 0.01f &&
            Vector2.Dot(after - start, Vector2.right) > 0f,
            "Dash moves Player in current facing direction");
        Check(!dash.CanDash && dash.GetComponent<DashFeedback>() != null &&
            dash.GetComponent<DashFeedback>().GetComponent<LineRenderer>().enabled,
            "Dash cooldown and visual feedback activate");

        int eventsAfterDash = recorder.CurrentRecording.ActionCount;
        Check(eventsAfterDash >= 1 &&
            recorder.CurrentRecording.GetAction(eventsAfterDash - 1).Kind == PlayerTimeline.ActionKind.Dash &&
            recorder.CurrentRecording.GetAction(eventsAfterDash - 1).Direction == Vector2.right &&
            recorder.CurrentRecording.GetAction(eventsAfterDash - 1).Payload == Mathf.RoundToInt(dash.DashDistance * 1000f),
            "Timeline records Dash event, direction and distance payload");

        Vector2 blockedPosition = player.GetComponent<Rigidbody2D>().position;
        dash.Dash();
        Check(player.GetComponent<Rigidbody2D>().position == blockedPosition &&
            recorder.CurrentRecording.ActionCount == eventsAfterDash,
            "Cooldown blocks accidental repeated dash");

        yield return new WaitForSeconds(dash.DashCooldown + 0.05f);
        player.SetMoveDirection(Vector2.up);
        Vector2 beforeSecond = player.GetComponent<Rigidbody2D>().position;
        dash.Dash();
        Check(Vector2.Dot(player.GetComponent<Rigidbody2D>().position - beforeSecond, Vector2.up) > 0f &&
            recorder.CurrentRecording.ActionCount == eventsAfterDash + 1,
            "Dash becomes available and follows updated facing");

        Time.timeScale = 0f;
        Vector2 pausedPosition = player.GetComponent<Rigidbody2D>().position;
        int pausedActions = recorder.CurrentRecording.ActionCount;
        dash.Dash();
        yield return new WaitForSecondsRealtime(0.1f);
        Check(player.GetComponent<Rigidbody2D>().position == pausedPosition &&
            recorder.CurrentRecording.ActionCount == pausedActions,
            "Pause rejects Dash and freezes timeline");
        Time.timeScale = 1f;

        yield return WaitForLoop(loop.loopIndex);
        Check(ghosts.ActiveGhostCount == 1 && ghosts.ActiveGhost.Timeline.ActionCount >= 2,
            "Ghost receives recorded Dash timeline");
        observedGhost = ghosts.ActiveGhost;
        observedGhost.ActionReplayed += action => {
            if (action.Kind == PlayerTimeline.ActionKind.Dash) dashReplayed = true;
        };
        float replayDeadline = Time.realtimeSinceStartup + 3f;
        while (!dashReplayed && Time.realtimeSinceStartup < replayDeadline) yield return null;
        Check(dashReplayed && observedGhost.GetComponent<DashFeedback>() != null,
            "Ghost replays Dash event and visual feedback without Player input");

        Debug.Log("M3.2 ALL CHECKS PASSED; checks=" + checks);
        Application.runInBackground = background;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Application.runInBackground = background;
    }
}
#endif
