using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChronoGuardian), typeof(ChronoGuardianTemporalShield), typeof(EnemyAttackTelegraph))]
public sealed class ChronoGuardianFinale : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PlayerMovement target;
    [SerializeField] private TemporalGhostManager ghostManager;
    [SerializeField, Range(.05f, .95f)] private float triggerHealthRatio = .5f;
    [SerializeField, Min(.1f)] private float hazardWindup = .9f;
    [SerializeField, Min(.1f)] private float hazardCooldown = 2.5f;
    [SerializeField, Min(.1f)] private float hazardRadius = 1.7f;
    [SerializeField, Min(.1f)] private float vulnerabilityDuration = 2.5f;
    [SerializeField, Min(0f)] private float hazardDamage = 25f;
    [SerializeField, Min(.1f)] private float ghostDisruptionDuration = 1.2f;
    private ChronoGuardian guardian;
    private ChronoGuardianTemporalShield shield;
    private EnemyAttackTelegraph telegraph;
    private Health targetHealth;
    private float hazardAt;
    private float nextHazard;
    private float vulnerableUntil;
    private int hazardCount;

    public bool IsFinaleActive { get; private set; }
    public bool IsHazardWindingUp { get; private set; }
    public bool IsVulnerableWindow => Time.time < vulnerableUntil;
    public int HazardCount => hazardCount;

    private void Awake()
    {
        guardian = GetComponent<ChronoGuardian>();
        shield = GetComponent<ChronoGuardianTemporalShield>();
        telegraph = GetComponent<EnemyAttackTelegraph>();
        if (target == null) target = FindAnyObjectByType<PlayerMovement>();
        if (target != null) targetHealth = target.GetComponent<Health>();
        guardian.Health.Changed += OnBossHealthChanged;
    }

    private void OnEnable() => ResetToInitialState();
    private void OnDisable() => ResetToInitialState();

    private void OnBossHealthChanged(Health changed)
    {
        if (!IsFinaleActive && changed.currentHealth <= changed.maxHealth * triggerHealthRatio)
            BeginFinale();
    }

    public void BeginFinale()
    {
        IsFinaleActive = true;
        nextHazard = 0f;
    }

    private void FixedUpdate()
    {
        if (!IsFinaleActive || Time.timeScale <= 0f || guardian.Health.IsDead) return;
        if (IsHazardWindingUp)
        {
            float progress = 1f - Mathf.Clamp01((hazardAt - Time.time) / hazardWindup);
            if (telegraph != null) telegraph.Show(transform.position, hazardRadius, progress);
            if (Time.time < hazardAt) return;
            IsHazardWindingUp = false;
            if (telegraph != null) telegraph.Hide();
            hazardCount++;
            nextHazard = Time.time + hazardCooldown;
            vulnerableUntil = Time.time + vulnerabilityDuration;
            shield.SetFinaleVulnerability(true);
            if (ghostManager != null) ghostManager.DisruptGhosts(ghostDisruptionDuration);
            if (targetHealth != null && Vector2.Distance(transform.position, targetHealth.transform.position) <= hazardRadius)
                targetHealth.TakeDamage(hazardDamage);
            return;
        }
        if (Time.time < nextHazard) return;
        IsHazardWindingUp = true;
        hazardAt = Time.time + hazardWindup;
        if (telegraph != null) telegraph.Show(transform.position, hazardRadius, 0f);
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        IsFinaleActive = false;
        IsHazardWindingUp = false;
        hazardAt = nextHazard = vulnerableUntil = 0f;
        hazardCount = 0;
        if (telegraph != null) telegraph.Hide();
        if (shield != null) shield.ResetToInitialState();
    }

    private void OnDestroy()
    {
        if (guardian != null && guardian.Health != null) guardian.Health.Changed -= OnBossHealthChanged;
    }
}
