#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

// Runtime-only MCP integration probe. Never save into the gameplay scene.
[DefaultExecutionOrder(10000)]
public sealed class PressureSwitchPlayModeChecks : MonoBehaviour
{
    private PressureSwitch plate;
    private PlayerMovement player;
    private TimeLoopManager loop;
    private TemporalGhostManager ghosts;
    private EnemyFollow enemy;
    private Health hp, enemyHP;
    private int checks, frames, changes, inspectorChanges, replayed;
    private bool monitor, resetObserved, originalBackground;
    private float worstPosition;
    private Vector3 center;

    private void Check(bool condition, string message)
    {
        if (!condition) { monitor = false; StopAllCoroutines(); throw new System.Exception("M2.1 FAIL: " + message); }
        checks++;
        Debug.Log("M2.1 PASS: " + message);
    }

    private void Bind()
    {
        plate = FindAnyObjectByType<PressureSwitch>();
        player = FindAnyObjectByType<PlayerMovement>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        ghosts = FindAnyObjectByType<TemporalGhostManager>();
        enemy = FindAnyObjectByType<EnemyFollow>();
        hp = player.GetComponent<Health>();
        enemyHP = enemy.GetComponent<Health>();
        center = plate.transform.position;
        enemy.enabled = false;
        enemy.GetComponent<EnemyContactDamage>().enabled = false;
    }

    private void LateUpdate()
    {
        if (!monitor || !loop.IsRunning) return;
        for (int i = 0; i < ghosts.ActiveGhostCount; i++)
        {
            var g = ghosts.GetGhost(i);
            float error = Vector3.Distance(g.transform.position, g.Timeline.Evaluate(loop.ElapsedTime).Position);
            worstPosition = Mathf.Max(worstPosition, error);
            if (error > 0.001f) Check(false, "Movement replay changed");
        }
        frames++;
    }

    private IEnumerator Settle()
    {
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.08f);
    }

    private void Place(Component actor, Vector3 position)
    {
        actor.transform.position = position;
        var rb = actor.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = position; rb.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private IEnumerator WalkTo(Vector3 destination)
    {
        float deadline = Time.realtimeSinceStartup + 5;
        while (Vector2.Distance(player.transform.position, destination) > 0.15f && Time.realtimeSinceStartup < deadline)
        {
            player.SetMoveDirection(destination - player.transform.position);
            yield return null;
        }
        player.SetMoveDirection(Vector2.zero);
        Check(Vector2.Distance(player.transform.position, destination) <= 0.15f, "Player reaches target using movement");
        yield return Settle();
    }

    private IEnumerator At(float time)
    {
        while (loop.ElapsedTime < time) yield return null;
    }

    private IEnumerator Rewind()
    {
        int old = loop.loopIndex;
        resetObserved = false;
        float deadline = Time.realtimeSinceStartup + 30;
        while (loop.loopIndex == old && Time.realtimeSinceStartup < deadline) yield return null;
        Check(loop.loopIndex == old + 1 && resetObserved, "Natural rewind resets switch inactive with zero occupants");
        yield return Settle();
        Check(!plate.IsActive && plate.OccupantCount == 0, "No stale occupancy after rewind teleports actors");
    }

    private IEnumerator Start()
    {
        originalBackground = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        FindAnyObjectByType<GameManager>().Restart();
        yield return null;
        yield return null;
        Bind();
        monitor = true;
        plate.StateChanged += active => changes++;
        var stateEvent = (UnityEvent<bool>)typeof(PressureSwitch).GetField("onStateChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plate);
        stateEvent.AddListener(active => inspectorChanges++);
        loop.LoopRewound += () => resetObserved = !plate.IsActive && plate.OccupantCount == 0;
        var visual = plate.transform.Find("Plate").GetComponent<SpriteRenderer>();
        var light = plate.transform.Find("Active Light").GetComponent<SpriteRenderer>();
        Color idleColor = visual.color;
        Check(!plate.IsActive && plate.OccupantCount == 0 && !light.enabled, "Initial switch is visibly inactive");
        Check(ghosts.MaxGhosts == 3 && loop.LoopDuration == 20, "Existing maxGhosts=3 and 20-second clock retained");
        var actor = player.GetComponent<PressureSwitchActor>();
        var bodyCollider = player.GetComponent<BoxCollider2D>();
        var extraObject = new GameObject("M2.1 extra actor collider");
        extraObject.transform.SetParent(player.transform, false);
        var extra = extraObject.AddComponent<BoxCollider2D>();
        extra.size = Vector2.one * 0.4f;
        extra.isTrigger = true;
        actor.RefreshColliders();
        Place(enemy, center + Vector3.right * 1.7f);
        enemy.GetComponent<TimeLoopActor>().CaptureInitialState();
        yield return At(1f);
        yield return WalkTo(center);
        float entered = loop.ElapsedTime;
        Check(plate.IsActive && plate.OccupantCount == 1, "Player with two colliders counts as one occupant");
        Check(light.enabled && visual.color != idleColor, "Active state changes color and enables light");
        int transitionCount = changes;
        bodyCollider.enabled = false;
        yield return Settle();
        Check(plate.IsActive && plate.OccupantCount == 1 && changes == transitionCount,
            "Disabling one collider keeps switch active through remaining collider");
        extraObject.transform.localPosition = Vector3.right * 10;
        yield return Settle();
        Check(!plate.IsActive && plate.OccupantCount == 0, "Last collider leaving deactivates switch");
        extraObject.transform.localPosition = Vector3.zero;
        yield return Settle();
        Check(plate.IsActive, "Returning child collider activates switch");
        actor.enabled = false;
        yield return Settle();
        Check(!plate.IsActive, "Disabled opt-in actor is removed without an exit callback");
        actor.enabled = true;
        bodyCollider.enabled = true;
        yield return Settle();
        player.GetComponent<PlayerAttack>().Attack();
        float attackTime = loop.ElapsedTime;
        Check(enemyHP.currentHealth == 75 && hp.currentHealth == 100, "Player attack remains exact 25 damage with switch present");
        yield return new WaitForSeconds(0.5f);
        yield return WalkTo(Vector3.zero);
        float departed = loop.ElapsedTime;
        Check(!plate.IsActive && !light.enabled && visual.color == idleColor, "Player leaving restores inactive visual");
        Place(enemy, center);
        yield return Settle();
        Check(!plate.IsActive, "Enemy without opt-in cannot activate switch");
        Place(enemy, center + Vector3.right * 1.7f);
        yield return At(18.5f);
        yield return WalkTo(center);
        Check(plate.IsActive, "Switch held active before rewind");
        yield return Rewind();
        var ghost = ghosts.ActiveGhost;
        Check(ghost != null && ghost.GetComponent<PressureSwitchActor>() != null &&
            ghost.GetComponent<Collider2D>() == null && ghost.GetComponent<Health>() == null,
            "Ghost explicitly opts in without physics collider or Health");
        ghost.ActionReplayed += action => replayed++;
        yield return At(entered + 0.05f);
        Check(plate.IsActive && plate.OccupantCount == 1 && Vector2.Distance(player.transform.position, center) > 2,
            "Recorded Ghost movement activates switch while live Player is elsewhere");
        yield return At(attackTime + 0.12f);
        Check(replayed == 1 && enemyHP.currentHealth == 75 && hp.currentHealth == 100,
            "Ghost attack replay remains once-only, exactly 25 Enemy damage and no Player damage");
        Place(player, center);
        yield return Settle();
        Check(plate.IsActive && plate.OccupantCount == 2, "Player plus Ghost count as two actors, not three colliders");
        transitionCount = changes;
        Place(player, Vector3.zero);
        yield return Settle();
        Check(plate.IsActive && plate.OccupantCount == 1 && changes == transitionCount,
            "Player exits while Ghost remains: no false inactive event");
        Place(player, center);
        yield return Settle();
        yield return At(departed + 0.2f);
        Check(plate.IsActive && plate.OccupantCount == 1 && changes == transitionCount,
            "Ghost exits while Player remains: switch stays active");
        Place(player, Vector3.zero);
        yield return Settle();
        Check(!plate.IsActive && plate.OccupantCount == 0, "Last actor exits: switch deactivates");
        // Lifecycle cases use a temporary compatible actor, not an extra Ghost.
        var temporary = new GameObject("M2.1 disposable actor");
        temporary.transform.position = center;
        temporary.AddComponent<PressureSwitchActor>().UseReplayPoint();
        yield return Settle();
        Check(plate.IsActive && plate.OccupantCount == 1, "Reusable opt-in actor activates switch");
        plate.enabled = false;
        Check(!plate.IsActive && plate.OccupantCount == 0, "Disabling switch clears output");
        plate.enabled = true;
        yield return Settle();
        Check(plate.IsActive, "Re-enabled switch reconciles existing occupants");
        Destroy(temporary);
        yield return Settle();
        Check(!plate.IsActive, "Destroyed actor cannot leave stale occupancy");
        yield return At(19.5f);
        Check(plate.IsActive && plate.OccupantCount == 1, "Ghost can hold switch until loop ends");
        yield return Rewind();
        Check(ghosts.ActiveGhostCount == 2 && ghosts.MaxGhosts == 3, "Multiple Ghost history survives switch resets");
        // Check the existing chase/contact path before triggering Game Over.
        Place(enemy, player.transform.position + Vector3.right * 1.5f);
        enemy.enabled = true;
        float distance = Vector2.Distance(enemy.transform.position, player.transform.position);
        yield return new WaitForSeconds(0.15f);
        Check(Vector2.Distance(enemy.transform.position, player.transform.position) < distance, "Enemy chase still works");
        enemy.enabled = false;
        Place(player, center);
        Place(enemy, center + Vector3.right * 0.4f);
        var contact = enemy.GetComponent<EnemyContactDamage>();
        contact.enabled = true;
        contact.ResetDamageCooldown();
        yield return new WaitForSeconds(0.15f);
        Check(hp.currentHealth < 100 && plate.IsActive, "Enemy contact damages Player while switch remains usable");
        hp.TakeDamage(10000);
        var game = FindAnyObjectByType<GameManager>();
        Check(game.IsGameOver && Time.timeScale == 0, "Game Over still freezes gameplay");
        float elapsed = loop.ElapsedTime;
        int previousChanges = changes, previousReplays = replayed;
        yield return new WaitForSecondsRealtime(0.3f);
        Check(loop.ElapsedTime == elapsed && previousChanges == changes && previousReplays == replayed,
            "Switch state, loop and Ghost actions stay frozen during Game Over");
        Check(changes == inspectorChanges, "C# and Inspector events agree on every state transition");
        monitor = false;
        var oldPlate = plate;
        game.Restart();
        yield return null;
        yield return null;
        Bind();
        yield return Settle();
        Check(oldPlate == null && !plate.IsActive && plate.OccupantCount == 0 && ghosts.ActiveGhostCount == 0 &&
            loop.loopIndex == 1 && hp.currentHealth == 100 && Time.timeScale == 1,
            "Restart gives clean switch, actor registry, Ghost history and Player state");
        Application.runInBackground = originalBackground;
        Debug.Log("M2.1 ALL CHECKS PASSED; checks=" + checks + "; frames=" + frames +
            "; transitions=" + changes + "; worstPosition=" + worstPosition);
    }

    private void OnDestroy() => Application.runInBackground = originalBackground;
}
#endif
