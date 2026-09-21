#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public sealed class DungeonLayoutPlayModeChecks : MonoBehaviour
{
    private RoomManager manager;
    private TimeLoopManager loop;
    private PlayerMovement player;
    private TemporalGhostManager ghosts;
    private int checks;
    private bool background;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M4.2 FAIL: " + message);
        checks++;
        Debug.Log("M4.2 PASS: " + message);
    }

    private void Place(Vector2 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        player.GetComponent<Rigidbody2D>().position = position;
        Physics2D.SyncTransforms();
    }

    private IEnumerator RewindNaturally()
    {
        int before = loop.loopIndex;
        float deadline = Time.realtimeSinceStartup + 30;
        while (loop.loopIndex == before && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == before + 1, "Natural rewind in " + manager.CurrentRoom.Role);
        yield return null;
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
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        var rooms = FindObjectsByType<Room>(FindObjectsInactive.Include);
        System.Array.Sort(rooms, (a, b) => a.Role.CompareTo(b.Role));
        Check(manager.RoomCount == 8 && rooms.Length == 8, "Exactly eight authored rooms");
        for (int i = 0; i < rooms.Length; i++)
        {
            Check((int)rooms[i].Role == i && rooms[i].IsConfigured, "Configured unique role " + rooms[i].Role);
            for (int j = 0; j < i; j++)
                Check(Vector2.Distance(rooms[i].CameraAnchor.position, rooms[j].CameraAnchor.position) >= 16, "Separate footprints " + j + " and " + i);
        }

        for (int i = 0; i < 8; i++)
        {
            Room room = rooms[i];
            Check(manager.CurrentIndex == i && manager.CurrentRoom == room, "Entered authored room " + (i + 1));
            Check(Vector2.Distance(player.transform.position, room.SpawnPoint.position) < 0.05f, "Player at room spawn " + (i + 1));
            Check(Vector2.Distance(Camera.main.transform.position, room.CameraAnchor.position) < 0.01f, "Camera frames room " + (i + 1));
            int active = 0;
            foreach (Room candidate in rooms) if (candidate.Content.activeInHierarchy) active++;
            Check(active == 1 && room.Content.activeInHierarchy, "Only current content active " + (i + 1));
            Check(ghosts.ActiveGhostCount == 0 && loop.loopIndex == 1, "Fresh temporal state on entry " + (i + 1));
            var enemies = room.Content.GetComponentsInChildren<EnemyFollow>(true);
            foreach (var enemy in enemies) { enemy.enabled = false; enemy.GetComponent<EnemyContactDamage>().enabled = false; }

            if (room.ClearCondition == RoomClearCondition.DefeatEnemies)
            {
                Check(room.ExitDoor.IsLocked && !manager.TryAdvance(room, player), "Combat exit denies early transition " + (i + 1));
                for (int j = 0; j < enemies.Length - 1; j++) enemies[j].GetComponent<Health>().TakeDamage(10000);
                Check(room.State == RoomState.Active, "All assigned enemies required " + (i + 1));
                enemies[enemies.Length - 1].GetComponent<Health>().TakeDamage(10000);
                Check(room.State == RoomState.Completed && room.ExitDoor.IsOpen, "Combat group clears " + (i + 1));
                if (i == 1)
                {
                    yield return RewindNaturally();
                    Check(room.State == RoomState.Active && room.ExitDoor.IsLocked, "Rewind relocks guard hall");
                    foreach (var enemy in enemies)
                    {
                        Check(!enemy.GetComponent<Health>().IsDead && enemy.gameObject.activeInHierarchy, "Guard restored at room-local rewind");
                        enemy.GetComponent<Health>().TakeDamage(10000);
                    }
                    Check(ghosts.ActiveGhostCount == 1, "Guard hall produces world-space Ghost");
                }
            }
            else if (room.ClearCondition == RoomClearCondition.SolvePuzzle)
            {
                var switches = room.Content.GetComponentsInChildren<PressureSwitch>();
                var a = switches[0].name == "Pressure Switch" ? switches[0] : switches[1];
                var b = switches[0] == a ? switches[1] : switches[0];
                Place(a.transform.position); yield return null; yield return null;
                Check(a.IsActive && !b.IsActive && room.State == RoomState.Active, "Translated puzzle still requires cooperation");
                yield return RewindNaturally();
                Check(Vector2.Distance(player.transform.position, room.SpawnPoint.position) < 0.05f, "Puzzle rewind returns to translated spawn");
                Place(b.transform.position); yield return new WaitForSeconds(0.3f);
                Check(a.IsActive && b.IsActive && ghosts.ActiveGhostCount == 1, "Ghost holds translated plate A while Player holds B");
                Check(room.State == RoomState.Completed && room.ExitDoor.IsOpen, "Translated puzzle unlocks room exit");
            }
            else Check(room.State == RoomState.Completed && room.ExitDoor.IsOpen, "Layout traversal room has open exit " + (i + 1));

            Time.timeScale = 0;
            Vector3 cameraPosition = Camera.main.transform.position;
            Check(!manager.TryAdvance(room, player), "Pause rejects progression " + (i + 1));
            yield return new WaitForSecondsRealtime(0.05f);
            Check(Camera.main.transform.position == cameraPosition, "Pause preserves room framing " + (i + 1));
            Time.timeScale = 1;
            Check(!manager.TryAdvance(room, null), "Non-player rejected " + (i + 1));
            var exit = room.Content.GetComponentInChildren<RoomExit>();
            float hp = player.GetComponent<Health>().currentHealth;
            Place(exit.transform.position);
            yield return new WaitForFixedUpdate(); yield return null;
            Check(player.GetComponent<Health>().currentHealth == hp, "Transition preserves HP " + (i + 1));
            if (i < 7)
            {
                Check(manager.CurrentIndex == i + 1 && room.State == RoomState.Exited, "Physics exit advances once " + (i + 1));
                Check(!manager.TryAdvance(room, player), "Old exit rejected " + (i + 1));
            }
        }
        Check(manager.IsComplete && !loop.IsRunning, "All eight rooms reach layout completion");
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        Check(manager.CurrentIndex == 0 && manager.CurrentRoom.Role == RoomRole.Start && !manager.IsComplete, "Restart returns to Threshold");
        Check(Vector2.Distance(Camera.main.transform.position, manager.CurrentRoom.CameraAnchor.position) < 0.01f, "Restart restores camera framing");
        Check(FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 0, "Restart leaves no Ghost history");
        manager.Player.GetComponent<Health>().TakeDamage(10000);
        Check(FindAnyObjectByType<GameManager>().IsGameOver && Time.timeScale == 0, "Game Over still works");
        FindAnyObjectByType<GameManager>().Restart(); yield return null; yield return null;
        Check(Time.timeScale == 1 && !FindAnyObjectByType<GameManager>().IsGameOver, "Restart after death restores run");
        FindAnyObjectByType<GameManager>().MainMenu(); yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenuScene" && FindAnyObjectByType<RoomCamera>() == null, "Main Menu unloads dungeon and room camera");
        Debug.Log("M4.2 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() { Application.runInBackground = background; Time.timeScale = 1; }
}
#endif
