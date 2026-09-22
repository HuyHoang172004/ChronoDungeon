using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChronoGuardian), typeof(EnemyAttackTelegraph))]
public sealed class ChronoGuardianPhase1 : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PlayerMovement target;
    [SerializeField, Min(.5f)] private float attackRange = 3f;
    [SerializeField, Min(.05f)] private float windupDuration = .8f;
    [SerializeField, Min(.1f)] private float cooldown = 2.2f;
    [SerializeField, Min(0f)] private float damage = 30f;
    [SerializeField, Min(.1f)] private float impactRadius = 1.25f;
    private EnemyAttackTelegraph telegraph;
    private Health bossHealth;
    private float strikeAt;
    private float nextAttack;
    private int attackCount;

    public bool IsWindingUp { get; private set; }
    public int AttackCount => attackCount;
    public float WindupDuration => windupDuration;
    public float ImpactRadius => impactRadius;

    private void Awake()
    {
        telegraph = GetComponent<EnemyAttackTelegraph>();
        bossHealth = GetComponent<Health>();
        if (target == null) target = FindAnyObjectByType<PlayerMovement>();
    }

    private void OnEnable() => ResetToInitialState();
    private void OnDisable() => ResetToInitialState();

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f || bossHealth == null || bossHealth.IsDead || target == null) return;
        if (IsWindingUp)
        {
            float progress = 1f - Mathf.Clamp01((strikeAt - Time.time) / windupDuration);
            if (telegraph != null) telegraph.Show(transform.position, impactRadius, progress);
            if (Time.time < strikeAt) return;
            IsWindingUp = false;
            if (telegraph != null) telegraph.Hide();
            nextAttack = Time.time + cooldown;
            attackCount++;
            ApplyImpact();
            return;
        }
        if (Time.time < nextAttack || Vector2.Distance(transform.position, target.transform.position) > attackRange) return;
        IsWindingUp = true;
        strikeAt = Time.time + windupDuration;
        if (telegraph != null) telegraph.Show(transform.position, impactRadius, 0f);
    }

    private void ApplyImpact()
    {
        var hits = new List<Collider2D>(8);
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        Physics2D.OverlapCircle(transform.position, impactRadius, filter, hits);
        foreach (Collider2D hit in hits)
        {
            Health victim = hit == null ? null : hit.GetComponentInParent<Health>();
            if (victim != null && victim == target.GetComponent<Health>() && !victim.IsDead)
            {
                victim.TakeDamage(damage);
                break;
            }
        }
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        IsWindingUp = false;
        strikeAt = nextAttack = 0f;
        attackCount = 0;
        if (telegraph != null) telegraph.Hide();
    }
}
