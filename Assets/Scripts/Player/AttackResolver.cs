using System.Collections.Generic;
using UnityEngine;

public struct AttackSnapshot
{
    public Vector2 Position;
    public Vector2 Direction;
    public float Range;
    public float Damage;
    public float ArcAngle;
}

// Shared radial hit rules. Direction is recorded for future directional combat.
public sealed class AttackResolver
{
    private readonly List<Collider2D> hits = new List<Collider2D>(16);
    private readonly HashSet<Health> damaged = new HashSet<Health>();

    public void Execute(AttackSnapshot attack)
    {
        damaged.Clear();
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        Physics2D.OverlapCircle(attack.Position, attack.Range, filter, hits);
        float halfArc = Mathf.Clamp(attack.ArcAngle <= 0f ? 360f : attack.ArcAngle, 0f, 360f) * 0.5f;
        float minDot = Mathf.Cos(halfArc * Mathf.Deg2Rad);
        Vector2 direction = attack.Direction.sqrMagnitude > 0.001f ? attack.Direction.normalized : Vector2.right;
        foreach (Collider2D hit in hits)
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target == null || target.IsDead || target.CompareTag("Player") ||
                target.GetComponent<PlayerMovement>() != null) continue;
            Vector2 toTarget = (Vector2)target.transform.position - attack.Position;
            if (toTarget.sqrMagnitude > 0.001f && Vector2.Dot(direction, toTarget.normalized) < minDot) continue;
            var enemy = target.GetComponent<EnemyAttackTarget>();
            if (enemy != null && enemy.isActiveAndEnabled && damaged.Add(target))
            {
                var feedback = target.GetComponent<DamageFeedback>();
                if (feedback == null) feedback = target.gameObject.AddComponent<DamageFeedback>();
                target.TakeDamage(attack.Damage);
            }
        }
        hits.Clear();
        damaged.Clear();
    }
}
