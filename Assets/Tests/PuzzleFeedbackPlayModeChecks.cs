#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class PuzzleFeedbackPlayModeChecks : MonoBehaviour
{
    private PressureSwitch a, b;
    private Door door;
    private DualPressureDoor gate;
    private PuzzleFeedback feedback;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private int checks, frames;
    private bool monitor, background;
    private Vector3 aScale, bScale;
    private Color idleLine;

    private void Check(bool ok, string message)
    {
        if (!ok) { monitor = false; StopAllCoroutines(); throw new System.Exception("M2.4 FAIL: " + message); }
        checks++;
        Debug.Log("M2.4 PASS: " + message);
    }

    private void Bind()
    {
        var switches = FindObjectsByType<PressureSwitch>();
        a = switches[0]; b = switches[1];
        door = FindAnyObjectByType<Door>();
        gate = FindAnyObjectByType<DualPressureDoor>();
        feedback = FindAnyObjectByType<PuzzleFeedback>();
        player = FindAnyObjectByType<PlayerMovement>();
        loop = FindAnyObjectByType<TimeLoopManager>();
    }

    private void Place(Vector3 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        var rb = player.GetComponent<Rigidbody2D>();
        rb.position = position; rb.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }

    private IEnumerator Settle() { Physics2D.SyncTransforms(); yield return new WaitForSeconds(0.12f); }

    private IEnumerator Start()
    {
        background = Application.runInBackground; Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        Bind(); monitor = true;
        var line = feedback.GetComponent<LineRenderer>();
        var hint = feedback.GetComponentInChildren<TextMesh>();
        var audio = feedback.GetComponent<AudioSource>();
        aScale = a.transform.Find("Active Light").localScale;
        bScale = b.transform.Find("Active Light").localScale;
        idleLine = line.startColor;
        Check(feedback != null && line.positionCount == 3 && audio != null && hint != null,
            "Feedback adapter has line, hint and audio components");
        Check(!line.enabled && hint.text == "STAND ON BOTH TIME PLATES" && !a.IsActive && !b.IsActive,
            "Fresh puzzle shows tutorial hint and inactive connection");
        Place(a.transform.position); yield return Settle();
        Check(a.IsActive && line.enabled && hint.text == "ONE MORE SWITCH" && line.startColor != idleLine,
            "First switch activates line, hint and switch feedback");
        yield return new WaitForSeconds(0.2f);
        Check(a.transform.Find("Active Light").localScale != aScale,
            "Active switch receives pulse feedback");
        var holder = new GameObject("M2.4 feedback ghost holder");
        holder.transform.position = a.transform.position;
        holder.AddComponent<PressureSwitchActor>().UseReplayPoint();
        yield return Settle();
        Place(b.transform.position); yield return Settle();
        Check(a.IsActive && b.IsActive && gate.IsSolved && door.IsOpen &&
            hint.text == "TIME LINK COMPLETE" && line.startColor != idleLine,
            "Both switches show solved connection and Door-open feedback");
        Destroy(holder);
        yield return Settle();
        Place(Vector3.zero); yield return Settle();
        Check(!a.IsActive && !b.IsActive && door.IsLocked && !line.enabled &&
            hint.text == "STAND ON BOTH TIME PLATES",
            "Releasing both switches clears connection and restores hint");
        Time.timeScale = 0;
        bool before = line.enabled; string beforeHint = hint.text;
        yield return new WaitForSecondsRealtime(0.2f);
        Check(line.enabled == before && hint.text == beforeHint, "Pause freezes puzzle feedback state");
        Time.timeScale = 1;
        monitor = false;
        var oldFeedback = feedback;
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        Bind(); yield return Settle();
        Check(oldFeedback == null && !a.IsActive && !b.IsActive && door.IsLocked && loop.loopIndex == 1,
            "Restart clears feedback and puzzle state");
        Application.runInBackground = background;
        Debug.Log("M2.4 ALL CHECKS PASSED; checks=" + checks + "; frames=" + frames);
    }

    private void LateUpdate() { if (monitor && loop != null && loop.IsRunning) frames++; }
    private void OnDestroy() => Application.runInBackground = background;
}
#endif
