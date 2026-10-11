using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyAbilityBrain : MonoBehaviour
{
    private enum Phase { Ready, Windup, Active, Recovery }
    [SerializeField] private EnemyAbilityProfile profile;
    [SerializeField] private Transform target;
    private EnemyAnimationDriver animationDriver;
    private CharacterAnimationEventRelay relay;
    private Health health;
    private EnemyAbilitySpec active;
    private Phase phase;
    private float phaseEnds;
    private float[] readyAt;
    private int activeIndex = -1;
    private bool impactApplied;
    private Vector2 lockedHeading;

    public bool BlocksMovement => phase != Phase.Ready;

    private void Awake()
    {
        if (profile == null) profile = EnemyAbilityProfile.CreateRuntime(gameObject);
        animationDriver = GetComponent<EnemyAnimationDriver>();
        relay = GetComponent<CharacterAnimationEventRelay>();
        health = GetComponent<Health>();
        if (relay != null) relay.Impact += OnImpact;
        if (profile != null) readyAt = new float[profile.Abilities.Length];
    }

    private void OnDestroy()
    {
        if (relay != null) relay.Impact -= OnImpact;
    }

    private void Update()
    {
        // The scene installer may attach the driver after this component's Awake.
        if (animationDriver == null) animationDriver = GetComponent<EnemyAnimationDriver>();
        if (relay == null)
        {
            relay = GetComponent<CharacterAnimationEventRelay>();
            if (relay != null) relay.Impact += OnImpact;
        }
        if (health != null && health.IsDead) return;
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
        if (target == null || profile == null || profile.Abilities == null || profile.Abilities.Length == 0) return;
        if (readyAt == null || readyAt.Length != profile.Abilities.Length) readyAt = new float[profile.Abilities.Length];

        if (phase == Phase.Ready) TrySelectAbility();
        else if (Time.time >= phaseEnds) AdvancePhase();
    }

    private void TrySelectAbility()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        float total = 0f;
        for (int i = 0; i < profile.Abilities.Length; i++)
        {
            EnemyAbilitySpec candidate = profile.Abilities[i];
            if (Time.time < readyAt[i] || distance < candidate.minimumRange || distance > candidate.maximumRange) continue;
            total += Mathf.Max(.01f, candidate.weight);
        }
        if (total <= 0f) return;
        float roll = Random.value * total;
        for (int i = 0; i < profile.Abilities.Length; i++)
        {
            EnemyAbilitySpec candidate = profile.Abilities[i];
            if (Time.time < readyAt[i] || distance < candidate.minimumRange || distance > candidate.maximumRange) continue;
            roll -= Mathf.Max(.01f, candidate.weight);
            if (roll > 0f) continue;
            activeIndex = i;
            active = candidate;
            phase = Phase.Windup;
            phaseEnds = Time.time + active.windup;
            impactApplied = false;
            Vector2 facing = target.position - transform.position;
            lockedHeading = facing.normalized;
            animationDriver?.RequestAction(CharacterAnimationState.Windup, active.windup, facing);
            return;
        }
    }

    private void AdvancePhase()
    {
        switch (phase)
        {
            case Phase.Windup:
                phase = Phase.Active;
                phaseEnds = Time.time + active.activeDuration;
                animationDriver?.RequestAction(active.animationState, active.activeDuration, target.position - transform.position);
                if (active.kind == EnemyAbilityKind.Dash) transform.position = Vector2.MoveTowards(transform.position, target.position, active.maximumRange * .55f);
                break;
            case Phase.Active:
                if (!impactApplied) ApplyImpact();
                phase = Phase.Recovery;
                phaseEnds = Time.time + active.recovery;
                animationDriver?.RequestAction(CharacterAnimationState.Recover, active.recovery, target.position - transform.position);
                break;
            case Phase.Recovery:
                if (activeIndex >= 0) readyAt[activeIndex] = Time.time + active.cooldown;
                activeIndex = -1;
                phase = Phase.Ready;
                break;
        }
    }

    private void OnImpact()
    {
        if (phase == Phase.Active && !impactApplied) ApplyImpact();
    }

    private void ApplyImpact()
    {
        impactApplied = true;
        if (target == null) return;
        if (active.kind == EnemyAbilityKind.Projectile)
        {
            EnemyProjectilePool pool = GetComponent<EnemyProjectilePool>();
            Health victim = target.GetComponent<Health>();
            if (pool != null)
            {
                pool.TryFire(transform.position, lockedHeading, victim, 6f, active.damage, 2f);
                return;
            }
        }
        if (active.kind == EnemyAbilityKind.Melee || active.kind == EnemyAbilityKind.Area ||
            active.kind == EnemyAbilityKind.Dash || active.kind == EnemyAbilityKind.Projectile)
        {
            if (Vector2.Distance(transform.position, target.position) <= active.maximumRange)
            {
                Health victim = target.GetComponent<Health>() ?? target.GetComponentInChildren<Health>();
                if (victim != null && !victim.IsDead) victim.TakeDamage(active.damage);
            }
        }
        if (active.vfxPrefab != null)
        {
            Vector3 position = active.kind == EnemyAbilityKind.Projectile && target != null
                ? target.position : transform.position;
            Instantiate(active.vfxPrefab, position, Quaternion.identity);
        }
    }
}
