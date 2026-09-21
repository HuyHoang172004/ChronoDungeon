#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class PlayerFeedbackPlayModeChecks : MonoBehaviour
{
    private PlayerMovement player;
    private PlayerAttack attack;
    private Health playerHealth;
    private EnemyFollow enemy;
    private Health enemyHealth;
    private EnemyContactDamage contact;
    private int checks;
    private bool background;

    private void Check(bool ok, string message)
    {
        if (!ok) { StopAllCoroutines(); throw new System.Exception("M3.3 FAIL: " + message); }
        checks++;
        Debug.Log("M3.3 PASS: " + message);
    }

    private void PlaceEnemy(Vector2 position)
    {
        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        body.position = position;
        enemy.transform.position = position;
        Physics2D.SyncTransforms();
    }

    private IEnumerator Start()
    {
        background = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        yield return new WaitForSeconds(0.5f);

        player = FindAnyObjectByType<PlayerMovement>();
        attack = player.GetComponent<PlayerAttack>();
        playerHealth = player.GetComponent<Health>();
        enemy = FindAnyObjectByType<EnemyFollow>();
        enemyHealth = enemy.GetComponent<Health>();
        contact = enemy.GetComponent<EnemyContactDamage>();
        enemy.enabled = false;
        contact.enabled = false;
        player.SetMoveDirection(Vector2.right);
        player.transform.position = Vector3.zero;
        player.GetComponent<Rigidbody2D>().position = Vector2.zero;
        enemyHealth.RestoreToFullHealth();

        Check(player.GetComponent<CombatFeedback>() != null &&
            enemy.GetComponent<CombatFeedback>() != null,
            "Player and Enemy have reusable combat feedback");
        Check(player.GetComponent<AudioSource>() != null &&
            enemy.GetComponent<AudioSource>() != null,
            "Player and Enemy have audio feedback sources");

        PlaceEnemy(Vector2.right * 1.25f);
        attack.Attack();
        Check(attack.GetComponent<AudioSource>().isPlaying,
            "Player attack plays attack SFX");
        Check(enemyHealth.currentHealth < enemyHealth.maxHealth &&
            enemy.GetComponent<DamageFeedback>() != null,
            "Enemy hit applies damage and hit flash feedback");
        Check(enemy.GetComponent<AudioSource>().isPlaying,
            "Enemy damage plays damage SFX");

        float beforeCooldown = enemyHealth.currentHealth;
        attack.Attack();
        Check(enemyHealth.currentHealth == beforeCooldown,
            "Attack cooldown still prevents duplicate damage");

        playerHealth.RestoreToFullHealth();
        Vector2 contactPosition = new Vector2(player.transform.position.x, player.transform.position.y) + Vector2.right * 0.4f;
        PlaceEnemy(contactPosition);
        contact.enabled = true;
        yield return new WaitForFixedUpdate();
        Check(playerHealth.currentHealth < playerHealth.maxHealth,
            "Player receives contact damage");
        Check(player.GetComponent<CombatFeedback>() != null &&
            player.GetComponent<AudioSource>().isPlaying,
            "Player damage feedback and SFX are active");
        contact.enabled = false;

        enemyHealth.RestoreToFullHealth();
        enemyHealth.TakeDamage(enemyHealth.maxHealth + 1f);
        Check(enemyHealth.IsDead && enemy.GetComponent<CombatFeedback>() != null,
            "Enemy death triggers death feedback path");

        playerHealth.RestoreToFullHealth();
        Check(!playerHealth.IsDead && Time.timeScale == 1f,
            "Player remains alive and gameplay time remains active after feedback checks");

        Debug.Log("M3.3 ALL CHECKS PASSED; checks=" + checks);
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
