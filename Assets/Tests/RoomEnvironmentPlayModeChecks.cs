#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]
public sealed class RoomEnvironmentPlayModeChecks : MonoBehaviour
{
    private RoomManager manager;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private int checks;
    private bool background;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M4.3 FAIL: " + message);
        checks++;
        Debug.Log("M4.3 PASS: " + message);
    }

    private void Place(Vector2 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        player.GetComponent<Rigidbody2D>().position = position;
        Physics2D.SyncTransforms();
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        background = Application.runInBackground;
        Application.runInBackground = true;
        yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        player = manager.Player;
        loop = FindAnyObjectByType<TimeLoopManager>();
        player.GetComponent<Health>().RestoreToFullHealth();
        Time.timeScale = 1f;
        foreach (var enemy in FindObjectsByType<EnemyFollow>(FindObjectsInactive.Include))
        {
            enemy.enabled = false;
            var contact = enemy.GetComponent<EnemyContactDamage>();
            if (contact != null) contact.enabled = false;
        }
        var rooms = FindObjectsByType<Room>(FindObjectsInactive.Include);
        Check(rooms.Length == 8, "Eight rooms remain in environment scene");
        foreach (var room in rooms)
        {
            var boundary = room.GetComponent<RoomBoundary>();
            Check(boundary != null && boundary.IsConfigured, "Boundary configured for " + room.DisplayName);
            var shell = room.Content.transform.Find("Solid Room Boundaries");
            var colliders = shell.GetComponentsInChildren<BoxCollider2D>(true);
            Check(colliders.Length == 5, "Five boundary segments for " + room.DisplayName);
            Check(room.Content.GetComponentsInChildren<Collider2D>(true).Length >= 6, "Room collision content exists for " + room.DisplayName);
        }

        for (int index = 0; index < 8; index++)
        {
            var enter = typeof(RoomManager).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enter.Invoke(manager, new object[] { index });
            yield return null;
            var room = manager.CurrentRoom;
            Vector2 center = room.transform.position;
            Check(manager.CurrentIndex == index && room.Content.activeInHierarchy, "Environment enters room " + (index + 1));

            Place(center + new Vector2(-7.55f, 3.0f));
            player.SetMoveDirection(Vector2.left);
            yield return new WaitForSeconds(0.35f);
            player.SetMoveDirection(Vector2.zero);
            Check(player.transform.position.x > center.x - 7.9f, "West wall contains movement " + (index + 1));

            Place(center + new Vector2(7.0f, 3.0f));
            player.SetMoveDirection(Vector2.right);
            player.GetComponent<PlayerDash>().Dash();
            player.SetMoveDirection(Vector2.zero);
            yield return new WaitForFixedUpdate();
            Check(player.transform.position.x < center.x + 7.9f, "East wall blocks dash outside exit gap " + (index + 1));

            var wall = room.Content.transform.Find("Solid Room Boundaries/East Upper Wall").GetComponent<BoxCollider2D>();
            var probe = Physics2D.OverlapBox(center + new Vector2(8.15f, 3.0f), new Vector2(.2f, .2f), 0f);
            Check(probe == wall, "East wall collider is present " + (index + 1));
            var gapProbe = Physics2D.OverlapBox(center + new Vector2(8.15f, 0f), new Vector2(.2f, .2f), 0f);
            Check(gapProbe == null || gapProbe.isTrigger, "Exit gap remains open " + (index + 1));

            float before = loop.remainingTime;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.05f);
            Check(Mathf.Approximately(loop.remainingTime, before), "Boundary test pause freezes loop " + (index + 1));
            Time.timeScale = 1;
        }

        Check(Time.timeScale == 1f && SceneManager.GetActiveScene().name == "GameScene", "Environment checks remain in GameScene");
        FindAnyObjectByType<GameManager>().Restart();
        yield return null; yield return null;
        Check(FindAnyObjectByType<RoomManager>().CurrentIndex == 0, "Restart restores first room boundary");
        Check(FindAnyObjectByType<RoomManager>().Player.GetComponent<PlayerDash>().CanDash, "Restart restores dash state");
        Debug.Log("M4.3 ALL CHECKS PASSED; checks=" + checks);
        FindAnyObjectByType<GameManager>().MainMenu();
        yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenuScene", "Main Menu still exits environment run");
        Destroy(gameObject);
    }

    private void OnDestroy() { Application.runInBackground = background; Time.timeScale = 1f; }
}
#endif
