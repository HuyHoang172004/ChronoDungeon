#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class ElitePlayModeChecks : MonoBehaviour
{
    private int checks;
    private RoomManager manager;
    private PlayerMovement player;
    private EliteEnemy elite;
    private Health playerHealth;
    private Health eliteHealth;
    private void Check(bool ok, string message)
    {
        if (!ok) throw new System.Exception("M5.5 FAIL: " + message);
        checks++; Debug.Log("M5.5 PASS: " + message);
    }
    private void Place(Component actor, Vector2 position)
    {
        actor.transform.position = position;
        var body = actor.GetComponent<Rigidbody2D>();
        if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }
    private void Enter(int index) => typeof(RoomManager).GetMethod("Enter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, new object[] { index });
    private static float Field(object target, string name) => (float)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private IEnumerator Start()
    {
        yield return null;
        manager = FindAnyObjectByType<RoomManager>();
        player = manager.Player;
        playerHealth = player.GetComponent<Health>();
        Enter(6);
        yield return null;
        var room = manager.CurrentRoom;
        elite = room.Content.GetComponentInChildren<EliteEnemy>();
        eliteHealth = elite.GetComponent<Health>();
        var knight = elite.GetComponent<EnemyKnight>();
        var aura = elite.transform.Find("Elite Aura").GetComponent<SpriteRenderer>();
        Check(elite != null && eliteHealth.maxHealth == 220f && room.IsConfigured,
            "Elite uses stronger 220 HP tuning and remains room-configured");
        Check(Field(knight, "chargeDamage") == 50f && Field(knight, "chargeSpeed") == 8f &&
            Field(knight, "blockDuration") == 1.1f && Field(knight, "blockInterval") == 2.3f,
            "Elite has stronger charge and defense pattern values");
        Check(aura.enabled && aura.color.a > .1f && elite.GetComponent<EnemyAttackTarget>() != null,
            "Elite aura is visible and combat target remains compatible");
        playerHealth.RestoreToFullHealth();
        playerHealth.TakeDamage(50f);
        float beforeReward = playerHealth.currentHealth;
        eliteHealth.TakeDamage(10000f);
        yield return null;
        var reward = elite.Reward;
        Check(!elite.gameObject.activeSelf && reward != null && reward.gameObject.activeSelf &&
            room.State == RoomState.Completed && room.ExitDoor.IsOpen,
            "Elite death opens exit and spawns a visible reward");
        Place(player, reward.transform.position);
        yield return new WaitForSeconds(.1f);
        Check(Mathf.Approximately(playerHealth.currentHealth, beforeReward + 35f) && !reward.gameObject.activeSelf,
            "Player collects reward and heals 35 HP");
        var loop = FindAnyObjectByType<TimeLoopManager>();
        typeof(TimeLoopManager).GetMethod("Rewind", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(loop, null);
        yield return null;
        Check(elite.gameObject.activeSelf && eliteHealth.currentHealth == eliteHealth.maxHealth &&
            !elite.GetComponent<EnemyKnight>().IsBlocking && !elite.GetComponent<EnemyKnight>().IsCharging,
            "Rewind restores elite actor and combat state");
        Check(!reward.gameObject.activeSelf && elite.GetComponent<EliteEnemy>().Reward == reward,
            "Rewind clears consumed reward without destroying its reusable slot");
        eliteHealth.TakeDamage(10000f);
        yield return null;
        Check(reward.gameObject.activeSelf, "Elite can spawn the reward again after a later death");
        Debug.Log("M5.5 ALL CHECKS PASSED; checks=" + checks);
        FindAnyObjectByType<GameManager>().Restart();
    }
    private void OnDestroy() { Time.timeScale = 1f; }
}
#endif
