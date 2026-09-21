using System;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDamage = 25f;
    private readonly AttackResolver resolver = new AttackResolver();
    public event Action<AttackSnapshot> AttackPerformed;

    public void Attack()
    {
        Health ownHealth = GetComponentInParent<Health>();
        if (Time.timeScale <= 0f || (ownHealth != null && ownHealth.IsDead)) return;

        if (!isActiveAndEnabled) return;
        var movement = GetComponent<PlayerMovement>();
        var attack = new AttackSnapshot { Position = transform.position,
            Direction = movement != null ? movement.FacingDirection : Vector2.right,
            Range = attackRange, Damage = attackDamage };
        // Record accepted attempts, including swings that hit nothing.
        AttackPerformed?.Invoke(attack);
        resolver.Execute(attack);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
