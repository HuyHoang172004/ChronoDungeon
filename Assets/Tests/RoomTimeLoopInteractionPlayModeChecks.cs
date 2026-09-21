#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public sealed class RoomTimeLoopInteractionPlayModeChecks : MonoBehaviour
{
    private RoomManager manager;
    private TimeLoopManager loop;
    private PlayerMovement player;
    private PlayerTimelineRecorder recorder;
    private TemporalGhostManager ghosts;
    private int checks;
    private bool background;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M4.4 FAIL: " + message);
        checks++;
        Debug.Log("M4.4 PASS: " + message);
    }

    private void Place(Vector2 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        player.GetComponent<Rigidbody2D>().position = position;
        Physics2D.SyncTransforms();
    }

    private void Enter(int index)
    {
        typeof(RoomManager).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(manager, new object[] { index });
    }

    private void RewindNow()
    {
        typeof(TimeLoopManager).GetMethod("Rewind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(loop, null);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        background = Application.runInBackground;
        Application.runInBackground = true;
        yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        player = manager.Player;
        recorder = player.GetComponent<PlayerTimelineRecorder>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        foreach (var enemy in FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include))
        {
            enemy.enabled = false;
            var contact = enemy.GetComponent<EnemyContactDamage>();
            if (contact != null) contact.enabled = false;
        }
        player.GetComponent<Health>().RestoreToFullHealth();
        Time.timeScale = 1f;

        Enter(1);
        yield return null;
        var combat = manager.CurrentRoom;
        var guard = combat.Content.GetComponentsInChildren<EnemyFollow>(true)[0];
        var guardHealth = guard.GetComponent<Health>();
        Vector2 combatSpawn = combat.SpawnPoint.position;
        Check(loop.loopIndex == 1 && ghosts.ActiveGhostCount == 0 && recorder.CurrentRecording.SourceLoop == 1,
            "Combat encounter starts at local Loop 1 with empty Ghost/timeline state");
        Place(combatSpawn + Vector2.right * 1.2f);
        player.SetMoveDirection(Vector2.right);
        player.GetComponent<PlayerAttack>().Attack();
        player.SetMoveDirection(Vector2.zero);
        Check(recorder.CurrentRecording.ActionCount > 0, "Combat action is recorded in current room timeline");
        guardHealth.TakeDamage(20f);
        float damagedHealth = guardHealth.currentHealth;
        RewindNow();
        yield return null;
        Check(loop.loopIndex == 2 && ghosts.ActiveGhostCount == 1, "Room-local rewind increments loop and spawns one Ghost");
        Check(ghosts.ActiveGhost.Timeline.SourceLoop == 1, "Ghost keeps the completed combat room timeline");
        Check(Vector2.Distance(player.transform.position, combatSpawn) < 0.05f, "Player returns to current room spawn on rewind");
        Check(Mathf.Approximately(guardHealth.currentHealth, guardHealth.maxHealth) && guardHealth.currentHealth > damagedHealth,
            "Current room enemy restores health on rewind");
        Check(recorder.CurrentRecording.SourceLoop == 2, "Recorder begins the next loop after rewind");
        Check(ghosts.ActiveGhost.transform.parent == null && ghosts.ActiveGhost.GetComponent<Health>() == null,
            "Ghost remains an independent playback actor");

        Enter(2);
        yield return null;
        var puzzle = manager.CurrentRoom;
        Check(manager.CurrentIndex == 2 && puzzle.Content.activeInHierarchy && !combat.Content.activeInHierarchy,
            "Room transition activates puzzle and deactivates combat content");
        Check(loop.loopIndex == 1 && ghosts.ActiveGhostCount == 0 && recorder.CurrentRecording.SourceLoop == 1,
            "Room transition clears Ghosts and starts a fresh local timeline");
        Check(Vector2.Distance(player.transform.position, puzzle.SpawnPoint.position) < 0.05f,
            "Room transition uses the new room spawn");
        Check(!guard.gameObject.activeInHierarchy || !guardHealth.IsDead,
            "Old room enemy cannot remain as a live active actor in new room");

        var switches = puzzle.Content.GetComponentsInChildren<PressureSwitch>(true);
        var first = switches[0].name == "Pressure Switch" ? switches[0] : switches[1];
        var second = switches[0] == first ? switches[1] : switches[0];
        Place(first.transform.position);
        yield return new WaitForSeconds(.15f);
        Check(first.IsActive && !second.IsActive && ghosts.ActiveGhostCount == 0,
            "New room starts without stale Ghost pressure");
        RewindNow();
        yield return null;
        Check(loop.loopIndex == 2 && ghosts.ActiveGhostCount == 1 && Vector2.Distance(player.transform.position, puzzle.SpawnPoint.position) < .05f,
            "Puzzle rewind creates only the new room Ghost and restores its spawn");
        Place(second.transform.position);
        yield return new WaitForSeconds(.3f);
        Check(first.IsActive && second.IsActive && puzzle.State == RoomState.Completed,
            "New room Ghost cooperates with current Player after local rewind");
        Check(puzzle.ExitDoor.IsOpen, "Puzzle room exit responds to current room Ghost state");
        Check(manager.CurrentIndex == 2 && manager.CurrentRoom == puzzle,
            "Puzzle room remains current until its physical exit is reached");
        Time.timeScale = 0f;
        float paused = loop.remainingTime;
        yield return new WaitForSecondsRealtime(.1f);
        Check(Mathf.Approximately(loop.remainingTime, paused) && ghosts.ActiveGhostCount == 1,
            "Pause freezes local timer and Ghost state");
        Time.timeScale = 1f;
        FindAnyObjectByType<GameManager>().Restart();
        yield return null; yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        Check(manager.CurrentIndex == 0 && FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 0,
            "Restart clears room progression and all Ghost history");
        FindAnyObjectByType<GameManager>().MainMenu();
        yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenuScene", "Main Menu exits the local time-loop run");
        Debug.Log("M4.4 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() { Application.runInBackground = background; Time.timeScale = 1f; }
}
#endif
