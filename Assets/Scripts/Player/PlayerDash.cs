using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerMovement))]
public sealed class PlayerDash : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(0.1f)] private float dashDistance = 2.5f;
    [SerializeField, Min(0.05f)] private float dashCooldown = 0.8f;
    [SerializeField] private TimeLoopManager loop;

    private Rigidbody2D body;
    private PlayerMovement movement;
    private DashFeedback feedback;
    private float nextDashTime;
    private Vector2 initialPosition;
    private Vector2 initialFacing;
    private readonly RaycastHit2D[] dashHits = new RaycastHit2D[8];
    private ContactFilter2D dashFilter;

    public event Action<Vector2> DashPerformed;
    public float DashDistance => dashDistance;
    public float DashCooldown => dashCooldown;
    public void SetDashCooldown(float value) => dashCooldown = Mathf.Max(0.1f, value);
    public float CooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);
    public bool CanDash => CooldownRemaining <= 0f && Time.timeScale > 0f;
    public Vector2 LastDashDirection { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        feedback = GetComponent<DashFeedback>();
        if (feedback == null) feedback = gameObject.AddComponent<DashFeedback>();
        if (loop == null) loop = FindAnyObjectByType<TimeLoopManager>();
        dashFilter.useTriggers = false;
        dashFilter.SetLayerMask(Physics2D.DefaultRaycastLayers);
    }

    public void CaptureInitialState()
    {
        initialPosition = body != null ? body.position : (Vector2)transform.position;
        initialFacing = movement != null ? movement.FacingDirection : Vector2.right;
    }

    public void ResetToInitialState()
    {
        nextDashTime = 0f;
        LastDashDirection = Vector2.zero;
        if (body != null) body.position = initialPosition;
        transform.position = initialPosition;
        if (movement != null) movement.SetMoveDirection(Vector2.zero);
        if (movement != null) movement.ResetToInitialState();
    }

    public void Dash()
    {
        Health health = GetComponentInParent<Health>();
        if (!isActiveAndEnabled || !CanDash || (health != null && health.IsDead)) return;

        Vector2 direction = movement != null ? movement.CurrentMoveDirection : Vector2.zero;
        if (direction.sqrMagnitude < 0.001f)
            direction = movement != null ? movement.FacingDirection : initialFacing;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        direction.Normalize();

        LastDashDirection = direction;
        nextDashTime = Time.time + dashCooldown;
        Vector2 start = body.position;
        float travel = dashDistance;
        int hitCount = body.Cast(direction, dashFilter, dashHits, dashDistance);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = dashHits[i].collider;
            if (hit == null || hit.attachedRigidbody == body || hit.isTrigger) continue;
            travel = Mathf.Min(travel, Mathf.Max(0f, dashHits[i].distance - 0.05f));
        }
        body.position = start + direction * travel;
        Physics2D.SyncTransforms();
        DashPerformed?.Invoke(direction);
        if (feedback != null) feedback.Play(start, direction, travel);
    }
}
