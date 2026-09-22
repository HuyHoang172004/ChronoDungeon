#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class CombatCooperationPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager rooms;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private CombatCooperationObjective objective;

    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M6.3 FAIL: " + message);
        checks++;
        Debug.Log("M6.3 PASS: " + message);
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
        Enter(5); // Split Bastion combat challenge
        yield return null;
        objective = FindAnyObjectByType<CombatCooperationObjective>();
        var ghostTarget = objective.GhostTarget;
        var playerTarget = objective.PlayerTarget;
        Check(objective != null && ghostTarget != null && playerTarget != null && !objective.IsComplete,
            "Combat cooperation objective has distinct Ghost and Player targets");

        Vector2 origin = player.transform.position;
        Place(player, ghostTarget.transform.position);
        player.SetMoveDirection(Vector2.zero);
        player.GetComponent<PlayerAttack>().Attack();
        yield return new WaitForSeconds(.1f);
        Check(!ghostTarget.GhostDamaged && !objective.IsComplete,
            "Player attack alone cannot satisfy the Ghost target requirement");
        Place(player, origin);
        Rewind();
        yield return new WaitForSeconds(.35f);
        Check(ghostTarget.GhostDamaged && !objective.IsComplete,
            "Ghost replay damages its assigned target while objective remains incomplete");

        Place(player, playerTarget.transform.position);
        player.SetMoveDirection(Vector2.zero);
        player.GetComponent<PlayerAttack>().Attack();
        yield return new WaitForSeconds(.1f);
        Check(playerTarget.PlayerDamaged && objective.IsComplete,
            "Player completes the complementary target after Ghost cooperation");
        Rewind();
        yield return null;
        Check(!ghostTarget.GhostDamaged && !playerTarget.PlayerDamaged && !objective.IsComplete,
            "Rewind resets both cooperation roles and objective state");
        Debug.Log("M6.3 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
#endif
