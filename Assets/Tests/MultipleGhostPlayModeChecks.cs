#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Temporary editor-only probe; never attach to the saved GameScene.
[DefaultExecutionOrder(10000)]
public sealed class MultipleGhostPlayModeChecks : MonoBehaviour
{
    private static MultipleGhostPlayModeChecks instance;
    private TimeLoopManager loop;
    private PlayerTimelineRecorder recorder;
    private TemporalGhostManager ghosts;
    private PlayerMovement player;
    private VirtualJoystick joystick;
    private readonly Dictionary<int, PlayerTimeline> expected = new Dictionary<int, PlayerTimeline>();
    private int assertions, frames, maximumActive;
    private float worstError;
    private bool monitor;
    private bool originalBackground;

    private void Awake()
    {
        // Editor scene reload can restore an unsaved edit-mode probe. The
        // persistent runner alone owns the test across GameManager.Restart.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        originalBackground = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    private void Check(bool condition, string message)
    {
        if (!condition)
        {
            monitor = false;
            throw new System.InvalidOperationException("M1.2 FAIL: " + message);
        }
        assertions++;
        Debug.Log("M1.2 PASS: " + message);
    }

    private void Bind()
    {
        loop = FindAnyObjectByType<TimeLoopManager>();
        recorder = FindAnyObjectByType<PlayerTimelineRecorder>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        player = FindAnyObjectByType<PlayerMovement>();
        joystick = FindAnyObjectByType<VirtualJoystick>();
        recorder.RecordingCompleted += Remember;
    }

    private void Remember(PlayerTimeline timeline)
    {
        var copy = new PlayerTimeline(timeline.PoseCapacity, timeline.ActionCapacity);
        copy.CopyFrom(timeline);
        expected[timeline.SourceLoop] = copy;
    }

    private void LateUpdate()
    {
        if (!monitor || ghosts == null) return;
        int actual = FindObjectsByType<GhostPlayback>().Length;
        maximumActive = Mathf.Max(maximumActive, actual);
        if (actual > 3 || actual != ghosts.ActiveGhostCount)
            Check(false, "Scene and manager ghost count must agree and never exceed 3");
        for (int i = 0; i < ghosts.ActiveGhostCount; i++)
        {
            var ghost = ghosts.GetGhost(i);
            if (!expected.TryGetValue(ghost.Timeline.SourceLoop, out var saved))
                Check(false, "Unexpected source loop in Ghost history");
            if (ReferenceEquals(ghost.Timeline, recorder.CurrentRecording))
                Check(false, "Ghost aliases the writable recorder buffer");
            if (!loop.IsRunning) continue;
            float error = Vector3.Distance(ghost.transform.position, saved.Evaluate(loop.ElapsedTime).Position);
            worstError = Mathf.Max(worstError, error);
            if (error > 0.001f) Check(false, "Ghost diverged from independently saved recording");
        }
        frames++;
    }

    private void MoveJoystick(Vector2 direction)
    {
        Canvas.ForceUpdateCanvases();
        var point = joystick.joystickBG.TransformPoint(Vector3.Scale(direction,
            joystick.joystickBG.sizeDelta * 0.4f));
        var data = new PointerEventData(EventSystem.current)
        {
            position = RectTransformUtility.WorldToScreenPoint(null, point), pointerId = 42
        };
        joystick.OnPointerDown(data);
    }

    private void VerifyHistory(int currentLoop)
    {
        int count = Mathf.Min(currentLoop - 1, 3);
        Check(ghosts.ActiveGhostCount == count, "Loop " + currentLoop + " active Ghost count=" + count);
        for (int i = 0; i < count; i++)
        {
            int source = currentLoop - count + i;
            var ghost = ghosts.GetGhost(i);
            var actual = ghost.Timeline;
            var saved = expected[source];
            bool identical = actual.SourceLoop == source && actual.PoseCount == saved.PoseCount;
            for (int j = 0; identical && j < actual.PoseCount; j++)
                identical = actual.GetPose(j).Equals(saved.GetPose(j));
            Check(identical, "Loop " + currentLoop + " preserves every sample from Loop " + source);
            Check(ghost.GetComponent<Health>() == null && ghost.GetComponent<Collider2D>() == null &&
                ghost.GetComponent<PlayerMovement>() == null && ghost.GetComponent<PlayerTimelineRecorder>() == null &&
                !ghost.CompareTag("Player"), "Ghost " + source + " excludes damage/input/recording components and Player tag");
        }
    }

    private IEnumerator FreezeCheck(bool gameOver)
    {
        float elapsed = loop.ElapsedTime;
        int loopNumber = loop.loopIndex, samples = recorder.CurrentRecording.PoseCount;
        var poses = new Vector3[ghosts.ActiveGhostCount];
        var timelines = new PlayerTimeline[ghosts.ActiveGhostCount];
        for (int i = 0; i < poses.Length; i++)
        {
            poses[i] = ghosts.GetGhost(i).transform.position;
            timelines[i] = ghosts.GetGhost(i).Timeline;
        }
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.5f);
        bool same = elapsed == loop.ElapsedTime && loopNumber == loop.loopIndex &&
            samples == recorder.CurrentRecording.PoseCount && ghosts.ActiveGhostCount == poses.Length;
        for (int i = 0; i < poses.Length; i++)
            same &= poses[i] == ghosts.GetGhost(i).transform.position &&
                ReferenceEquals(timelines[i], ghosts.GetGhost(i).Timeline);
        Check(same, (gameOver ? "Game Over" : "Pause") + " freezes all Ghosts, history, recorder and clock");
        VerifyHistory(loopNumber);
        if (!gameOver) Time.timeScale = 1;
    }

    private IEnumerator Start()
    {
        Bind();
        Check(loop.loopIndex == 1 && ghosts.ActiveGhostCount == 0 && ghosts.MaxGhosts == 3,
            "Fresh run starts with zero Ghosts and maxGhosts=3");
        Check(loop.LoopDuration == 20f, "Tests use real 20-second loops at timeScale 1");
        var initial = player.transform.position;
        var enemy = FindAnyObjectByType<EnemyFollow>();
        var enemyStart = enemy.transform.position;
        var enemyHealth = enemy.GetComponent<Health>();
        var hp = player.GetComponent<Health>();
        enemy.enabled = false;
        monitor = true;
        var directions = new[] { Vector2.up, Vector2.left, Vector2.down, Vector2.right };
        var firstSlots = new HashSet<GhostPlayback>();
        for (int source = 1; source <= 4; source++)
        {
            MoveJoystick(directions[source - 1]);
            yield return new WaitForSeconds(0.45f);
            joystick.OnPointerUp(new PointerEventData(EventSystem.current));
            Check(Vector3.Dot(player.transform.position - initial, directions[source - 1]) > 1f,
                "Synthetic joystick drives live Player independently in Loop " + source);
            if (source == 3) yield return FreezeCheck(false);
            if (source == 1)
            {
                hp.TakeDamage(10);
                enemyHealth.TakeDamage(10000);
                Check(!enemy.gameObject.activeSelf, "Enemy death deactivates before rewind");
            }
            float deadline = Time.realtimeSinceStartup + 45f;
            while (loop.loopIndex == source && Time.realtimeSinceStartup < deadline) yield return null;
            Check(loop.loopIndex == source + 1, "Natural rewind " + source + " completed");
            Check(Vector3.Distance(player.transform.position, initial) < 0.05f && hp.currentHealth == hp.maxHealth,
                "Player reset and HP preserved after rewind " + source);
            Check(enemy.gameObject.activeSelf && enemyHealth.currentHealth == enemyHealth.maxHealth &&
                Vector3.Distance(enemy.transform.position, enemyStart) < 0.05f,
                "Enemy restoration after rewind " + source);
            VerifyHistory(source + 1);
            for (int i = 0; i < ghosts.ActiveGhostCount; i++)
            {
                var ghost = ghosts.GetGhost(i);
                Check(Vector3.Distance(ghost.transform.position, ghost.Timeline.GetPose(0).Position) < 0.01f,
                    "Ghost " + ghost.Timeline.SourceLoop + " restarts at time zero");
                if (source <= 3) firstSlots.Add(ghost);
                else Check(firstSlots.Contains(ghost), "Full history reuses an existing slot");
            }
        }

        Check(ghosts.GetGhost(0).Timeline.SourceLoop == 2 && ghosts.GetGhost(2).Timeline.SourceLoop == 4,
            "Loop 5 evicts Loop 1 and retains exactly Loops 2,3,4");
        yield return new WaitForSeconds(1f);
        var targetGhost = ghosts.GetGhost(0);
        var body = enemy.GetComponent<Rigidbody2D>();
        body.position = targetGhost.transform.position;
        Physics2D.SyncTransforms();
        float beforeHp = hp.currentHealth;
        yield return new WaitForSeconds(0.25f);
        Check(targetGhost != null && targetGhost.gameObject.activeInHierarchy && hp.currentHealth == beforeHp,
            "Enemy overlapping Ghost neither damages Player nor removes Ghost");
        body.position = (Vector2)player.transform.position + Vector2.right * 1.5f;
        Physics2D.SyncTransforms();
        player.GetComponent<PlayerAttack>().Attack();
        Check(enemyHealth.currentHealth < enemyHealth.maxHealth && hp.currentHealth == beforeHp,
            "Player attack still damages Enemy without self-damage");
        enemy.enabled = true;
        float oldDistance = Vector2.Distance(body.position, player.transform.position);
        yield return new WaitForSeconds(0.2f);
        Check(Vector2.Distance(body.position, player.transform.position) < oldDistance, "Enemy still follows live Player");
        body.position = (Vector2)player.transform.position + Vector2.right * 0.4f;
        enemy.GetComponent<EnemyContactDamage>().ResetDamageCooldown();
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.2f);
        Check(hp.currentHealth < beforeHp, "Enemy contact still damages live Player");
        hp.TakeDamage(10000);
        var game = FindAnyObjectByType<GameManager>();
        Check(game.IsGameOver && Time.timeScale == 0, "Game Over with three Ghosts");
        yield return FreezeCheck(true);
        Debug.Log("M1.2 HISTORY CHECKS PASSED; monitored frames=" + frames +
            "; max active=" + maximumActive + "; worst position error=" + worstError);
        // Leave a short real-time window for an MCP visual capture.
        yield return new WaitForSecondsRealtime(8f);

        monitor = false;
        var oldGhosts = FindObjectsByType<GhostPlayback>();
        var oldRecorder = recorder;
        var oldRecording = recorder.CurrentRecording;
        recorder.RecordingCompleted -= Remember;
        expected.Clear();
        game.Restart();
        yield return null;
        yield return null;
        Bind();
        bool destroyed = true;
        foreach (var ghost in oldGhosts) destroyed &= ghost == null;
        Check(destroyed && oldRecorder == null && ghosts.ActiveGhostCount == 0 &&
            FindObjectsByType<GhostPlayback>().Length == 0,
            "Restart destroys every old Ghost and recorder");
        Check(loop.loopIndex == 1 && recorder.CurrentRecording.SourceLoop == 1 &&
            !ReferenceEquals(oldRecording, recorder.CurrentRecording) && Time.timeScale == 1,
            "Restart starts fresh recording and Loop 1, unpaused");
        enemy = FindAnyObjectByType<EnemyFollow>();
        enemy.enabled = false;
        monitor = true;
        MoveJoystick(Vector2.one.normalized);
        yield return new WaitForSeconds(0.3f);
        joystick.ResetInput();
        float restartDeadline = Time.realtimeSinceStartup + 45f;
        while (loop.loopIndex == 1 && Time.realtimeSinceStartup < restartDeadline) yield return null;
        Check(loop.loopIndex == 2 && ghosts.ActiveGhostCount == 1,
            "First rewind after Restart creates exactly one fresh Ghost");
        VerifyHistory(2);
        monitor = false;
        recorder.RecordingCompleted -= Remember;
        var activeGhost = ghosts.ActiveGhost;
        FindAnyObjectByType<GameManager>().Restart();
        yield return null;
        yield return null;
        Bind();
        Check(activeGhost == null && ghosts.ActiveGhostCount == 0 && loop.loopIndex == 1,
            "Restart during active gameplay also clears Ghost history");
        recorder.RecordingCompleted -= Remember;
        FindAnyObjectByType<GameManager>().MainMenu();
        yield return null;
        yield return null;
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenuScene" &&
            FindObjectsByType<GhostPlayback>().Length == 0,
            "Main Menu navigation leaves no Ghosts");
        Application.runInBackground = originalBackground;
        Debug.Log("M1.2 ALL CHECKS PASSED; assertions=" + assertions + "; max active=" + maximumActive +
            "; frames=" + frames + "; worst position error=" + worstError);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        if (recorder != null) recorder.RecordingCompleted -= Remember;
        Application.runInBackground = originalBackground;
    }
}
#endif
