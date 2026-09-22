#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// Attach through MCP only in Play Mode; combines chase/tuning checks with M5.1 regression.
public sealed class ChaserPlayModeChecks : MonoBehaviour
{
    private int checks;
    private void Check(bool ok, string text)
    {
        if (!ok) throw new System.Exception("M5.2 FAIL: " + text);
        checks++;
        Debug.Log("M5.2 PASS: " + text);
    }
    private IEnumerator Start()
    {
        yield return null;
        var manager = FindAnyObjectByType<RoomManager>();
        var player = manager.Player;
        manager.TryAdvance(manager.CurrentRoom, player);
        yield return null;
        var room = manager.CurrentRoom;
        var guards = room.Content.GetComponentsInChildren<EnemyFollow>();
        var enemy = guards[0];
        guards[1].enabled = false;
        guards[1].GetComponent<EnemyContactDamage>().enabled = false;
        var body = enemy.GetComponent<Rigidbody2D>();
        var hp = enemy.GetComponent<Health>();
        var initial = body.position;
        player.transform.position = initial + Vector2.left * 4f;
        player.GetComponent<Rigidbody2D>().position = player.transform.position;
        player.SetMoveDirection(Vector2.zero);
        Physics2D.SyncTransforms();
        Check(hp.maxHealth == 75f && enemy.moveSpeed == 2.2f && enemy.moveSpeed < player.moveSpeed,
            "Chaser has 75 HP and 2.2 speed below Player speed");
        Check(enemy.transform.Find("Eye Left") != null && enemy.transform.Find("Fang Right") != null,
            "Distinct Ember Chaser face is present");
        yield return new WaitForSeconds(.4f);
        float travel = Vector2.Distance(initial, body.position);
        Check(travel > .65f && travel < 1.2f && body.position.x < initial.x,
            "Chaser closes distance at authored speed toward Player");
        Time.timeScale = 0f;
        var paused = body.position;
        yield return new WaitForSecondsRealtime(.15f);
        Check(body.position == paused, "Pause freezes chase");
        Time.timeScale = 1f;
        // Stop-distance rule in isolation, with real physics and no contact attack.
        enemy.GetComponent<EnemyContactDamage>().enabled = false;
        var collider = enemy.GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        player.transform.position = body.position + Vector2.left * .8f;
        player.GetComponent<Rigidbody2D>().position = player.transform.position;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.15f);
        Check(Mathf.Abs(Vector2.Distance(body.position, player.transform.position) - .75f) < .02f,
            "Chase stops at 0.75 without overshoot");
        enemy.enabled = false;
        var attack = player.GetComponent<PlayerAttack>();
        player.SetMoveDirection(Vector2.right);
        player.SetMoveDirection(Vector2.zero);
        attack.Attack();
        yield return new WaitForSeconds(.4f);
        attack.Attack();
        yield return new WaitForSeconds(.4f);
        Check(hp.currentHealth == 25f && enemy.gameObject.activeSelf,
            "Two directional attacks leave Chaser alive at 25 HP");
        attack.Attack();
        Check(hp.IsDead && !enemy.gameObject.activeSelf, "Third attack kills Chaser");
        Debug.Log("M5.2 ALL CHECKS PASSED; checks=" + checks);
        FindAnyObjectByType<GameManager>().Restart();
    }
    private void OnDestroy() { Time.timeScale = 1f; }
}
#endif
