using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public sealed class EnemyKnight : MonoBehaviour, ITimeLoopResettable
{
    private enum State { Chase, ChargeWindup, Charging, Blocking }
    [SerializeField] private Health target;
    [SerializeField] private LineRenderer chargeLine;
    [SerializeField] private LineRenderer blockRing;
    [SerializeField, Min(0f)] private float chaseSpeed = 1.35f;
    [SerializeField, Min(.1f)] private float chargeTriggerDistance = 5.5f;
    [SerializeField, Min(.1f)] private float chargeStopDistance = 2.2f;
    [SerializeField, Min(.1f)] private float chargeWindup = .6f;
    [SerializeField, Min(.1f)] private float chargeSpeed = 7f;
    [SerializeField, Min(.1f)] private float chargeDuration = .6f;
    [SerializeField, Min(.1f)] private float chargeCooldown = 2.4f;
    [SerializeField, Min(0f)] private float chargeDamage = 35f;
    [SerializeField, Min(.1f)] private float blockDuration = .8f;
    [SerializeField, Min(.1f)] private float blockInterval = 3.2f;
    [SerializeField, Min(.01f)] private float hitRadius = .55f;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    private Rigidbody2D body;
    private Health health;
    private State state;
    private Vector2 chargeDirection;
    private float stateUntil;
    private float nextCharge;
    private float nextBlock;
    private int blockedAttacks;
    public bool IsBlocking => state == State.Blocking && isActiveAndEnabled;
    public bool IsCharging => state == State.Charging;
    public int BlockedAttackCount => blockedAttacks;
    public Vector2 ChargeDirection => chargeDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        ConfigureLine(chargeLine, 2);
        ConfigureLine(blockRing, 32);
    }

    private static void ConfigureLine(LineRenderer line, int points)
    {
        if (line == null) return;
        line.useWorldSpace = true;
        line.positionCount = points;
        line.enabled = false;
    }

    private void OnEnable() => ResetToInitialState();
    private void OnDisable() => ResetToInitialState();
    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        state = State.Chase;
        stateUntil = nextCharge = 0f;
        nextBlock = Time.time + .5f;
        blockedAttacks = 0;
        if (chargeLine != null) chargeLine.enabled = false;
        if (blockRing != null) blockRing.enabled = false;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale <= 0f || health == null || health.IsDead || target == null || target.IsDead)
            return;
        Vector2 delta = (Vector2)target.transform.position - body.position;
        float distance = delta.magnitude;
        if (state == State.Blocking)
        {
            DrawBlock();
            if (Time.time >= stateUntil)
            {
                state = State.Chase;
                nextCharge = Time.time + .35f;
                nextBlock = Time.time + blockInterval;
                if (blockRing != null) blockRing.enabled = false;
            }
            return;
        }
        if (state == State.ChargeWindup)
        {
            DrawCharge(.5f * (1f - Mathf.Clamp01((stateUntil - Time.time) / chargeWindup)));
            if (Time.time >= stateUntil)
            {
                state = State.Charging;
                stateUntil = Time.time + chargeDuration;
                DrawCharge(1f);
            }
            return;
        }
        if (state == State.Charging)
        {
            PerformChargeStep();
            if (Time.time >= stateUntil)
            {
                state = State.Chase;
                nextCharge = Time.time + chargeCooldown;
                if (chargeLine != null) chargeLine.enabled = false;
            }
            return;
        }
        if (distance <= 2.4f && Time.time >= nextBlock)
        {
            state = State.Blocking;
            stateUntil = Time.time + blockDuration;
            if (blockRing != null) blockRing.enabled = true;
            return;
        }
        if (distance >= chargeStopDistance && distance <= chargeTriggerDistance && Time.time >= nextCharge)
        {
            chargeDirection = distance > .001f ? delta / distance : Vector2.right;
            state = State.ChargeWindup;
            stateUntil = Time.time + chargeWindup;
            DrawCharge(0f);
            return;
        }
        if (distance > chargeStopDistance)
        {
            float travel = Mathf.Min(chaseSpeed * Time.fixedDeltaTime, distance - chargeStopDistance);
            body.MovePosition(body.position + delta.normalized * travel);
        }
    }

    private void PerformChargeStep()
    {
        float distance = chargeSpeed * Time.fixedDeltaTime;
        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers);
        int hitCount = Physics2D.CircleCast(body.position, hitRadius, chargeDirection, filter, hits, distance);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = hits[i];
            Health victim = hit.collider == null ? null : hit.collider.GetComponentInParent<Health>();
            if (victim != target || victim.IsDead) continue;
            target.TakeDamage(chargeDamage);
            stateUntil = Time.time;
            return;
        }
        Vector2 next = body.position + chargeDirection * distance;
        body.MovePosition(next);
        // Rigidbody collision resolution can stop just before the Player collider;
        // confirm the authored heavy-hit radius after movement as a stable fallback.
        if (Vector2.Distance(next, target.transform.position) <= hitRadius + .75f)
        {
            target.TakeDamage(chargeDamage);
            stateUntil = Time.time;
        }
    }

    private void DrawCharge(float progress)
    {
        if (chargeLine == null) return;
        Color color = Color.Lerp(new Color(.9f, .35f, .1f, .45f), new Color(1f, .08f, .03f, .95f), Mathf.Clamp01(progress));
        chargeLine.startColor = chargeLine.endColor = color;
        chargeLine.SetPosition(0, body.position);
        chargeLine.SetPosition(1, body.position + chargeDirection * chargeTriggerDistance);
        chargeLine.enabled = true;
    }

    private void DrawBlock()
    {
        if (blockRing == null) return;
        blockRing.startColor = blockRing.endColor = new Color(.25f, .7f, 1f, .9f);
        for (int i = 0; i < blockRing.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / blockRing.positionCount;
            blockRing.SetPosition(i, body.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .8f);
        }
        blockRing.enabled = true;
    }

    public void NotifyBlockedAttack() { if (IsBlocking) blockedAttacks++; }
}
