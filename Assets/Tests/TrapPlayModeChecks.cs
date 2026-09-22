#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class TrapPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager rooms;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private TemporalSpikeTrap trap;
    private Health playerHealth;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M6.4 FAIL: " + message);
        checks++;
        Debug.Log("M6.4 PASS: " + message);
    }

    private void Place(Component actor, Vector2 position)
    {
        actor.transform.position = position;
        var body = actor.GetComponent<Rigidbody2D>();
        if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private void Enter(int index) => typeof(RoomManager).GetMethod("Enter", BindingFlags.NonPublic | BindingFlags.Instance)
        .Invoke(rooms, new object[] { index });

    private void Rewind() => typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance)
        .Invoke(loop, null);

    private IEnumerator Start()
    {
        yield return null;
        rooms = FindAnyObjectByType<RoomManager>();
        player = rooms.Player;
        playerHealth = player.GetComponent<Health>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        Enter(3); // Pendulum Gallery
        yield return null;
        trap = FindAnyObjectByType<TemporalSpikeTrap>();
        Check(trap != null && !trap.IsWindingUp && !trap.IsActive,
            "Pendulum Gallery trap starts idle and is room-configured");

        playerHealth.RestoreToFullHealth();
        Place(player, trap.transform.position);
        player.SetMoveDirection(Vector2.zero);
        float before = playerHealth.currentHealth;
        yield return new WaitForSeconds(.15f);
        Check(trap.IsWindingUp && playerHealth.currentHealth == before,
            "Player contact starts a readable warning without immediate damage");
        yield return new WaitForSeconds(trap.WarningDuration + .1f);
        Check(trap.IsActive && playerHealth.currentHealth == before - 20f,
            "Spike pulse damages Player after the warning window");

        Rewind();
        yield return null;
        Check(!trap.IsWindingUp && !trap.IsActive && playerHealth.currentHealth == playerHealth.maxHealth,
            "Rewind clears trap state and restores Player health");
        Debug.Log("M6.4 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
