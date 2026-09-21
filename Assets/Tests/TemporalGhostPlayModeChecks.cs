#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// Editor-only integration probe. Add via MCP in Play Mode; never saved in GameScene.
[DefaultExecutionOrder(10000)]
public sealed class TemporalGhostPlayModeChecks : MonoBehaviour
{
    private TimeLoopManager loop;
    private PlayerTimelineRecorder recorder;
    private TemporalGhostManager ghosts;
    private PlayerMovement player;
    private int comparisons;
    private bool comparePlayback;
    private float worstError;

    private void Check(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException("M1.1 FAIL: " + message);
        Debug.Log("M1.1 PASS: " + message);
    }

    private void LateUpdate()
    {
        if (!comparePlayback || ghosts.ActiveGhost == null || !loop.IsRunning) return;
        var ghost = ghosts.ActiveGhost;
        float error = Vector3.Distance(ghost.transform.position, ghost.Timeline.Evaluate(loop.ElapsedTime).Position);
        worstError = Mathf.Max(worstError, error);
        comparisons++;
    }

    private IEnumerator Start()
    {
        loop = FindAnyObjectByType<TimeLoopManager>();
        recorder = FindAnyObjectByType<PlayerTimelineRecorder>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        player = FindAnyObjectByType<PlayerMovement>();
        var enemy = FindAnyObjectByType<EnemyFollow>();
        var enemyHealth = enemy.GetComponent<Health>();
        var playerHealth = player.GetComponent<Health>();
        var game = FindAnyObjectByType<GameManager>();
        var initialPlayer = player.transform.position;
        var initialEnemy = enemy.transform.position;
        enemy.enabled = false; // Keep the recording path safe; restored below.
        Check(recorder.CurrentRecording != null && ghosts.ActiveGhost == null, "Loop 1 records with no Ghost");

        var data = new PlayerTimeline(4);
        data.AddPose(new PlayerTimeline.Pose { Time = 0, Position = Vector3.zero, Rotation = Quaternion.identity });
        data.AddPose(new PlayerTimeline.Pose { Time = 2, Position = Vector3.right * 4, Rotation = Quaternion.Euler(0, 0, 90), FlipX = true });
        Check(Vector3.Distance(data.Evaluate(1).Position, Vector3.right * 2) < 0.001f &&
            Quaternion.Angle(data.Evaluate(1).Rotation, Quaternion.Euler(0, 0, 45)) < 0.01f,
            "Midpoint position and rotation interpolation");
        Check(data.Evaluate(100).Position == Vector3.right * 4 && data.Evaluate(100).FlipX,
            "Playback holds final pose after last sample");

        player.SetMoveDirection(Vector2.up);
        yield return new WaitForSeconds(0.5f);
        player.SetMoveDirection(Vector2.right);
        yield return new WaitForSeconds(0.5f);
        player.SetMoveDirection(Vector2.zero);
        var endpoint = player.transform.position;
        Check(Vector3.Distance(endpoint, initialPlayer) > 1f, "Player movement records a nontrivial path");
        int count = recorder.CurrentRecording.PoseCount;
        float elapsed = loop.ElapsedTime;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.4f);
        Check(loop.ElapsedTime == elapsed && recorder.CurrentRecording.PoseCount == count, "Pause freezes timer and recording");
        Time.timeScale = 1;
        playerHealth.TakeDamage(10);
        enemyHealth.TakeDamage(10000);
        Check(!enemy.gameObject.activeSelf, "Enemy death deactivates actor");
        int sourceLoop = loop.loopIndex;
        while (loop.loopIndex == sourceLoop) yield return null;
        Check(loop.loopIndex == sourceLoop + 1 && gameObject.scene.name == "GameScene", "Natural 20-second rewind without scene reload");
        Check(Vector3.Distance(player.transform.position, initialPlayer) < 0.05f && playerHealth.currentHealth == playerHealth.maxHealth,
            "Rewind restores Player pose and HP");
        Check(enemy.gameObject.activeSelf && enemyHealth.currentHealth == enemyHealth.maxHealth &&
            Vector3.Distance(enemy.transform.position, initialEnemy) < 0.05f, "Rewind restores dead Enemy pose and HP");
        var ghost = ghosts.ActiveGhost;
        Check(ghost != null && ghost.Timeline.SourceLoop == sourceLoop, "Loop 2 Ghost replays Loop 1 recording");
        Check(Vector3.Distance(ghost.Timeline.GetPose(ghost.Timeline.PoseCount - 1).Position, endpoint) < 0.05f,
            "Final pose captured before rewind teleport");
        Check(ghost.GetComponent<Health>() == null && ghost.GetComponent<Collider2D>() == null &&
            ghost.GetComponent<PlayerMovement>() == null && ghost.GetComponent<PlayerTimelineRecorder>() == null,
            "Ghost has no damage, collision, input or recorder components");
        Check(ghost.GetComponent<SpriteRenderer>().sprite != null && ghost.GetComponent<SpriteRenderer>().color.a < 1,
            "Ghost has a translucent visual");
        Check(ghost.Timeline.PoseCount > 100 && ghost.Timeline.PoseCount <= 403,
            "Recording sample count is bounded at 20 seconds");
        comparePlayback = true;
        player.SetMoveDirection(Vector2.left);
        yield return new WaitForSeconds(0.6f);
        player.SetMoveDirection(Vector2.zero);
        Check(Vector3.Distance(player.transform.position, ghost.transform.position) > 1f,
            "Live Player moves independently while Ghost replays");
        Time.timeScale = 0;
        var pausedPose = ghost.transform.position;
        elapsed = loop.ElapsedTime;
        yield return new WaitForSecondsRealtime(0.4f);
        Check(ghost.transform.position == pausedPose && loop.ElapsedTime == elapsed, "Pause freezes Ghost and shared clock");
        Time.timeScale = 1;
        yield return new WaitForSeconds(1.5f);
        Check(comparisons > 5 && worstError < 0.001f, "Ghost matches timeline each frame; worst error=" + worstError);
        Check(Vector3.Distance(ghost.transform.position, endpoint) < 0.05f, "Ghost holds recorded stationary endpoint");
        comparePlayback = false;

        // Restore enemy behavior and verify existing combat against the live Player.
        enemy.enabled = true;
        enemy.GetComponent<Rigidbody2D>().position = (Vector2)player.transform.position + Vector2.right;
        Physics2D.SyncTransforms();
        player.GetComponent<PlayerAttack>().Attack();
        Check(enemyHealth.currentHealth < enemyHealth.maxHealth && playerHealth.currentHealth == playerHealth.maxHealth,
            "Player attack damages Enemy without self-damage");
        enemy.GetComponent<Rigidbody2D>().position = (Vector2)player.transform.position + Vector2.right * 0.4f;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.2f);
        Check(playerHealth.currentHealth < playerHealth.maxHealth, "Enemy contact damage still works");
        playerHealth.TakeDamage(10000);
        Check(game.IsGameOver && Time.timeScale == 0, "Player death triggers Game Over");
        elapsed = loop.ElapsedTime;
        pausedPose = ghost.transform.position;
        count = recorder.CurrentRecording.PoseCount;
        yield return new WaitForSecondsRealtime(0.4f);
        Check(elapsed == loop.ElapsedTime && pausedPose == ghost.transform.position && count == recorder.CurrentRecording.PoseCount,
            "Game Over freezes timer, Ghost and recording");
        Debug.Log("M1.1 AUTOMATED PLAY MODE CHECKS PASSED; frame comparisons=" + comparisons);
    }
}
#endif
