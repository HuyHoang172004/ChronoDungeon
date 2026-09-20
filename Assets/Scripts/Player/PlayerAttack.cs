using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDamage = 25f;

    public void Attack()
    {
        Health ownHealth = GetComponentInParent<Health>();
        if (Time.timeScale <= 0f || (ownHealth != null && ownHealth.IsDead)) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange
        );

        var damaged = new HashSet<Health>();
        foreach (Collider2D hit in hits)
        {
            Health target = hit.GetComponentInParent<Health>();

            if (target != null && target != ownHealth &&
                !target.transform.IsChildOf(transform) && !target.IsDead && damaged.Add(target))
            {
                target.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
