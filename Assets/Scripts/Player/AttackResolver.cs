using System.Collections.Generic;
using UnityEngine;

public struct AttackSnapshot
{
    public Vector2 Position;
    public Vector2 Direction;
    public float Range;
    public float Damage;
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
        foreach (Collider2D hit in hits)
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target == null || target.IsDead || target.CompareTag("Player") ||
                target.GetComponent<PlayerMovement>() != null) continue;
            var enemy = target.GetComponent<EnemyAttackTarget>();
            if (enemy != null && enemy.isActiveAndEnabled && damaged.Add(target))
                target.TakeDamage(attack.Damage);
        }
        hits.Clear();
        damaged.Clear();
    }
}
