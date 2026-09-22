#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class KnightPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager manager;
    private PlayerMovement player;
    private EnemyKnight knight;
    private Health playerHealth;
    private Health knightHealth;
    private Vector2 center;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M5.4 FAIL: " + message);
        checks++; Debug.Log("M5.4 PASS: " + message);
    }
    private void Place(Component actor, Vector2 position)
    {
        actor.transform.position = position;
        var body = actor.GetComponent<Rigidbody2D>();
        if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }
    private void Enter(int index) => typeof(RoomManager).GetMethod("Enter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { index });
    private void Rewind() => typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(FindAnyObjectByType<TimeLoopManager>(), null);
    private IEnumerator Start()
    {
        yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        player = manager.Player;
        playerHealth = player.GetComponent<Health>();
        Enter(6);
        yield return null;
        var room = manager.CurrentRoom;
        knight = room.Content.GetComponentInChildren<EnemyKnight>();
        knightHealth = knight.GetComponent<Health>();
        center = knight.transform.position;
        Check(knightHealth.maxHealth == 140f && room.IsConfigured && knight.GetComponent<EnemyAttackTarget>() != null,
            "Elite room contains configured Chrono Knight using shared Enemy target");
        Check(knight.transform.Find("Knight Crest") != null && knight.transform.Find("Knight Blade") != null,
            "Knight visual has crest and blade readability");
        Place(knight, center);
        Place(player, center + Vector2.left * 4f);
        playerHealth.RestoreToFullHealth();
        yield return new WaitForSeconds(.12f);
        Check(!knight.IsCharging && knight.transform.Find("Charge Telegraph").GetComponent<LineRenderer>().enabled,
            "Charge wind-up displays a readable line before movement");
        yield return new WaitForSeconds(.65f);
        Check(knight.IsCharging && playerHealth.currentHealth == playerHealth.maxHealth,
            "Charge begins after wind-up without immediate damage");
        yield return new WaitForSeconds(.45f);
        Check(Mathf.Approximately(playerHealth.currentHealth, playerHealth.maxHealth - 35f),
            "Charge heavy hit deals 35 damage on contact");
        Check(!knight.IsCharging, "Charge ends after impact");
        // Block window: approach while the knight's interval is available.
        knight.ResetToInitialState();
        Place(knight, center); Place(player, center + Vector2.left * 1.4f);
        yield return new WaitForSeconds(.65f);
        var ring = knight.transform.Find("Block Ring").GetComponent<LineRenderer>();
        Check(knight.IsBlocking && ring.enabled, "Knight enters visible blue block state at close range");
        float beforeBlock = knightHealth.currentHealth;
        player.SetMoveDirection(Vector2.right); player.SetMoveDirection(Vector2.zero);
        player.GetComponent<PlayerAttack>().Attack();
        Check(knightHealth.currentHealth == beforeBlock && knight.BlockedAttackCount == 1,
            "Player attack is blocked during defense state");
        yield return new WaitForSeconds(.9f);
        Check(!knight.IsBlocking && !ring.enabled, "Block state expires cleanly");
        player.GetComponent<PlayerAttack>().Attack();
        Check(knightHealth.currentHealth == beforeBlock - 25f, "Player attack damages Knight after block ends");
        knightHealth.RestoreToFullHealth();
        playerHealth.RestoreToFullHealth();
        knight.ResetToInitialState();
        Place(knight, center); Place(player, center + Vector2.left * 4f);
        yield return new WaitForSeconds(.15f);
        Rewind();
        Check(!knight.IsCharging && !knight.IsBlocking && !ring.enabled && knightHealth.currentHealth == knightHealth.maxHealth,
            "Rewind cancels charge/block and restores Knight health");
        Check(FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 1, "Knight rewind still creates Ghost");
        knightHealth.TakeDamage(1000f);
        Check(!knight.gameObject.activeSelf && room.State == RoomState.Completed && room.ExitDoor.IsOpen,
            "Defeating Knight deactivates it and unlocks Elite exit");
        yield return null;
        Check(manager.TryAdvance(room, player) && !room.Content.activeInHierarchy,
            "Elite room transitions after Knight defeat");
        Debug.Log("M5.4 ALL CHECKS PASSED; checks=" + checks);
        FindAnyObjectByType<GameManager>().Restart();
    }
    private void OnDestroy() { Time.timeScale = 1f; }
}
#endif
