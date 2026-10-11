using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
// Attack phases intentionally remain public read-only state so presentation and AI
// consume the same authoritative combat timing.
public class EnemyContactDamage : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(0f)] private float damage = 20f;
    [SerializeField, Min(0.01f)] private float damageCooldown = 1f;
    [SerializeField, Min(0f)] private float contactRange = 1.15f;
    [SerializeField, Min(0.05f)] private float windupDuration = 0.45f;
    [SerializeField] private EnemyAttackTelegraph telegraph;
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private float strikeTime;
    private float strikeAnimationEnd;
    private float fallbackImpactAt;
    private float recoveryEnd;
    private float nextDamageTime;
    private Health ownHealth;
    private Health playerHealth;
    private Transform playerTransform;
    private CharacterAnimationEventRelay animationRelay;
    private bool impactApplied;
    public bool IsWindingUp { get; private set; }
    public bool IsStriking { get; private set; }
    public bool IsRecovering => isActiveAndEnabled && !IsWindingUp && !IsStriking && Time.time < recoveryEnd;
    public bool BlocksMovement => isActiveAndEnabled && (IsWindingUp || IsStriking || IsRecovering);
    public float AttackRange => contactRange;

    public void Configure(float newDamage, float newRange, float newCooldown, float newWindup)
    {
        damage = Mathf.Max(0f, newDamage);
        contactRange = Mathf.Max(.1f, newRange);
        damageCooldown = Mathf.Max(.1f, newCooldown);
        windupDuration = Mathf.Max(.05f, newWindup);
    }

    private void Awake()
    {
        ownHealth = GetComponentInParent<Health>();
        FindPlayer();
        BindAnimationRelay();
    }
    private void OnEnable()
    {
        ResetDamageCooldown();
        BindAnimationRelay();
    }
    private void OnDisable()
    {
        if (animationRelay != null) animationRelay.Impact -= OnAnimationImpact;
        ResetDamageCooldown();
    }

    public void ResetDamageCooldown()
    {
        IsWindingUp = false;
        IsStriking = false;
        impactApplied = false;
        strikeTime = strikeAnimationEnd = fallbackImpactAt = recoveryEnd = nextDamageTime = 0f;
        hits.Clear();
        if (telegraph != null) telegraph.Hide();
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState() => ResetDamageCooldown();

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f) return;
        if (ownHealth != null && ownHealth.IsDead) { ResetDamageCooldown(); return; }
        if (animationRelay == null) BindAnimationRelay();
        if (IsStriking)
        {
            if (!impactApplied && Time.time >= fallbackImpactAt) ApplyImpact();
            if (Time.time < strikeAnimationEnd) return;
            IsStriking = false;
            recoveryEnd = Time.time + .18f;
            nextDamageTime = recoveryEnd + Mathf.Max(0.01f, damageCooldown);
            return;
        }
        if (IsRecovering) return;
        if (IsWindingUp)
        {
            if (telegraph != null)
                telegraph.Show(transform.position, contactRange, 1f - (strikeTime - Time.time) / windupDuration);
            if (Time.time < strikeTime) return;
            IsWindingUp = false;
            if (telegraph != null) telegraph.Hide();
            IsStriking = true;
            impactApplied = false;
            strikeAnimationEnd = Time.time + .36f;
            fallbackImpactAt = Time.time + .20f;
            return;
        }
        if (Time.time < nextDamageTime || FindPlayerInRange() == null) return;
        IsWindingUp = true;
        strikeTime = Time.time + Mathf.Max(0.05f, windupDuration);
        if (telegraph != null) telegraph.Show(transform.position, contactRange, 0f);
    }

    private void BindAnimationRelay()
    {
        CharacterAnimationEventRelay found = GetComponent<CharacterAnimationEventRelay>();
        if (found == null) return;
        if (animationRelay != null) animationRelay.Impact -= OnAnimationImpact;
        animationRelay = found;
        animationRelay.Impact -= OnAnimationImpact;
        animationRelay.Impact += OnAnimationImpact;
    }

    private void OnAnimationImpact()
    {
        if (IsStriking && !impactApplied) ApplyImpact();
    }

    private void ApplyImpact()
    {
        impactApplied = true;
        Health victim = FindPlayerInRange();
        if (victim != null) victim.TakeDamage(damage);
    }

    private Health FindPlayerInRange()
    {
        if (playerHealth == null || playerTransform == null) FindPlayer();
        if (playerHealth != null && playerTransform != null && !playerHealth.IsDead &&
            Vector2.Distance(transform.position, playerTransform.position) <= contactRange)
            return playerHealth;

        // Collider query remains as a fallback for scenes with a nonstandard Player hierarchy.
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

    private void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        playerTransform = player.transform;
        playerHealth = player.GetComponent<Health>();
        if (playerHealth == null) playerHealth = player.GetComponentInChildren<Health>(true);
        if (playerHealth == null) playerHealth = player.GetComponentInParent<Health>();
    }
}
