#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class TimedMechanismPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager rooms;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private TimedDoorMechanism mechanism;
    private Door door;
    private PressureSwitch trigger;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M6.2 FAIL: " + message);
        checks++;
        Debug.Log("M6.2 PASS: " + message);
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
        loop = FindAnyObjectByType<TimeLoopManager>();
        Enter(2); // Echo Chamber
        yield return null;
        mechanism = System.Array.Find(FindObjectsByType<TimedDoorMechanism>(FindObjectsInactive.Include), x => x.name == "Timed Echo Gate");
        Debug.Log("M6.2 probe: entered=" + rooms.CurrentIndex + ", mechanism=" + (mechanism == null ? "null" : mechanism.name));
        door = mechanism.GetComponent<Door>();
        var triggerField = typeof(TimedDoorMechanism).GetField("trigger", BindingFlags.NonPublic | BindingFlags.Instance);
        trigger = (PressureSwitch)triggerField.GetValue(mechanism);
        Check(mechanism != null && door != null && trigger != null && !mechanism.IsActive && door.IsLocked,
            "Timed Echo Gate starts closed with a referenced replayable trigger");
        Vector2 origin = player.transform.position;
        Place(player, trigger.transform.position);
        player.SetMoveDirection(Vector2.zero);
        // Let PlayerTimelineRecorder sample the deliberate interaction pose.
        yield return new WaitForSeconds(.35f);
        Check(trigger.IsActive && mechanism.IsActive && door.IsOpen && mechanism.RemainingTime > 0f,
            "Player activation opens the gate and starts a visible timed window");
        Place(player, origin);
        Rewind();
        yield return null;
        Check(!mechanism.IsActive && door.IsLocked && !trigger.IsActive,
            "Rewind closes the timed gate and clears trigger state");
        float deadline = Time.realtimeSinceStartup + 5f;
        while (!mechanism.IsActive && Time.realtimeSinceStartup < deadline) yield return null;
        Check(mechanism.IsActive && door.IsOpen,
            "Ghost replay of the recorded trigger opens the timed gate");
        yield return new WaitForSeconds(mechanism.OpenDuration + .25f);
        Check(!mechanism.IsActive && door.IsLocked,
            "Timed gate closes after the window expires");
        Debug.Log("M6.2 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
