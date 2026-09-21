#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class DirectionalCombatPlayModeChecks : MonoBehaviour
{
    private PlayerMovement player;
    private PlayerAttack attack;
    private Health playerHealth;
    private Health enemyHealth;
    private EnemyFollow follow;
    private TimeLoopManager loop;
    private int checks;
    private bool background;

    private void Check(bool ok, string message)
    {
        if (!ok) { StopAllCoroutines(); throw new System.Exception("M3.1 FAIL: " + message); }
        checks++;
        Debug.Log("M3.1 PASS: " + message);
    }

    private void PlaceEnemy(Vector2 position)
    {
        var body = follow.GetComponent<Rigidbody2D>();
        body.position = position;
        follow.transform.position = position;
        Physics2D.SyncTransforms();
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        yield return new WaitForSeconds(0.5f);

        player = FindAnyObjectByType<PlayerMovement>();
        if (player == null) { Debug.LogError("M3.1 FAIL: Player did not load"); yield break; }
        attack = player.GetComponent<PlayerAttack>();
        playerHealth = player.GetComponent<Health>();
        follow = FindAnyObjectByType<EnemyFollow>();
        loop = FindAnyObjectByType<TimeLoopManager>();
        follow.enabled = false;
        var contact = follow.GetComponent<EnemyContactDamage>();
        if (contact != null) contact.enabled = false;
        enemyHealth = follow.GetComponent<Health>();
        player.SetMoveDirection(Vector2.zero);
        player.transform.position = Vector3.zero;
        player.GetComponent<Rigidbody2D>().position = Vector2.zero;
        enemyHealth.RestoreToFullHealth();
        yield return null;

        Check(attack.attackArc >= 30f && attack.attackCooldown > 0f &&
            attack.GetComponent<AttackFeedback>() != null, "Directional attack configuration and visual feedback are present");

        player.SetMoveDirection(Vector2.right);
        PlaceEnemy(Vector2.right * 1.25f);
        attack.Attack();
        Check(enemyHealth.currentHealth == enemyHealth.maxHealth - attack.attackDamage,
            "Enemy in facing direction receives one attack hit");
        Check(attack.GetComponent<AttackFeedback>().GetComponent<LineRenderer>().enabled,
            "Attack slash visual activates");
        Check(follow.GetComponent<DamageFeedback>() != null,
            "Enemy hit feedback component is created");

        float healthAfterFirst = enemyHealth.currentHealth;
        attack.Attack();
        Check(enemyHealth.currentHealth == healthAfterFirst && !attack.CanAttack,
            "Cooldown prevents accidental immediate multi-hit");
        yield return new WaitForSeconds(attack.attackCooldown + 0.05f);
        attack.Attack();
        Check(enemyHealth.currentHealth == healthAfterFirst - attack.attackDamage,
            "Attack becomes available after cooldown");

        enemyHealth.RestoreToFullHealth();
        player.SetMoveDirection(Vector2.right);
        PlaceEnemy(Vector2.left * 1.25f);
        attack.Attack();
        Check(enemyHealth.currentHealth == enemyHealth.maxHealth,
            "Enemy behind Player is outside directional attack arc");
        player.SetMoveDirection(Vector2.up);
        PlaceEnemy(Vector2.up * 1.25f);
        yield return new WaitForSeconds(attack.attackCooldown + 0.05f);
        attack.Attack();
        Check(enemyHealth.currentHealth == enemyHealth.maxHealth - attack.attackDamage,
            "Changing facing direction changes valid hit direction");

        var recorder = FindAnyObjectByType<PlayerTimelineRecorder>();
        bool hasArc = false;
        for (int i = 0; i < recorder.CurrentRecording.ActionCount; i++)
            hasArc |= Mathf.Abs(recorder.CurrentRecording.GetAction(i).Attack.ArcAngle - attack.attackArc) < 0.001f;
        Debug.Log("M3.1 INFO: recorded actions=" + recorder.CurrentRecording.ActionCount + "; attack arc=" + attack.attackArc + "; hasArc=" + hasArc);
        Check(hasArc,
            "Timeline records directional arc payload");

        Time.timeScale = 0f;
        float frozen = loop.ElapsedTime;
        attack.Attack();
        yield return new WaitForSecondsRealtime(0.1f);
        Check(loop.ElapsedTime == frozen, "Pause freezes combat and loop time");
        Time.timeScale = 1f;

        Debug.Log("M3.1 ALL CHECKS PASSED; checks=" + checks);
        Application.runInBackground = background;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Application.runInBackground = background;
    }
}
#endif
