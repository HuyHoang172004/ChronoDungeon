using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDamage = 25f;
    [Range(30f, 360f)] public float attackArc = 180f;
    [Min(0f)] public float attackCooldown = 0.35f;
    private float nextAttackTime;
    private AttackFeedback feedback;
    private CombatFeedback combatFeedback;
    private readonly AttackResolver resolver = new AttackResolver();
    public event Action<AttackSnapshot> AttackPerformed;
    public float CooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);
    public bool CanAttack => CooldownRemaining <= 0f;

    private void Awake()
    {
        feedback = GetComponent<AttackFeedback>();
        if (feedback == null) feedback = gameObject.AddComponent<AttackFeedback>();
        combatFeedback = GetComponent<CombatFeedback>();
        if (combatFeedback == null) combatFeedback = gameObject.AddComponent<CombatFeedback>();
    }

    public void Attack()
    {
        Health ownHealth = GetComponentInParent<Health>();
        if (Time.timeScale <= 0f || (ownHealth != null && ownHealth.IsDead)) return;

        if (!isActiveAndEnabled || !CanAttack) return;
        var movement = GetComponent<PlayerMovement>();
        Vector2 direction = movement != null ? movement.FacingDirection : Vector2.right;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        var attack = new AttackSnapshot { Position = transform.position,
            Direction = direction.normalized, Range = attackRange, Damage = attackDamage,
            ArcAngle = attackArc };
        nextAttackTime = Time.time + Mathf.Max(0f, attackCooldown);
        // Record accepted attempts, including swings that hit nothing.
        AttackPerformed?.Invoke(attack);
        resolver.Execute(attack);
        if (feedback != null) feedback.Play(attack);
        if (combatFeedback != null) combatFeedback.PlayAttack();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
