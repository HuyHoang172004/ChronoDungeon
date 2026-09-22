using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyContactDamage : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(0f)] private float damage = 20f;
    [SerializeField, Min(0.01f)] private float damageCooldown = 1f;
    [SerializeField, Min(0f)] private float contactRange = 0.65f;
    [SerializeField, Min(0.05f)] private float windupDuration = 0.45f;
    [SerializeField] private EnemyAttackTelegraph telegraph;
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private float strikeTime;
    private float nextDamageTime;
    private Health ownHealth;
    public bool IsWindingUp { get; private set; }
    public bool BlocksMovement => isActiveAndEnabled && IsWindingUp;

    private void Awake() => ownHealth = GetComponentInParent<Health>();
    private void OnEnable() => ResetDamageCooldown();
    private void OnDisable() => ResetDamageCooldown();

    public void ResetDamageCooldown()
    {
        IsWindingUp = false;
        strikeTime = nextDamageTime = 0f;
        hits.Clear();
        if (telegraph != null) telegraph.Hide();
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState() => ResetDamageCooldown();

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f) return;
        if (ownHealth != null && ownHealth.IsDead) { ResetDamageCooldown(); return; }
        if (IsWindingUp)
        {
            if (telegraph != null)
                telegraph.Show(transform.position, contactRange, 1f - (strikeTime - Time.time) / windupDuration);
            if (Time.time < strikeTime) return;
            IsWindingUp = false;
            if (telegraph != null) telegraph.Hide();
            nextDamageTime = Time.time + Mathf.Max(0.01f, damageCooldown);
            // Recheck at impact: leaving the warning circle avoids the attack.
            Health victim = FindPlayerInRange();
            if (victim != null) victim.TakeDamage(damage);
            return;
        }
        if (Time.time < nextDamageTime || FindPlayerInRange() == null) return;
        IsWindingUp = true;
        strikeTime = Time.time + Mathf.Max(0.05f, windupDuration);
        if (telegraph != null) telegraph.Show(transform.position, contactRange, 0f);
    }

    private Health FindPlayerInRange()
    {
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        Physics2D.OverlapCircle(transform.position, contactRange, filter, hits);
        foreach (Collider2D hit in hits)
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target == null || target == ownHealth || target.IsDead || !target.CompareTag("Player"))
                continue;

            return target;
        }
        return null;
    }
}
