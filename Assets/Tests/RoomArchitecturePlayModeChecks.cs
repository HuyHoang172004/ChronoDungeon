#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public sealed class RoomArchitecturePlayModeChecks : MonoBehaviour
{
    private int checks;
    private bool background;
    private PlayerMovement player;
    private RoomManager rooms;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private int completions;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M4.1 FAIL: " + message);
        checks++;
        Debug.Log("M4.1 PASS: " + message);
    }

    private void Place(Vector2 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        player.GetComponent<Rigidbody2D>().position = position;
        Physics2D.SyncTransforms();
    }

    private IEnumerator NextLoop()
    {
        int index = loop.loopIndex;
        float deadline = Time.realtimeSinceStartup + 30f;
        while (loop.loopIndex == index && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == index + 1, "Natural 20-second rewind completes");
        yield return null;
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        yield return null;
        rooms = FindAnyObjectByType<RoomManager>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        player = rooms.Player;
        var first = rooms.CurrentRoom;
        var enemy = FindAnyObjectByType<EnemyFollow>();
        enemy.enabled = false;
        enemy.GetComponent<EnemyContactDamage>().enabled = false;
        var enemyHealth = enemy.GetComponent<Health>();
        var playerHealth = player.GetComponent<Health>();
        first.StateChanged += r => { if (r.State == RoomState.Completed) completions++; };
        Check(rooms.CurrentIndex == 0 && first.State == RoomState.Active && first.ExitDoor.IsLocked, "Combat room starts active with exit locked");
        Check(FindAnyObjectByType<PressureSwitch>() == null, "Future puzzle content is inactive");
        Check(!rooms.TryAdvance(first, player), "Uncleared room rejects transition");
        var enemyBody = enemy.GetComponent<Rigidbody2D>();
        enemy.transform.position = new Vector2(1.2f, 0); enemyBody.position = enemy.transform.position;
        Place(Vector2.zero); player.SetMoveDirection(Vector2.right); player.SetMoveDirection(Vector2.zero);
        Physics2D.SyncTransforms();
        player.GetComponent<PlayerAttack>().Attack();
        Check(enemyHealth.currentHealth == enemyHealth.maxHealth - 25f, "Directional attack still damages enemy");
        enemyHealth.TakeDamage(enemyHealth.maxHealth);
        Check(first.State == RoomState.Completed && first.ExitDoor.IsOpen && completions == 1, "Enemy death completes once and unlocks exit");
        first.Evaluate();
        Check(completions == 1, "Repeated evaluation cannot duplicate completion");
        Check(!rooms.TryAdvance(first, null), "Non-player cannot advance");
        Time.timeScale = 0;
        float pausedTime = loop.remainingTime;
        Check(!rooms.TryAdvance(first, player), "Pause blocks transitions");
        yield return new WaitForSecondsRealtime(0.15f);
        Check(loop.remainingTime == pausedTime, "Pause freezes timer");
        Time.timeScale = 1;
        yield return NextLoop();
        Check(first.State == RoomState.Active && first.ExitDoor.IsLocked && !enemyHealth.IsDead && enemy.gameObject.activeInHierarchy, "Rewind restores enemy and locks completed room again");
        Check(ghosts.ActiveGhostCount == 1, "Combat recording creates Ghost");
        enemyHealth.TakeDamage(enemyHealth.maxHealth);
        playerHealth.TakeDamage(10);
        float hp = playerHealth.currentHealth;
        Place(new Vector2(6.8f,0));
        yield return new WaitForFixedUpdate(); yield return null;
        Check(rooms.CurrentIndex == 1 && first.State == RoomState.Exited, "Player exit trigger advances exactly one room");
        var second = rooms.CurrentRoom;
        Check(!enemy.gameObject.activeInHierarchy && second.State == RoomState.Active, "Old content sleeps and new room starts");
        Check(Vector2.Distance(player.transform.position, second.SpawnPoint.position) < 0.01f && playerHealth.currentHealth == hp, "Transition uses room spawn and preserves HP");
        Check(loop.loopIndex == 1 && loop.ElapsedTime < 1 && ghosts.ActiveGhostCount == 0, "Transition starts fresh timer and clears old Ghost history");
        Check(!rooms.TryAdvance(first, player), "Stale room exit cannot advance");
        var recorder = player.GetComponent<PlayerTimelineRecorder>();
        Check(recorder.CurrentRecording.SourceLoop == 1, "Recorder starts new room timeline");
        var switches = FindObjectsByType<PressureSwitch>();
        PressureSwitch a = switches[0].name == "Pressure Switch" ? switches[0] : switches[1];
        PressureSwitch b = switches[0] == a ? switches[1] : switches[0];
        Place(a.transform.position);
        yield return null; yield return null;
        Check(a.IsActive && !b.IsActive && second.ExitDoor.IsLocked, "One Player cannot clear dual-switch room alone");
        yield return NextLoop();
        Check(Vector2.Distance(player.transform.position, second.SpawnPoint.position) < 0.01f && !enemy.gameObject.activeInHierarchy, "Room-local rewind uses new spawn and does not revive old room");
        Place(b.transform.position);
        yield return new WaitForSeconds(0.3f);
        Check(ghosts.ActiveGhostCount == 1 && a.IsActive && b.IsActive, "New room Ghost replays plate A while Player holds B");
        Check(second.State == RoomState.Completed && second.ExitDoor.IsOpen, "Temporal puzzle completes room and opens its exit");
        Place(new Vector2(4,0)); yield return null; yield return null;
        Check(second.ExitDoor.IsOpen, "Room clear latches until rewind so Player can reach exit");
        Place(new Vector2(6.8f,0)); yield return new WaitForFixedUpdate(); yield return null;
        Check(rooms.IsComplete && !loop.IsRunning, "Last exit signals completion and stops encounter timer");
        Check(!rooms.TryAdvance(second, player), "Final completion cannot fire twice");
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        rooms = FindAnyObjectByType<RoomManager>();
        Check(rooms.CurrentIndex == 0 && !rooms.IsComplete && rooms.CurrentRoom.State == RoomState.Active && FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 0, "Restart restores first room and clears progression/Ghosts");
        rooms.Player.GetComponent<Health>().TakeDamage(10000);
        Check(FindAnyObjectByType<GameManager>().IsGameOver && Time.timeScale == 0, "Player death still triggers Game Over");
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        Check(!FindAnyObjectByType<GameManager>().IsGameOver && Time.timeScale == 1, "Restart after Game Over restores gameplay");
        FindAnyObjectByType<GameManager>().MainMenu(); yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenuScene" && FindAnyObjectByType<RoomManager>() == null, "Main Menu unloads room progression");
        Debug.Log("M4.1 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() { Application.runInBackground = background; Time.timeScale = 1; }
}
#endif
