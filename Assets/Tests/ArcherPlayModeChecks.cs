#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class ArcherPlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager manager;
    private PlayerMovement player;
    private EnemyArcher archer;
    private EnemyProjectilePool pool;
    private Vector2 center;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M5.3 FAIL: " + message);
        checks++; Debug.Log("M5.3 PASS: " + message);
    }
    private void Place(Component actor, Vector2 position)
    {
        actor.transform.position = position;
        var body = actor.GetComponent<Rigidbody2D>();
        body.position = position; body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
    private void SetupShot()
    {
        archer.enabled = true;
        archer.ResetToInitialState();
        Place(archer, center + Vector2.right * 2f);
        Place(player, center + Vector2.left * 2f);
    }
    private IEnumerator Start()
    {
        yield return null;
        manager = FindAnyObjectByType<RoomManager>(); player = manager.Player;
        typeof(RoomManager).GetMethod("Enter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { 5 });
        yield return null;
        var room = manager.CurrentRoom;
        center = room.CameraAnchor.position;
        archer = room.Content.GetComponentInChildren<EnemyArcher>();
        pool = archer.GetComponent<EnemyProjectilePool>();
        var target = player.GetComponent<Health>();
        var health = archer.GetComponent<Health>();
        foreach (var chaser in room.Content.GetComponentsInChildren<EnemyFollow>())
        {
            chaser.enabled = false;
            chaser.GetComponent<EnemyContactDamage>().enabled = false;
        }
        Check(health.maxHealth == 50 && pool.Capacity == 3 && room.IsConfigured,
            "Mixed room contains configured 50 HP Archer with three-slot pool");
        SetupShot();
        float hp = target.currentHealth;
        yield return new WaitForSeconds(.1f);
        var aim = archer.transform.Find("Aim Telegraph").GetComponent<LineRenderer>();
        Check(archer.IsAiming && aim.enabled && pool.ActiveCount == 0 && target.currentHealth == hp,
            "Visible wind-up precedes arrow launch and damage");
        var locked = archer.LockedDirection;
        Place(player, center + new Vector2(-2f, 2f));
        yield return new WaitForSeconds(.15f);
        Check(archer.LockedDirection == locked, "Aim heading stays locked when Player sidesteps");
        yield return new WaitForSeconds(.6f);
        Check(pool.ActiveCount == 1 && !aim.enabled, "Wind-up launches one visible projectile");
        var arrow = room.Content.GetComponentInChildren<EnemyProjectile>();
        Vector3 paused = arrow.transform.position;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(.2f);
        Check(arrow.transform.position == paused && target.currentHealth == hp, "Pause freezes arrow and damage");
        Time.timeScale = 1f;
        yield return new WaitForSeconds(.55f);
        Check(target.currentHealth == hp, "Sidestep avoids locked projectile");
        SetupShot();
        var extra = player.gameObject.AddComponent<CircleCollider2D>(); extra.isTrigger = true; extra.radius = .4f;
        yield return new WaitForSeconds(1.4f);
        Check(Mathf.Approximately(target.currentHealth, hp - 15f) && pool.ActiveCount == 0,
            "Stationary Player takes exactly 15 damage; arrow returns to pool");
        Destroy(extra);
        archer.ResetToInitialState();
        typeof(EnemyArcher).GetField("nextAttack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(archer, Time.time + 10f);
        Place(archer, center); Place(player, center + Vector2.left * 2f);
        yield return new WaitForSeconds(.3f);
        Check(archer.transform.position.x > center.x + .3f, "Archer retreats when Player is too close");
        Place(archer, center + Vector2.right * 3f); Place(player, center + Vector2.left * 3f);
        yield return new WaitForSeconds(.3f);
        Check(archer.transform.position.x < center.x + 2.7f, "Archer approaches when Player is too far");
        archer.enabled = false;
        bool first = pool.TryFire(center, Vector2.up, target, 7f, 15f, 3f);
        bool second = pool.TryFire(center, Vector2.up, target, 7f, 15f, 3f);
        bool third = pool.TryFire(center, Vector2.up, target, 7f, 15f, 3f);
        Check(first && second && third && !pool.TryFire(center, Vector2.up, target, 7f, 15f, 3f) && pool.ActiveCount == 3,
            "Pool rejects fourth simultaneous arrow without growing");
        pool.ResetToInitialState();
        hp = target.currentHealth;
        Place(player, center + Vector2.left * 10f);
        pool.TryFire(center + Vector2.left * 7f, Vector2.left, target, 20f, 15f, 3f);
        yield return new WaitForSeconds(.2f);
        Check(pool.ActiveCount == 0 && target.currentHealth == hp, "Swept arrow hits west wall before Player behind it");
        Place(player, center + Vector2.left * 2f);
        pool.TryFire(center + Vector2.up * 50f, Vector2.right, target, 1f, 15f, .1f);
        yield return new WaitForSeconds(.2f);
        Check(pool.ActiveCount == 0 && pool.Capacity == 3, "Lifetime expiry releases slot for reuse");
        SetupShot();
        yield return new WaitForSeconds(.85f);
        Check(pool.ActiveCount == 1, "Arrow is in flight before rewind");
        var loop = FindAnyObjectByType<TimeLoopManager>();
        typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(loop, null);
        Check(pool.ActiveCount == 0 && !archer.IsAiming && !aim.enabled && health.currentHealth == 50,
            "Rewind clears projectiles, aim state and restores Archer health");
        Check(FindAnyObjectByType<TemporalGhostManager>().ActiveGhostCount == 1, "Archer encounter rewind still creates Ghost");
        // Record an actual Player attack for replay against a restored Archer.
        archer.enabled = false;
        Vector2 spawn = archer.transform.position;
        Place(player, spawn + Vector2.left * .8f);
        player.SetMoveDirection(Vector2.right); player.SetMoveDirection(Vector2.zero);
        player.GetComponent<PlayerAttack>().Attack();
        Check(health.currentHealth == 25f, "Player attack damages Archer through shared EnemyAttackTarget");
        yield return new WaitForSeconds(.1f);
        typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(loop, null);
        yield return new WaitForSeconds(.25f);
        Check(health.currentHealth == 25f, "Ghost replays recorded damage against Archer");
        pool.TryFire(center, Vector2.up, target, 7f, 15f, 3f);
        health.TakeDamage(25f);
        Check(!archer.gameObject.activeSelf && pool.ActiveCount == 0 && !aim.enabled,
            "Archer death deactivates actor and clears all owned arrows");
        foreach (var chaser in room.Content.GetComponentsInChildren<EnemyFollow>(true)) chaser.GetComponent<Health>().TakeDamage(1000f);
        Check(room.State == RoomState.Completed && room.ExitDoor.IsOpen, "Defeating mixed roster unlocks room exit");
        yield return null;
        Check(manager.TryAdvance(room, player) && !room.Content.activeInHierarchy && pool.ActiveCount == 0,
            "Room transition leaves no active arrows in previous room");
        Debug.Log("M5.3 ALL CHECKS PASSED; checks=" + checks);
        FindAnyObjectByType<GameManager>().Restart();
    }
    private void OnDestroy() { Time.timeScale = 1f; }
}
#endif
