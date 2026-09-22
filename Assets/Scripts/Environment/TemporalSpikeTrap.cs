using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
public sealed class TemporalSpikeTrap : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField, Min(0.05f)] private float warningDuration = 0.6f;
    [SerializeField, Min(0.05f)] private float activeDuration = 0.25f;
    [SerializeField, Min(0f)] private float cooldown = 1.25f;
    [SerializeField, Min(1f)] private float damage = 20f;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private SpriteRenderer warningIndicator;
    [SerializeField] private Color idleColor = new Color(.24f, .3f, .38f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, .65f, .1f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, .15f, .12f, 1f);
    private Collider2D area;
    private float stateUntil;
    private float nextAvailable;
    private bool damagedThisPulse;

    public bool IsWindingUp => stateUntil > 0f && Time.time < stateUntil && !damagedThisPulse;
    public bool IsActive => damagedThisPulse && Time.time < stateUntil;
    public float WarningDuration => warningDuration;

    private void Awake()
    {
        area = GetComponent<Collider2D>();
        area.isTrigger = true;
        SetVisual(idleColor, false);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (damagedThisPulse && stateUntil > 0f && Time.time >= stateUntil)
        {
            stateUntil = -1f;
            damagedThisPulse = false;
            SetVisual(idleColor, false);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.timeScale <= 0f || Time.time < nextAvailable) return;
        var target = other.GetComponentInParent<Health>();
        if (target == null || !target.CompareTag("Player") || target.IsDead) return;
        if (stateUntil > 0f && !damagedThisPulse && Time.time >= stateUntil)
        {
            target.TakeDamage(damage);
            damagedThisPulse = true;
            stateUntil = Time.time + activeDuration;
            nextAvailable = Time.time + cooldown;
            SetVisual(activeColor, true);
            return;
        }
        if (!IsWindingUp && !IsActive)
        {
            stateUntil = Time.time + warningDuration;
            damagedThisPulse = false;
            SetVisual(warningColor, true);
        }
    }

    private void SetVisual(Color color, bool indicatorOn)
    {
        if (visual != null) visual.color = color;
        if (warningIndicator != null) warningIndicator.enabled = indicatorOn;
    }

    public void CaptureInitialState() => ResetToInitialState();

    public void ResetToInitialState()
    {
        stateUntil = -1f;
        nextAvailable = 0f;
        damagedThisPulse = false;
        SetVisual(idleColor, false);
    }
}
