#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class DualPressurePuzzlePlayModeChecks : MonoBehaviour
{
    private PressureSwitch first, second;
    private DualPressureDoor gate;
    private Door door;
    private PlayerMovement player;
    private TemporalGhostManager ghosts;
    private TimeLoopManager loop;
    private EnemyFollow enemy;
    private int checks, frames, replayActions;
    private float worstPosition;
    private bool monitor, resetSeen, background;

    private void Check(bool ok, string message)
    {
        if (!ok) { monitor = false; StopAllCoroutines(); throw new System.Exception("M2.3 FAIL: " + message); }
        checks++;
        Debug.Log("M2.3 PASS: " + message);
    }

    private void Bind()
    {
        var switches = FindObjectsByType<PressureSwitch>();
        first = switches[0].transform.position.y > switches[1].transform.position.y ? switches[0] : switches[1];
        second = first == switches[0] ? switches[1] : switches[0];
        gate = FindAnyObjectByType<DualPressureDoor>();
        door = FindAnyObjectByType<Door>();
        player = FindAnyObjectByType<PlayerMovement>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        enemy = FindAnyObjectByType<EnemyFollow>();
        enemy.enabled = false;
        enemy.GetComponent<EnemyContactDamage>().enabled = false;
    }

    private void Place(Component actor, Vector3 position)
    {
        actor.transform.position = position;
        var rb = actor.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = position; rb.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private IEnumerator Settle()
    {
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator Rewind()
    {
        int old = loop.loopIndex;
        resetSeen = false;
        float deadline = Time.realtimeSinceStartup + 25f;
        while (loop.loopIndex == old && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == old + 1, "Natural rewind completes");
        Check(resetSeen && !first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked,
            "Rewind resets both switches, dual gate and Door");
    }

    private IEnumerator At(float time)
    {
        while (loop.ElapsedTime < time) yield return null;
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        FindAnyObjectByType<GameManager>().Restart();
        yield return null;
        yield return null;
        Bind();
        monitor = true;
        loop.LoopRewound += () => resetSeen = true;
        Check(first != null && second != null && gate != null && door != null,
            "Two switches and referenced dual mechanism exist");
        Check(!first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked,
            "Fresh puzzle starts unsolved with Door closed");
        var initial = player.transform.position;
        enemy.GetComponent<TimeLoopActor>().CaptureInitialState();
        float firstTime = 1.5f;
        yield return At(firstTime);
        Place(player, first.transform.position);
        yield return Settle();
        Check(first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked,
            "Loop 1 Player holds Switch A alone; Door remains locked");
        float aTime = loop.ElapsedTime;
        yield return new WaitForSeconds(0.5f);
        Place(player, initial);
        yield return Settle();
        Check(!first.IsActive && !second.IsActive, "Player leaving Switch A releases it in Loop 1");
        float deadline = Time.realtimeSinceStartup + 22f;
        while (loop.loopIndex == 1 && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == 2 && ghosts.ActiveGhostCount == 1, "Loop 2 creates Ghost 1 from Loop 1");
        var ghost = ghosts.ActiveGhost;
        ghost.ActionReplayed += _ => replayActions++;
        yield return At(aTime + 0.1f);
        Check(first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked,
            "Ghost 1 replays Switch A while Player is elsewhere");
        Place(player, second.transform.position);
        yield return Settle();
        Check(first.IsActive && second.IsActive && gate.IsSolved && door.IsOpen,
            "Player activates Switch B while Ghost holds A; Door opens");
        Check(!door.GetComponent<BoxCollider2D>().enabled && !door.transform.Find("Closed Panel").gameObject.activeSelf,
            "Solved dual mechanism synchronizes Door collider and visual");
        Place(player, initial);
        yield return Settle();
        Check(first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked,
            "Player leaving B closes Door while Ghost still holds A");
        Place(player, second.transform.position);
        yield return Settle();
        Check(first.IsActive && second.IsActive && gate.IsSolved && door.IsOpen,
            "Player returning to B reopens Door without another rewind");
        yield return At(5f);
        Check(replayActions == 0, "Puzzle movement does not fabricate action events");
        // Regression: attack and live Enemy remain valid while puzzle is solved.
        var hp = player.GetComponent<Health>();
        var enemyHp = enemy.GetComponent<Health>();
        Place(enemy, player.transform.position + Vector3.right * 1.5f);
        player.GetComponent<PlayerAttack>().Attack();
        Check(enemyHp.currentHealth == 75 && hp.currentHealth == 100, "Player combat remains valid in solved puzzle");
        yield return Rewind();
        Check(first.IsActive == false && second.IsActive == false && door.IsLocked,
            "Post-rewind puzzle state is clean before next loop");
        monitor = false;
        var oldGate = gate;
        var game = FindAnyObjectByType<GameManager>();
        game.Restart();
        yield return null;
        yield return null;
        Bind();
        yield return Settle();
        Check(oldGate == null && !first.IsActive && !second.IsActive && !gate.IsSolved && door.IsLocked &&
            ghosts.ActiveGhostCount == 0 && loop.loopIndex == 1 && Time.timeScale == 1,
            "Restart clears puzzle, Door and Ghost state");
        Application.runInBackground = background;
        Debug.Log("M2.3 ALL CHECKS PASSED; checks=" + checks + "; frames=" + frames +
            "; replayActions=" + replayActions + "; worstPosition=" + worstPosition);
    }

    private void LateUpdate()
    {
        if (!monitor || loop == null || !loop.IsRunning) return;
        for (int i = 0; i < ghosts.ActiveGhostCount; i++)
        {
            var ghost = ghosts.GetGhost(i);
            float error = Vector3.Distance(ghost.transform.position, ghost.Timeline.Evaluate(loop.ElapsedTime).Position);
            worstPosition = Mathf.Max(worstPosition, error);
            if (error > 0.001f) Check(false, "Dual puzzle changed Ghost movement replay");
        }
        frames++;
    }

    private void OnDestroy() => Application.runInBackground = background;
}
#endif
