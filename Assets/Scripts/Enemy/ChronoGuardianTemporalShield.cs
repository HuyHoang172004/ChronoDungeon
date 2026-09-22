using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health), typeof(EnemyAttackTarget), typeof(CombatCooperationTarget))]
public sealed class ChronoGuardianTemporalShield : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Color shieldColor = new Color(.45f, .2f, .95f, 1f);
    [SerializeField] private Color vulnerableColor = new Color(.25f, .95f, 1f, 1f);
    private SpriteRenderer cachedVisual;
    private Color baseColor = Color.white;

    public bool IsShieldActive { get; private set; } = true;
    public bool IsVulnerable => !IsShieldActive;
    public bool IsFinaleVulnerable { get; private set; }
    public event Action<bool> StateChanged;

    private void Awake()
    {
        cachedVisual = visual != null ? visual : GetComponent<SpriteRenderer>();
        if (cachedVisual != null) baseColor = cachedVisual.color;
        ApplyVisual();
    }

    public bool TryReceiveDamage(bool fromGhost)
    {
        if (IsFinaleVulnerable) return true;
        if (!IsShieldActive) return true;
        if (!fromGhost) return false;
        IsShieldActive = false;
        ApplyVisual();
        StateChanged?.Invoke(true);
        return false;
    }

    public void SetFinaleVulnerability(bool active)
    {
        IsFinaleVulnerable = active;
        if (active) IsShieldActive = false;
        ApplyVisual();
        StateChanged?.Invoke(active || IsVulnerable);
    }

    public void CaptureInitialState() { }
    public void ResetToInitialState()
    {
        IsShieldActive = true;
        IsFinaleVulnerable = false;
        ApplyVisual();
        StateChanged?.Invoke(false);
    }

    private void ApplyVisual()
    {
        if (cachedVisual == null) return;
        cachedVisual.color = IsShieldActive ? shieldColor : vulnerableColor;
    }

    private void OnDestroy()
    {
        if (cachedVisual != null) cachedVisual.color = baseColor;
    }
}
