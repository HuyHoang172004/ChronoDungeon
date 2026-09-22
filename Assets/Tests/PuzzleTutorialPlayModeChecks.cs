#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class PuzzleTutorialPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager rooms;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private PuzzleTutorialGuide guide;
    private PressureSwitch first;
    private PressureSwitch second;
    private DualPressureDoor puzzle;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M6.5 FAIL: " + message);
        checks++;
        Debug.Log("M6.5 PASS: " + message);
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
        guide = FindAnyObjectByType<PuzzleTutorialGuide>();
        first = guide.GetComponentInParent<Room>().Content.GetComponentInChildren<PressureSwitch>();
        var switches = guide.GetComponentInParent<Room>().Content.GetComponentsInChildren<PressureSwitch>();
        first = switches[0].transform.position.y > switches[1].transform.position.y ? switches[0] : switches[1];
        second = first == switches[0] ? switches[1] : switches[0];
        puzzle = FindAnyObjectByType<DualPressureDoor>();
        Check(guide != null && guide.CurrentMessage == "STEP ON A TO BEGIN" && !guide.IsShowingFailureHint,
            "Safe puzzle entry shows a short visual instruction");

        Vector2 origin = player.transform.position;
        Place(player, first.transform.position);
        yield return new WaitForSeconds(.15f);
        Check(guide.CurrentMessage == "HOLD A  •  REWIND  •  FOLLOW YOUR GHOST",
            "First-loop failure guidance explains the rewind setup");
        Place(player, origin);
        Rewind();
        yield return new WaitForSeconds(.35f);
        Check(guide.CurrentMessage == "GHOST HOLDS A  •  ACTIVATE B",
            "Ghost loop guidance points to the complementary switch");
        Place(player, second.transform.position);
        yield return new WaitForSeconds(.15f);
        Check(puzzle.IsSolved && guide.CurrentMessage == "TIME LINK COMPLETE",
            "Solved state replaces instructions with a clear completion message");
        Debug.Log("M6.5 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
