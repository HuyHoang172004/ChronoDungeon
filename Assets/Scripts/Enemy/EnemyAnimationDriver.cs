using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyAnimationDriver : MonoBehaviour
{
    private static readonly string[] DirectionNames = { "Down", "Left", "Right", "Up" };
    [SerializeField] private CharacterAnimationProfile profile;
    private Animator animator;
    private Rigidbody2D body;
    private EnemyContactDamage attack;
    private EnemyArcher archer;
    private EnemyFollow follow;
    private Health health;
    private string activeState;
    private int direction;
    private float previousHealth;
    private float hitUntil;
    private CharacterAnimationState requestedAction;
    private float requestedActionUntil;
    private Vector2 requestedFacing;

    public CharacterAnimationProfile Profile => profile;

    public void Configure(CharacterAnimationProfile value)
    {
        profile = value;
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null && profile != null && profile.Controller != null)
            animator.runtimeAnimatorController = profile.Controller;
        activeState = null;
    }

    public void RequestAction(CharacterAnimationState action, float duration, Vector2 facing)
    {
        requestedAction = action;
        requestedActionUntil = Time.time + Mathf.Max(0.01f, duration);
        if (facing.sqrMagnitude > .001f) requestedFacing = facing.normalized;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        if (GetComponent<CharacterAnimationEventRelay>() == null)
            gameObject.AddComponent<CharacterAnimationEventRelay>();
        if (profile == null) profile = CharacterAnimationResolver.Resolve(gameObject);
        // Every character owns its controller through a profile. Never assign the
        // RuinHusk controller to unrelated enemies as a global fallback.
        if (profile != null && profile.Controller != null)
            animator.runtimeAnimatorController = profile.Controller;
        else if (animator.runtimeAnimatorController == null && name.Contains("RuinHusk"))
            animator.runtimeAnimatorController =
                Resources.Load<RuntimeAnimatorController>("EnemyAnimations/RuinHusk");
        body = GetComponent<Rigidbody2D>();
        attack = GetComponent<EnemyContactDamage>();
        archer = GetComponent<EnemyArcher>();
        follow = GetComponent<EnemyFollow>();
        health = GetComponent<Health>();
        if (health != null) previousHealth = health.currentHealth;
    }

    private void OnEnable()
    {
        activeState = null;
        if (health == null) health = GetComponent<Health>();
        if (health != null)
        {
            previousHealth = health.currentHealth;
            health.Changed -= OnHealthChanged;
            health.Changed += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null) health.Changed -= OnHealthChanged;
    }

    private void Update()
    {
        if (attack == null) attack = GetComponent<EnemyContactDamage>();
        if (animator == null || animator.runtimeAnimatorController == null) return;
        Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
        Vector2 facing = Time.time < requestedActionUntil && requestedFacing.sqrMagnitude > .001f
            ? requestedFacing
            : follow != null ? follow.FacingDirection : velocity;
        bool moving = follow != null ? follow.IsMoving : velocity.sqrMagnitude > .01f;
        if (facing.sqrMagnitude > .01f) direction = ResolveDirection(facing);

        CharacterAnimationState action = ResolveAction(moving);
        string next = profile != null
            ? profile.StateName(action, DirectionNames[direction])
            : LegacyStateName(action, DirectionNames[direction]);
        int stateHash = Animator.StringToHash(next);
        if (!animator.HasState(0, stateHash))
        {
            CharacterAnimationState fallback = moving ? CharacterAnimationState.Move : CharacterAnimationState.Idle;
            next = profile != null
                ? profile.StateName(fallback, DirectionNames[direction])
                : LegacyStateName(fallback, DirectionNames[direction]);
            stateHash = Animator.StringToHash(next);
            if (!animator.HasState(0, stateHash)) return;
        }
        if (next == activeState) return;
        activeState = next;
        animator.Play(stateHash, 0, 0f);
    }

    private CharacterAnimationState ResolveAction(bool moving)
    {
        if (health != null && health.IsDead) return CharacterAnimationState.Death;
        if (Time.time < hitUntil) return CharacterAnimationState.Hit;
        if (Time.time < requestedActionUntil) return requestedAction;
        if (archer != null && archer.IsAiming) return CharacterAnimationState.Windup;
        if (attack != null && attack.IsWindingUp) return CharacterAnimationState.Windup;
        if (attack != null && attack.IsStriking) return CharacterAnimationState.Attack;
        if (attack != null && attack.IsRecovering) return CharacterAnimationState.Recover;
        if (moving && follow != null)
        {
            if (follow.MovementState == EnemyMovementState.Patrol ||
                follow.MovementState == EnemyMovementState.Return)
                return CharacterAnimationState.Patrol;
            if (follow.MovementState == EnemyMovementState.Chase)
                return CharacterAnimationState.Chase;
        }
        return moving ? CharacterAnimationState.Move : CharacterAnimationState.Idle;
    }

    private static string LegacyStateName(CharacterAnimationState action, string direction)
    {
        string stem = action == CharacterAnimationState.Move ||
            action == CharacterAnimationState.Patrol || action == CharacterAnimationState.Chase
            ? "Walk"
            : action == CharacterAnimationState.Windup ? "Attack" : action.ToString();
        return stem + "_" + direction;
    }

    private void OnHealthChanged(Health value)
    {
        if (value != null && !value.IsDead && value.currentHealth < previousHealth)
            hitUntil = Time.time + .32f;
        if (value != null) previousHealth = value.currentHealth;
    }

    private static int ResolveDirection(Vector2 value)
    {
        if (Mathf.Abs(value.x) > Mathf.Abs(value.y)) return value.x < 0f ? 1 : 2;
        return value.y < 0f ? 0 : 3;
    }
}
