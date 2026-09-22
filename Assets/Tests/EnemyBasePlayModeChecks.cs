#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

// Lean integration probe. Attach only in Play Mode through Unity MCP.
public sealed class EnemyBasePlayModeChecks : MonoBehaviour
{
    private int checks;
    private bool background;
    private RoomManager manager;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M5.1 FAIL: " + message);
        checks++;
        Debug.Log("M5.1 PASS: " + message);
    }
    private void Place(Vector2 position)
    {
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = position;
        player.GetComponent<Rigidbody2D>().position = position;
        Physics2D.SyncTransforms();
    }
    private void Rewind() => typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(loop, null);
    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        player = manager.Player;
        loop = FindAnyObjectByType<TimeLoopManager>();
        Check(manager.TryAdvance(manager.CurrentRoom, player), "Start room advances through public progression API");
        yield return null;
        var room = manager.CurrentRoom;
        var enemies = room.Content.GetComponentsInChildren<EnemyFollow>();
        var enemy = enemies[0];
        // Isolate one guard while preserving the real room clear list.
        enemies[1].enabled = false;
        enemies[1].GetComponent<EnemyContactDamage>().enabled = false;
        var attack = enemy.GetComponent<EnemyContactDamage>();
        var line = enemy.GetComponentInChildren<EnemyAttackTelegraph>().GetComponent<LineRenderer>();
        var hp = player.GetComponent<Health>();
        var enemyHp = enemy.GetComponent<Health>();
        Vector3 originalScale = enemy.transform.localScale;
        Color originalColor = enemy.GetComponent<SpriteRenderer>().color;
        Place((Vector2)enemy.transform.position + Vector2.left * 0.8f);
        float before = hp.currentHealth;
        yield return new WaitForSeconds(.08f);
        Check(attack.IsWindingUp && line.enabled && hp.currentHealth == before,
            "Proximity starts visible wind-up without instant damage");
        Vector3 stopped = enemy.transform.position;
        yield return new WaitForSeconds(.1f);
        Check(Vector3.Distance(stopped, enemy.transform.position) < .02f, "Enemy stops chasing during wind-up");
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(.5f);
        Check(attack.IsWindingUp && line.enabled && hp.currentHealth == before, "Pause freezes pending attack");
        Time.timeScale = 1f;
        player.SetMoveDirection(Vector2.left);
        player.GetComponent<PlayerDash>().Dash();
        player.SetMoveDirection(Vector2.zero);
        yield return new WaitForSeconds(.4f);
        Check(hp.currentHealth == before && !attack.IsWindingUp && !line.enabled, "Dash out of warning avoids impact");
        attack.ResetDamageCooldown();
        Place((Vector2)enemy.transform.position + Vector2.left * .8f);
        var extra = player.gameObject.AddComponent<CircleCollider2D>();
        extra.isTrigger = true;
        extra.radius = .3f;
        yield return new WaitForSeconds(.6f);
        Check(Mathf.Approximately(hp.currentHealth, before - 20f), "One strike deals 20 damage once despite two Player colliders");
        yield return new WaitForSeconds(.2f);
        Check(Mathf.Approximately(hp.currentHealth, before - 20f), "Cooldown prevents repeated contact damage");
        Destroy(extra);
        attack.ResetDamageCooldown();
        yield return new WaitForSeconds(.06f);
        Check(attack.IsWindingUp, "Next attack can start after reset");
        Rewind();
        Check(!attack.IsWindingUp && !line.enabled && hp.currentHealth == hp.maxHealth,
            "Rewind cancels live wind-up and restores Player health");
        Check(FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 1, "Rewind still creates Ghost");
        // Real Player attack uses shared damage and explicit EnemyAttackTarget filtering.
        enemy.enabled = false;
        Place((Vector2)enemy.transform.position + Vector2.left * .8f);
        player.SetMoveDirection(Vector2.right);
        player.GetComponent<PlayerAttack>().Attack();
        player.SetMoveDirection(Vector2.zero);
        Check(enemyHp.currentHealth == enemyHp.maxHealth - 25f, "Directional Player attack still damages Enemy");
        enemyHp.TakeDamage(1000f);
        Check(!enemy.gameObject.activeSelf && enemyHp.IsDead && !attack.IsWindingUp && !line.enabled,
            "Death deactivates Enemy and cancels attack/telegraph");
        enemies[1].GetComponent<Health>().TakeDamage(1000f);
        Check(room.State == RoomState.Completed && room.ExitDoor.IsOpen, "All dead enemies clear room and unlock exit");
        Rewind();
        Check(enemy.gameObject.activeSelf && enemyHp.currentHealth == enemyHp.maxHealth &&
            enemy.transform.localScale == originalScale && enemy.GetComponent<SpriteRenderer>().color == originalColor,
            "Dead Enemy revives at full HP with clean visual state");
        Check(room.State == RoomState.Active && !room.ExitDoor.IsOpen && !attack.IsWindingUp,
            "Rewind restores combat lock and clean attack state");
        yield return new WaitForSeconds(.2f);
        Check(enemyHp.currentHealth == enemyHp.maxHealth - 25f, "Ghost replays recorded attack against revived Enemy");
        var ghost = FindAnyObjectByType<TemporalGhostManager>().ActiveGhost;
        var slash = ghost.transform.Find("Attack Slash").GetComponent<LineRenderer>();
        var trail = ghost.transform.Find("Dash Trail").GetComponent<LineRenderer>();
        Check(slash != trail && slash.positionCount == 3 && trail.positionCount == 2,
            "Ghost attack and dash own independent renderers");
        foreach (var guard in enemies) guard.GetComponent<Health>().TakeDamage(1000f);
        yield return null;
        Check(manager.TryAdvance(room, player), "Cleared combat room advances to puzzle");
        Check(!room.Content.activeInHierarchy && FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 0,
            "Transition disables old Enemy content and clears Ghosts");
        Debug.Log("M5.1 ALL CHECKS PASSED; checks=" + checks);
        Destroy(gameObject);
    }
    private void OnDestroy() { Application.runInBackground = background; Time.timeScale = 1f; }
}
#endif
