using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(EnemyProjectilePool))]
public sealed class EnemyArcher : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private Health target;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField, Min(0f)] private float moveSpeed = 1.6f;
    [SerializeField, Min(.1f)] private float retreatDistance = 3f;
    [SerializeField, Min(.1f)] private float approachDistance = 5f;
    [SerializeField, Min(.1f)] private float attackRange = 9f;
    [SerializeField, Min(.1f)] private float windup = .75f;
    [SerializeField, Min(.1f)] private float cooldown = 1.3f;
    [SerializeField, Min(.1f)] private float arrowSpeed = 7f;
    [SerializeField, Min(0f)] private float damage = 15f;
    private readonly List<RaycastHit2D> sightHits = new List<RaycastHit2D>(16);
    private Rigidbody2D body;
    private Health health;
    private EnemyProjectilePool pool;
    private Vector2 shotDirection;
    private float fireAt, nextAttack;
    public bool IsAiming { get; private set; }
    public Vector2 LockedDirection => shotDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        pool = GetComponent<EnemyProjectilePool>();
        if (aimLine != null) { aimLine.useWorldSpace = true; aimLine.positionCount = 2; }
    }
    private void OnEnable() => ResetToInitialState();
    private void OnDisable() => ResetToInitialState();
    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        IsAiming = false;
        fireAt = nextAttack = 0f;
        if (aimLine != null) aimLine.enabled = false;
        if (pool != null) pool.ResetToInitialState();
    }

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f) return;
        if (health.IsDead || target == null || target.IsDead || !target.gameObject.activeInHierarchy)
        { ResetToInitialState(); return; }
        if (IsAiming)
        {
            DrawAim();
            if (Time.time < fireAt) return;
            IsAiming = false;
            if (aimLine != null) aimLine.enabled = false;
            // Locked heading gives Player a reliable sidestep window; no homing.
            pool.TryFire(body.position, shotDirection, target, arrowSpeed, damage, 3f);
            nextAttack = Time.time + cooldown;
            return;
        }
        Vector2 delta = (Vector2)target.transform.position - body.position;
        float distance = delta.magnitude;
        if (distance <= attackRange && Time.time >= nextAttack && HasLineOfSight(delta, distance))
        {
            shotDirection = distance > .001f ? delta / distance : Vector2.right;
            IsAiming = true;
            fireAt = Time.time + windup;
            body.linearVelocity = Vector2.zero;
            DrawAim();
            return;
        }
        if (distance < .001f) return;
        float travel = moveSpeed * Time.fixedDeltaTime;
        if (distance < retreatDistance)
            body.MovePosition(body.position - delta / distance * Mathf.Min(travel, retreatDistance - distance));
        else if (distance > approachDistance)
            body.MovePosition(body.position + delta / distance * Mathf.Min(travel, distance - approachDistance));
    }

    private bool HasLineOfSight(Vector2 delta, float distance)
    {
        var filter = new ContactFilter2D { useTriggers = false };
        Physics2D.Raycast(body.position, delta.normalized, filter, sightHits, distance);
        foreach (var hit in sightHits)
            if (EnemyProjectile.Blocks(hit.collider, target) && hit.collider.GetComponentInParent<Health>() != target)
                return false;
        return true;
    }

    private void DrawAim()
    {
        if (aimLine == null) return;
        float progress = Mathf.Clamp01(1f - (fireAt - Time.time) / windup);
        Color color = Color.Lerp(new Color(1f, .65f, .12f, .45f), new Color(1f, .15f, .08f, .9f), progress);
        aimLine.startColor = aimLine.endColor = color;
        float length = attackRange;
        var filter = new ContactFilter2D { useTriggers = false };
        Physics2D.Raycast(body.position, shotDirection, filter, sightHits, attackRange);
        foreach (var hit in sightHits)
            if (EnemyProjectile.Blocks(hit.collider, target) && hit.collider.GetComponentInParent<Health>() != target)
                length = Mathf.Min(length, hit.distance);
        aimLine.SetPosition(0, body.position);
        aimLine.SetPosition(1, body.position + shotDirection * length);
        aimLine.enabled = true;
    }
}
