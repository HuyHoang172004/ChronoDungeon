#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach only during Play Mode through MCP. All fixtures are runtime-only.
[DefaultExecutionOrder(10000)]
public sealed class ActionTimelinePlayModeChecks : MonoBehaviour
{
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private PlayerTimelineRecorder recorder;
    private PlayerMovement player;
    private Health enemy, hp;
    private int checks, frames, replayCount;
    private float worstTiming, worstPosition;
    private bool monitor;
    private bool background;
    private readonly HashSet<GhostPlayback> observed = new HashSet<GhostPlayback>();
    private readonly Dictionary<int, int> counts = new Dictionary<int, int>();

    private void Check(bool ok, string message)
    {
        if (!ok) { monitor = false; StopAllCoroutines(); throw new System.Exception("M1.3 FAIL: " + message); }
        checks++;
        Debug.Log("M1.3 PASS: " + message);
    }

    private void Bind()
    {
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        recorder = FindAnyObjectByType<PlayerTimelineRecorder>();
        player = FindAnyObjectByType<PlayerMovement>();
        hp = player.GetComponent<Health>();
        var follow = FindAnyObjectByType<EnemyFollow>();
        follow.enabled = false;
        follow.GetComponent<EnemyContactDamage>().enabled = false;
        enemy = follow.GetComponent<Health>();
    }

    private void Observe()
    {
        for (int i = 0; i < ghosts.ActiveGhostCount; i++)
        {
            var ghost = ghosts.GetGhost(i);
            if (!observed.Add(ghost)) continue;
            ghost.ActionReplayed += action => {
                replayCount++;
                int key = loop.loopIndex * 10 + ghost.Timeline.SourceLoop;
                counts[key] = counts.TryGetValue(key, out int count) ? count + 1 : 1;
                float delay = loop.ElapsedTime - action.Time;
                worstTiming = Mathf.Max(worstTiming, delay);
                Check(delay >= -0.0001f && delay <= Time.deltaTime + 0.005f,
                    "Replay timing within one frame, source " + ghost.Timeline.SourceLoop);
            };
        }
    }

    private void LateUpdate()
    {
        if (!monitor || ghosts == null) return;
        Observe();
        if (ghosts.ActiveGhostCount > 3) Check(false, "Ghost limit");
        if (!loop.IsRunning) return;
        for (int i = 0; i < ghosts.ActiveGhostCount; i++)
        {
            var g = ghosts.GetGhost(i);
            float error = Vector3.Distance(g.transform.position, g.Timeline.Evaluate(loop.ElapsedTime).Position);
            worstPosition = Mathf.Max(worstPosition, error);
            if (error > 0.001f) Check(false, "Movement replay divergence");
        }
        frames++;
    }

    private IEnumerator At(float time)
    {
        while (loop.ElapsedTime < time) yield return null;
    }

    private IEnumerator Rewind()
    {
        int index = loop.loopIndex;
        float deadline = Time.realtimeSinceStartup + 35f;
        while (loop.loopIndex == index && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == index + 1, "Natural 20-second rewind " + index);
        Observe();
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        // Start from a fresh run even if MCP entered Play Mode a few seconds ago.
        FindAnyObjectByType<GameManager>().Restart();
        yield return null;
        yield return null;
        Bind();
        Check(loop.LoopDuration == 20 && ghosts.MaxGhosts == 3, "20-second loops, maxGhosts=3");
        enemy.GetComponent<Rigidbody2D>().position = new Vector2(1.5f, 0);
        enemy.transform.position = new Vector3(1.5f, 0, 0);
        enemy.GetComponent<TimeLoopActor>().CaptureInitialState();
        enemy.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        var neutral = new GameObject("M1.3 neutral fixture");
        neutral.transform.position = Vector3.up;
        var neutralHP = neutral.AddComponent<Health>();
        neutral.AddComponent<CircleCollider2D>().isTrigger = true;
        Physics2D.SyncTransforms();
        monitor = true;
        player.SetMoveDirection(Vector2.up);
        yield return new WaitForSeconds(0.1f);
        player.SetMoveDirection(Vector2.zero);
        yield return At(0.75f);
        var attack = player.GetComponent<PlayerAttack>();
        attack.Attack();
        Check(enemy.currentHealth == 75 && hp.currentHealth == 100 && neutralHP.currentHealth == 100,
            "Player attack: exactly 25 damage across two colliders; Player/neutral safe");
        yield return At(1.5f);
        attack.Attack();
        Check(enemy.currentHealth == 50 && recorder.CurrentRecording.ActionCount == 2,
            "Two live attacks create exactly two events");
        var first = recorder.CurrentRecording.GetAction(0);
        var second = recorder.CurrentRecording.GetAction(1);
        Check(first.Direction == Vector2.up && first.Attack.Direction == Vector2.up &&
            first.Attack.Damage == 25 && first.Attack.Range == 2 && first.Time < second.Time,
            "Action snapshots preserve direction, damage, range and ordered timestamps");
        yield return Rewind();
        Check(ghosts.ActiveGhostCount == 1 && enemy.currentHealth == 100,
            "Loop 2 creates one Ghost and restores Enemy");
        Check(ghosts.ActiveGhost.Timeline.GetAction(0).Equals(first), "Snapshot retains exact action payload");
        yield return At(first.Time - 0.15f);
        Check(enemy.currentHealth == 100 && replayCount == 0, "No early Ghost attack");
        Time.timeScale = 0;
        float frozenTime = loop.ElapsedTime;
        attack.Attack();
        yield return new WaitForSecondsRealtime(0.25f);
        Check(loop.ElapsedTime == frozenTime && replayCount == 0 && recorder.CurrentRecording.ActionCount == 0,
            "Pause freezes actions and rejects Player attacks");
        Time.timeScale = 1;
        yield return At(first.Time + 0.15f);
        Check(enemy.currentHealth == 75 && replayCount == 1 && hp.currentHealth == 100,
            "Ghost first attack deals exactly 25 damage, never Player damage");
        yield return At(second.Time + 0.15f);
        Check(enemy.currentHealth == 50 && replayCount == 2 && neutralHP.currentHealth == 100,
            "Ghost second attack once only; explicit target filter");
        yield return At(2f);
        Check(enemy.currentHealth == 50 && replayCount == 2, "No duplicate damage on later frames");
        attack.Attack();
        Check(enemy.currentHealth == 25 && recorder.CurrentRecording.ActionCount == 1,
            "Live Player can still attack alongside Ghost");
        yield return Rewind();
        yield return At(2.3f);
        Check(ghosts.ActiveGhostCount == 2 && enemy.currentHealth == 25 && counts[31] == 2 && counts[32] == 1,
            "Two Ghosts independently replay two plus one attacks, exact total damage 75");
        yield return Rewind();
        yield return At(2.3f);
        Check(ghosts.ActiveGhostCount == 3 && enemy.currentHealth == 25 && counts[41] == 2 && counts[42] == 1,
            "Three Ghosts preserve independent action history");
        yield return Rewind();
        yield return At(2.3f);
        Check(ghosts.ActiveGhostCount == 3 && ghosts.GetGhost(0).Timeline.SourceLoop == 2 &&
            ghosts.GetGhost(2).Timeline.SourceLoop == 4 && enemy.currentHealth == 75 && counts[52] == 1,
            "Oldest slot reuse clears old actions; only retained attack deals damage");
        hp.TakeDamage(10000);
        var game = FindAnyObjectByType<GameManager>();
        Check(game.IsGameOver && Time.timeScale == 0, "Game Over still triggers");
        int oldReplays = replayCount, oldActions = recorder.CurrentRecording.ActionCount;
        frozenTime = loop.ElapsedTime;
        attack.Attack();
        yield return new WaitForSecondsRealtime(0.25f);
        Check(replayCount == oldReplays && recorder.CurrentRecording.ActionCount == oldActions &&
            frozenTime == loop.ElapsedTime, "Game Over freezes recording and all Ghost actions");
        monitor = false;
        var oldGhost = ghosts.ActiveGhost;
        game.Restart();
        yield return null;
        yield return null;
        Bind();
        Check(oldGhost == null && ghosts.ActiveGhostCount == 0 && loop.loopIndex == 1 &&
            recorder.CurrentRecording.ActionCount == 0 && Time.timeScale == 1 && hp.currentHealth == 100,
            "Restart clears Ghost/action history and restores live Player");
        Application.runInBackground = background;
        Debug.Log("M1.3 ALL CHECKS PASSED; checks=" + checks + "; frames=" + frames +
            "; replayed=" + replayCount + "; worstTiming=" + worstTiming + "; worstPosition=" + worstPosition);
    }

    private void OnDestroy() => Application.runInBackground = background;
}
#endif
