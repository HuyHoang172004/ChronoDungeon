using System.Collections.Generic;
using UnityEngine;

// Swept collision prevents fast arrows tunnelling through walls or the Player.
public sealed class EnemyProjectile : MonoBehaviour
{
    [SerializeField, Min(.01f)] private float radius = .08f;
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
    private Health target;
    private Vector2 direction;
    private float speed, damage, remaining;
    public bool IsFlying => gameObject.activeSelf;

    public void Launch(Vector2 origin, Vector2 heading, Health victim, float velocity, float amount, float lifetime)
    {
        target = victim;
        direction = heading.normalized;
        speed = velocity;
        damage = amount;
        remaining = lifetime;
        transform.SetPositionAndRotation(origin, Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        gameObject.SetActive(true);
    }

    // Non-target actors and trigger-only puzzle volumes do not intercept enemy arrows.
    public static bool Blocks(Collider2D collider, Health victim)
    {
        if (collider == null || collider.isTrigger) return false;
        Health actor = collider.GetComponentInParent<Health>();
        return actor == null || actor == victim;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f) return;
        if (target == null || !target.gameObject.activeInHierarchy || target.IsDead) { Cancel(); return; }
        float step = Mathf.Min(remaining, Time.fixedDeltaTime);
        float distance = speed * step;
        var filter = new ContactFilter2D { useTriggers = false };
        Physics2D.CircleCast(transform.position, radius, direction, filter, hits, distance);
        Collider2D closest = null;
        float nearest = float.PositiveInfinity;
        foreach (var hit in hits)
        {
            if (!Blocks(hit.collider, target) || hit.distance >= nearest) continue;
            closest = hit.collider;
            nearest = hit.distance;
        }
        if (closest != null)
        {
            // Return to pool before damage/events; one arrow can only hit once.
            Cancel();
            if (closest.GetComponentInParent<Health>() == target) target.TakeDamage(damage);
            return;
        }
        transform.position += (Vector3)(direction * distance);
        remaining -= step;
        if (remaining <= 0f) Cancel();
    }

    public void Cancel() { gameObject.SetActive(false); hits.Clear(); }
}
